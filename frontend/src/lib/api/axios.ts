import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "@/store/useAuthStore";
import { clearAuthSession } from "@/lib/auth/session";
import { AUTH_CHANGED } from "@/lib/auth/session";
import type { AuthSessionDto } from "@/types/api";

const apiClient = axios.create({
  baseURL:
    process.env.NEXT_PUBLIC_API_BASE_URL ??
    process.env.NEXT_PUBLIC_API_URL ??
    "",
  timeout: 10000,
  headers: { "Content-Type": "application/json" },
});

export function resolveApiAssetUrl(
  fileUrl: string | null | undefined,
): string | undefined {
  if (!fileUrl) return undefined;
  // Curated dish photos ship with the frontend, even when the API has its own host.
  if (fileUrl.startsWith("/images/dishes/")) return fileUrl;
  try {
    return new URL(fileUrl, apiClient.defaults.baseURL).toString();
  } catch {
    return fileUrl;
  }
}

apiClient.interceptors.request.use((config) => {
  // Zustand is initialized empty after a full page reload. Fall back to the
  // browser-persisted token so the first authenticated request (e.g. /me)
  // carries the existing session instead of looking like an anonymous user.
  const accessToken =
    useAuthStore.getState().accessToken ??
    (typeof window !== "undefined" ? localStorage.getItem("accessToken") : null);
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

type RetryConfig = InternalAxiosRequestConfig & { authRetried?: boolean };
let pendingRefresh: Promise<string> | null = null;

function refreshAccessToken(): Promise<string> {
  if (pendingRefresh) return pendingRefresh;
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) return Promise.reject(new Error("Session expired"));
  pendingRefresh = (async () => {
    const rotate = async () => {
      // Another tab may have completed rotation while this tab waited for the lock.
      const latestRefresh = localStorage.getItem("refreshToken");
      if (latestRefresh !== refreshToken) {
        const latestAccess = localStorage.getItem("accessToken");
        if (latestRefresh && latestAccess) return latestAccess;
        throw new Error("Session ended");
      }
      const { data } = await axios.post<AuthSessionDto>("/api/v1/auth/refresh",
        { refreshToken }, { baseURL: apiClient.defaults.baseURL, timeout: 10000 });
      // Do not restore a session the person logged out of during this request.
      if (localStorage.getItem("refreshToken") !== refreshToken) throw new Error("Session changed");
      localStorage.setItem("refreshToken", data.refreshToken);
      localStorage.setItem("accessTokenExpiresAt", data.accessTokenExpiresAt);
      useAuthStore.getState().setSession(data.accessToken, data.user);
      window.dispatchEvent(new Event(AUTH_CHANGED));
      return data.accessToken;
    };
    return navigator.locks ? navigator.locks.request("culinary-session-refresh", rotate) : rotate();
  })().finally(() => { pendingRefresh = null; });
  return pendingRefresh;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config as RetryConfig | undefined;
    if (typeof window === "undefined" || error.response?.status !== 401 || !config
      || /\/auth\/(login|register|refresh|google|logout)(?:\?|$)/.test(config.url ?? "")) {
      return Promise.reject(error);
    }
    if (config.authRetried) {
      clearAuthSession();
      return Promise.reject(error);
    }
    config.authRetried = true;
    try {
      const currentAccess = localStorage.getItem("accessToken");
      const nextAccess = currentAccess && config.headers.Authorization !== `Bearer ${currentAccess}`
        ? currentAccess : await refreshAccessToken();
      config.headers.Authorization = `Bearer ${nextAccess}`;
      return apiClient(config);
    } catch {
      clearAuthSession();
      return Promise.reject(error);
    }
  },
);

export default apiClient;
