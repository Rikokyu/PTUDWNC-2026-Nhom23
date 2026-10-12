import apiClient from "./axios";

export interface RegisterRequest {
  fullName: string;
  email: string;
  userName: string;
  password: string;
}

export interface AuthUser {
  id: string;
  fullName: string;
  email: string;
  userName: string;
  avatarUrl?: string | null;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AuthUser;
}

export interface ValidationProblem {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export async function registerAccount(
  request: RegisterRequest
): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>(
    "/api/v1/auth/register",
    request
  );
  return response.data;
}

export function saveAuthSession(session: AuthResponse): void {
  localStorage.setItem("accessToken", session.accessToken);
  localStorage.setItem("refreshToken", session.refreshToken);
  localStorage.setItem("authUser", JSON.stringify(session.user));
}
