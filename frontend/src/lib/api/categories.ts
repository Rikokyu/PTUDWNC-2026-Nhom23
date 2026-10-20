import { apiClient } from "@/lib/api/client";
import type { CategoryDetailDto, CategoryDto } from "@/types/api";

export const categoryApi = {
  getList: () =>
    apiClient.get<CategoryDto[]>("/api/v1/categories").then((response) => response.data),
  getBySlug: (slug: string, page = 1, pageSize = 9) =>
    apiClient
      .get<CategoryDetailDto>(`/api/v1/categories/${encodeURIComponent(slug)}`, {
        params: { page, pageSize },
      })
      .then((response) => response.data),
};
