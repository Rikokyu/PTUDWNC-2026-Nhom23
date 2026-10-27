import apiClient from "./axios";
import type { CategoryApiDto, CategoryDetailApiDto } from "@/types/api";

export const categoryApi = {
  getAll: async (): Promise<CategoryApiDto[]> => {
    const response = await apiClient.get<CategoryApiDto[]>("/api/v1/categories");
    return response.data;
  },
  getBySlug: async (slug: string): Promise<CategoryDetailApiDto> => {
    const response = await apiClient.get<CategoryDetailApiDto>(
      `/api/v1/categories/${encodeURIComponent(slug)}`,
    );
    return response.data;
  },
};
