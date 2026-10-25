import apiClient from "./axios";
import type {
  AuthSessionDto,
  AuthUserDto,
  UpdateProfileRequest,
  UserProfileDto,
} from "@/types/api";

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
  ): Promise<AuthUserDto> => {
    const response = await apiClient.post<AuthUserDto>(
      "/api/v1/auth/register",
      { displayName, email, password },
    );
    return response.data;
  },
  logout: async (): Promise<void> => {
    await apiClient.post("/api/v1/auth/logout");
  },
  getMe: async (): Promise<UserProfileDto> => {
    const response = await apiClient.get<UserProfileDto>("/api/v1/auth/me");
    return response.data;
  },
  updateProfile: async (
    request: UpdateProfileRequest,
  ): Promise<UserProfileDto> => {
    const response = await apiClient.patch<UserProfileDto>(
      "/api/v1/auth/me",
      request,
    );
    return response.data;
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
  requestPasswordReset: async (email: string): Promise<void> => {
    await apiClient.post("/api/v1/auth/forgot-password", { email });
  },
  resetPassword: async (token: string, newPassword: string): Promise<void> => {
    await apiClient.post("/api/v1/auth/reset-password", { token, newPassword });
  },
};