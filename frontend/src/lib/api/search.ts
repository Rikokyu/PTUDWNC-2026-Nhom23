import { apiRequest } from "@/lib/api/client";
import type { PaginatedResult, RecipeListDto, RecipeSearchFilters } from "@/types/api";

export async function searchRecipes(filters: RecipeSearchFilters) {
  const params = new URLSearchParams();

  if (filters.q) params.set("q", filters.q);
  if (filters.page) params.set("page", String(filters.page));
  if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
  if (filters.sort) params.set("sort", filters.sort);
  if (filters.sortBy) params.set("sortBy", filters.sortBy);
  if (filters.sortOrder) params.set("sortOrder", filters.sortOrder);

  return apiRequest<PaginatedResult<RecipeListDto>>({
    method: "GET",
    url: "/api/v1/recipes/search",
    params: Object.fromEntries(params.entries()),
  });
}
