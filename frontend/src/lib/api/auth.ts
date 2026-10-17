import apiClient from "./axios";
import type { AuthSessionDto, AuthUserDto } from "@/types/api";

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
};