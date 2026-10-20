import { apiClient } from "./client";
import type { PaginatedResult, RecipeFilters, RecipeListDto, RecipeSearchFilters } from "@/types/custom-api";
import type { RecipeApiSummaryDto } from "@/types/api";

export function normalizeRecipe(recipe: RecipeApiSummaryDto): RecipeListDto {
  const difficulty = ({1: "Easy", 2: "Medium", 3: "Hard", 4: "Expert"} as const)[recipe.difficulty];
  const status = ({0: "Draft", 1: "Published", 2: "Archived"} as const)[recipe.status];
  return { ...recipe, difficulty, status };
}
export function normalizePage(page: PaginatedResult<RecipeApiSummaryDto>): PaginatedResult<RecipeListDto> {
  return { ...page, items: page.items.map(normalizeRecipe) };
}
export const recipeApi = {
  getList: (filters: RecipeFilters = {}) => apiClient.get<PaginatedResult<RecipeApiSummaryDto>>("/api/v1/recipes", { params: filters }).then(({data}) => normalizePage(data)),
  search: (filters: RecipeSearchFilters) => apiClient.get<PaginatedResult<RecipeApiSummaryDto>>("/api/v1/recipes/search", { params: filters }).then(({data}) => normalizePage(data)),
};
