"use client";

import { createContext, useContext, useEffect, useState, useCallback } from "react";
import { usePathname } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import apiClient from "@/lib/api/axios";
import type { AuthUserDto } from "@/types/api";
import { AUTH_CHANGED, clearAuthSession } from "@/lib/auth/session";
import { useAuthStore } from "@/store/useAuthStore";

type AuthState = {
  user: AuthUserDto | null;
  status: "loading" | "authenticated" | "guest" | "error";
};
const AuthContext = createContext<(AuthState & {
  retry: () => void;
  logout: () => Promise<void>;
}) | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({ user: null, status: "loading" });
  const [revision, setRevision] = useState(0);
  const accessToken = useAuthStore((store) => store.accessToken);
  const setSession = useAuthStore((store) => store.setSession);
  const pathname = usePathname();
  const queryClient = useQueryClient();
  const retry = useCallback(() => setRevision((value) => value + 1), []);

  useEffect(() => {
    const changed = () => {
      setState({ user: null, status: "loading" });
      queryClient.clear();
      retry();
    };
    const storageChanged = (event: StorageEvent) => {
      if (event.key === "accessToken" || event.key === null) changed();
    };
    window.addEventListener(AUTH_CHANGED, changed);
    window.addEventListener("storage", storageChanged);
    window.addEventListener("focus", retry);
    return () => {
      window.removeEventListener(AUTH_CHANGED, changed);
      window.removeEventListener("storage", storageChanged);
      window.removeEventListener("focus", retry);
    };
  }, [retry, queryClient]);

  useEffect(() => {
    let token = accessToken;
    if (!token) {
      token = localStorage.getItem("accessToken");
      const storedUser = localStorage.getItem("authUser");
      if (token && storedUser) {
        try {
          setSession(token, JSON.parse(storedUser) as AuthUserDto);
          return;
        } catch {
          clearAuthSession();
        }
      }
    }
    if (!token) {
      setState({ user: null, status: "guest" });
      return;
    }
    const controller = new AbortController();
    let expiryTimer: ReturnType<typeof setTimeout> | undefined;
    setState({ user: null, status: "loading" });
    // Roles are loaded from the authenticated API, never trusted from localStorage.
    apiClient.get<AuthUserDto>("/api/v1/auth/me", { signal: controller.signal })
      .then(({ data }) => {
        if (controller.signal.aborted || localStorage.getItem("accessToken") !== token) return;
        setState({ user: data, status: "authenticated" });
        setSession(token, data);
        const expiry = Date.parse(localStorage.getItem("accessTokenExpiresAt") ?? "");
        if (Number.isFinite(expiry)) {
          expiryTimer = setTimeout(clearAuthSession, Math.max(0, expiry - Date.now()));
        }
      })
      .catch((error) => {
        if (controller.signal.aborted) return;
        setState({
          user: null,
          status: axios.isAxiosError(error) && error.response?.status === 401 ? "guest" : "error",
        });
      });
    return () => {
      controller.abort();
      clearTimeout(expiryTimer);
    };
  }, [revision, pathname, accessToken, setSession]);

  const logout = async () => {
    const token = localStorage.getItem("accessToken");
    clearAuthSession();
    setState({ user: null, status: "guest" });
    if (token) {
      try {
        await apiClient.post("/api/v1/auth/logout", undefined, {
          headers: { Authorization: `Bearer ${token}` },
        });
      } catch {
        // Local logout must still succeed if the server is unavailable.
      }
    }
  };

  return <AuthContext.Provider value={{ ...state, retry, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth requires AuthProvider");
  return value;
}
