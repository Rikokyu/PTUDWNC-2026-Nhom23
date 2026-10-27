import { apiRequest } from "@/lib/api/client";
import { normalizePage } from "./custom-recipes";
import type { RecipeApiSummaryDto } from "@/types/api";
import type { PaginatedResult, RecipeSearchFilters } from "@/types/custom-api";

export async function searchRecipes(filters: RecipeSearchFilters) {
  const params = new URLSearchParams();

  if (filters.categoryId) params.set("categoryId", filters.categoryId);
  if (filters.difficulty) params.set("difficulty", filters.difficulty);
  if (filters.maxCookTime !== undefined) params.set("maxCookTime", String(filters.maxCookTime));
  if (filters.q) params.set("q", filters.q);
  if (filters.page) params.set("page", String(filters.page));
  if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
  if (filters.sort) params.set("sort", filters.sort);
  if (filters.sortBy) params.set("sortBy", filters.sortBy);
  if (filters.sortOrder) params.set("sortOrder", filters.sortOrder);

  return apiRequest<PaginatedResult<RecipeApiSummaryDto>>({
    method: "GET",
    url: "/api/v1/recipes/search",
    params: Object.fromEntries(params.entries()),
  }).then(normalizePage);
}
