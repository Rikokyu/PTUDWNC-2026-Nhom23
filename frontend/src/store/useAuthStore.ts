"use client";

import { create } from "zustand";

export interface AuthUser {
  id: string;
  email: string;
  displayName: string;
  role: string;
}

interface AuthState {
  accessToken: string | null;
  user: AuthUser | null;
  setSession: (accessToken: string, user: AuthUser) => void;
  clearSession: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  user: null,
  setSession: (accessToken, user) => {
    if (typeof window !== "undefined") {
      localStorage.setItem("accessToken", accessToken);
      localStorage.setItem("authUser", JSON.stringify(user));
    }
    set({ accessToken, user });
  },
  clearSession: () => {
    if (typeof window !== "undefined") {
      for (const key of ["accessToken", "refreshToken", "authUser", "accessTokenExpiresAt"]) {
        localStorage.removeItem(key);
      }
    }
    set({ accessToken: null, user: null });
  },
}));
