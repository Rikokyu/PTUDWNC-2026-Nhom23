import { apiClient, apiRequest } from "@/lib/api/client";

export type AuthUser = {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  roles: string[];
  emailConfirmed: boolean;
};

type TokenResponse = {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
};

type LoginRequest = {
  email: string;
  password: string;
};

type RegisterRequest = {
  email: string;
  displayName: string;
  password: string;
};

type RegisterResponse = {
  userId: string;
  email: string;
  displayName: string;
};

let accessToken: string | null = null;
let refreshToken: string | null = null;

function setTokens(access: string | null, refresh: string | null) {
  accessToken = access;
  refreshToken = refresh;
  if (access) {
    apiClient.defaults.headers.common.Authorization = `Bearer ${access}`;
  } else {
    delete apiClient.defaults.headers.common.Authorization;
  }
}

export const authApi = {
  async login(credentials: LoginRequest): Promise<AuthUser> {
    const tokens = await apiRequest<TokenResponse>({
      method: "POST",
      url: "/api/v1/auth/login",
      data: credentials,
    });

    setTokens(tokens.accessToken, tokens.refreshToken);
    try {
      return await apiRequest<AuthUser>({
        method: "GET",
        url: "/api/v1/auth/me",
      });
    } catch (error) {
      setTokens(null, null);
      throw error;
    }
  },

  register(details: RegisterRequest) {
    return apiRequest<RegisterResponse>({
      method: "POST",
      url: "/api/v1/auth/register",
      data: details,
    });
  },

  async logout() {
    try {
      if (accessToken) {
        await apiRequest<void>({
          method: "POST",
          url: "/api/v1/auth/logout",
          data: { refreshToken },
        });
      }
    } finally {
      setTokens(null, null);
    }
  },
};
