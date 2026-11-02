import apiClient from "./axios";
import type { AdminUserDto, PaginatedResult, UserRole } from "@/types/api";

export interface AdminUserFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  role?: UserRole | "";
}

export const adminUserApi = {
  getAll: async (
    filters: AdminUserFilters = {},
  ): Promise<PaginatedResult<AdminUserDto>> => {
    const response = await apiClient.get<PaginatedResult<AdminUserDto>>(
      "/api/v1/admin/users",
      { params: filters },
    );
    return response.data;
  },

  setActive: async (userId: string, isActive: boolean): Promise<AdminUserDto> => {
    const response = await apiClient.patch<AdminUserDto>(
      `/api/v1/admin/users/${encodeURIComponent(userId)}/status`,
      { isActive },
    );
    return response.data;
  },

  setRole: async (userId: string, role: UserRole): Promise<AdminUserDto> => {
    const response = await apiClient.patch<AdminUserDto>(
      `/api/v1/admin/users/${encodeURIComponent(userId)}/role`,
      { role },
    );
    return response.data;
  },
};
