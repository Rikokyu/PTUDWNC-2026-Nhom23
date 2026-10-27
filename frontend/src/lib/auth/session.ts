import { useAuthStore } from "@/store/useAuthStore";

export const AUTH_CHANGED = "culinary:auth-changed";

export function clearAuthSession(): void {
  useAuthStore.getState().clearSession();
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("accessTokenExpiresAt");
  window.dispatchEvent(new Event(AUTH_CHANGED));
}

export function safeReturnPath(value: string | null): string {
  // Only return to known dashboard routes; never redirect to an external URL.
  if (value && /^\/dashboard(?:\/|$|\?)/.test(value) && !/[\\\s]/.test(value)) {
    return value;
  }
  return "/dashboard";
}
