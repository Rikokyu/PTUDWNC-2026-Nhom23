import apiClient from "./axios";
import type {
  PaginatedResult,
  RecipeApiDetailDto,
  RecipeApiSummaryDto,
  RecipeFilters,
} from "@/types/api";

export const recipeApi = {
  getList: async (
    filters: RecipeFilters = {},
  ): Promise<PaginatedResult<RecipeApiSummaryDto>> => {
    const response = await apiClient.get<PaginatedResult<RecipeApiSummaryDto>>(
      "/api/v1/recipes",
      {
        params: filters,
      },
    );
    return response.data;
  },
  getBySlug: async (slug: string): Promise<RecipeApiDetailDto> => {
    const response = await apiClient.get<RecipeApiDetailDto>(
      `/api/v1/recipes/${encodeURIComponent(slug)}`,
    );
    return response.data;
  },
};
