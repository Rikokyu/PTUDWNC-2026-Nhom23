import apiClient from "./axios";
import type { CategoryApiDto, CategoryDetailApiDto } from "@/types/api";

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
}

export interface UpdateCategoryRequest {
  name?: string | null;
  description?: string | null;
}

export const categoryApi = {
  getAll: async (): Promise<CategoryApiDto[]> => {
    const response = await apiClient.get<CategoryApiDto[]>("/api/v1/categories");
    return response.data;
  },
  getBySlug: async (
    slug: string,
    page = 1,
    pageSize = 12,
  ): Promise<CategoryDetailApiDto> => {
    const response = await apiClient.get<CategoryDetailApiDto>(
      `/api/v1/categories/${encodeURIComponent(slug)}`,
      { params: { page, pageSize } },
    );
    return response.data;
  },
  create: async (request: CreateCategoryRequest): Promise<CategoryApiDto> => {
    const response = await apiClient.post<CategoryApiDto>(
      "/api/v1/categories",
      request,
    );
    return response.data;
  },
  update: async (
    id: string,
    request: UpdateCategoryRequest,
  ): Promise<CategoryApiDto> => {
    const response = await apiClient.put<CategoryApiDto>(
      `/api/v1/categories/${encodeURIComponent(id)}`,
      request,
    );
    return response.data;
  },
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/v1/categories/${encodeURIComponent(id)}`);
  },
  moveRecipes: async (categoryId: string, recipeIds: string[], targetCategoryId: string): Promise<number> => {
    const response = await apiClient.post<{ affectedCount: number }>(
      `/api/v1/categories/${encodeURIComponent(categoryId)}/recipes/move`,
      { recipeIds, targetCategoryId },
    );
    return response.data.affectedCount;
  },
  deleteRecipes: async (categoryId: string, recipeIds: string[]): Promise<number> => {
    const response = await apiClient.post<{ affectedCount: number }>(
      `/api/v1/categories/${encodeURIComponent(categoryId)}/recipes/delete`,
      { recipeIds },
    );
    return response.data.affectedCount;
  },
};
