import { useAuthStore } from "@/store/useAuthStore";
export { safeReturnPath } from "./return-path";

export const AUTH_CHANGED = "culinary:auth-changed";

export function clearAuthSession(): void {
  useAuthStore.getState().clearSession();
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("accessTokenExpiresAt");
  window.dispatchEvent(new Event(AUTH_CHANGED));
}
