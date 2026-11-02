import axios from "axios";
import { useAuthStore } from "@/store/useAuthStore";
import { clearAuthSession } from "@/lib/auth/session";

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

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && useAuthStore.getState().accessToken) {
      clearAuthSession();
    }
    return Promise.reject(error);
  },
);

export default apiClient;
