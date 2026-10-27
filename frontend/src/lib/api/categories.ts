import apiClient from "./axios";
import type { CategoryApiDto, CategoryDetailApiDto } from "@/types/api";

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
};
