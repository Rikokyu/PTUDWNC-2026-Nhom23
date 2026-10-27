import apiClient from "./axios";
import type { AuthSessionDto, UserProfileDto } from "@/types/api";
import { useAuthStore } from "@/store/useAuthStore";
import { AUTH_CHANGED } from "@/lib/auth/session";

export const authApi = {
  login: async (email: string, password: string): Promise<AuthSessionDto> => {
    const response = await apiClient.post<AuthSessionDto>(
      "/api/v1/auth/login",
      { email, password },
    );
    return response.data;
  },
  register: async (
    displayName: string,
    email: string,
    password: string,
  ): Promise<AuthSessionDto> => {
    const response = await apiClient.post<AuthSessionDto>(
      "/api/v1/auth/register",
      { displayName, email, password },
    );
    return response.data;
  },
  logout: async (): Promise<void> => {
    await apiClient.post("/api/v1/auth/logout");
  },
  changePassword: async (
    currentPassword: string,
    newPassword: string,
  ): Promise<void> => {
    await apiClient.post("/api/v1/auth/change-password", {
      currentPassword,
      newPassword,
    });
  },
  forgotPassword: async (email: string): Promise<void> => {
    await apiClient.post("/api/v1/auth/forgot-password", { email });
  },
  resetPassword: async (
    email: string,
    token: string,
    newPassword: string,
  ): Promise<void> => {
    await apiClient.post("/api/v1/auth/reset-password", {
      email,
      token,
      newPassword,
    });
  },
  updateProfile: async (profile: {
    displayName: string;
    bio: string | null;
    avatarUrl: string | null;
  }): Promise<UserProfileDto> => {
    const response = await apiClient.put<UserProfileDto>(
      "/api/v1/auth/me",
      profile,
    );
    return response.data;
  },
};

export function saveAuthSession(session: AuthSessionDto): void {
  localStorage.setItem("refreshToken", session.refreshToken);
  localStorage.setItem("accessTokenExpiresAt", session.accessTokenExpiresAt);
  useAuthStore.getState().setSession(session.accessToken, session.user);
  window.dispatchEvent(new Event(AUTH_CHANGED));
}
