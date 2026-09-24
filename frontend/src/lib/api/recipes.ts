import apiClient from "./axios";
import type {
  PaginatedResult,
  RecipeDto,
  RecipeFilters,
  RecipeListDto,
} from "@/types/api";

export const recipeApi = {
  getList: (filters: RecipeFilters = {}) =>
    apiClient
      .get<
        PaginatedResult<RecipeListDto>
      >("/api/v1/recipes", { params: filters })
      .then((response) => response.data),
  getBySlug: (slug: string) =>
    apiClient
      .get<RecipeDto>(`/api/v1/recipes/${slug}`)
      .then((response) => response.data),
};
