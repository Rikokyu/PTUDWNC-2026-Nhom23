import axios, { type AxiosError, type AxiosRequestConfig, type AxiosResponse } from "axios";

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  process.env.NEXT_PUBLIC_API_URL ??
  "http://localhost:5000";

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 15000,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

export type ApiError = {
  message: string;
  status?: number;
  details?: Record<string, unknown>;
};

export class ApiRequestError extends Error implements ApiError {
  status?: number;
  details?: Record<string, unknown>;

  constructor(error: ApiError) {
    super(error.message);
    this.name = "ApiRequestError";
    this.status = error.status;
    this.details = error.details;
  }
}

export async function apiRequest<T>(
  config: AxiosRequestConfig,
): Promise<T> {
  try {
    const response: AxiosResponse<T> = await apiClient.request<T>(config);
    return response.data;
  } catch (error) {
    const axiosError = error as AxiosError<{ title?: string; detail?: string; errors?: Record<string, string[]> }>;

    const status = axiosError.response?.status;
    const details = axiosError.response?.data?.errors ?? {};
    const message =
      axiosError.response?.data?.detail ??
      axiosError.response?.data?.title ??
      axiosError.message ??
      "Request failed.";

    throw new ApiRequestError({
      message,
      status,
      details,
    });
  }
}

export function buildSearchParams(filters: Record<string, string | number | undefined>) {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      params.set(key, String(value));
    }
  });

  return params;
}
