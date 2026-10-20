import { normalizePage } from "./custom-recipes";
import type { CategoryDetailApiDto } from "@/types/api";
import { apiClient } from "@/lib/api/client";
import type { CategoryDto } from "@/types/custom-api";

export const categoryApi = {
  getList: () =>
    apiClient.get<CategoryDto[]>("/api/v1/categories").then((response) => response.data),
  getBySlug: (slug: string, page = 1, pageSize = 9) =>
    apiClient
      .get<CategoryDetailApiDto>(`/api/v1/categories/${encodeURIComponent(slug)}`, {
        params: { page, pageSize },
      })
      .then(({data}) => ({ ...data, recipes: normalizePage(data.recipes) })),
};
