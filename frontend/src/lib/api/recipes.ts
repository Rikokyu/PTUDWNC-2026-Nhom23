import apiClient from "./axios";
import type {
  PaginatedResult,
  RecipeDto,
  RecipeFilters,
  RecipeListDto,
} from "@/types/api";
import { MOCK_RECIPES, ExtendedRecipeDto } from "@/lib/mockData";

export const recipeApi = {
  getList: async (filters: RecipeFilters = {}): Promise<PaginatedResult<ExtendedRecipeDto>> => {
    try {
      const response = await apiClient.get<PaginatedResult<ExtendedRecipeDto>>("/api/v1/recipes", {
        params: filters,
      });
      return response.data;
    } catch {
      // Fallback mock data when backend is not connected
      return {
        items: MOCK_RECIPES,
        totalCount: MOCK_RECIPES.length,
        page: filters.page || 1,
        pageSize: filters.pageSize || 10,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      };
    }
  },
  getBySlug: async (slug: string): Promise<ExtendedRecipeDto> => {
    try {
      const response = await apiClient.get<ExtendedRecipeDto>(`/api/v1/recipes/${slug}`);
      return response.data;
    } catch {
      return MOCK_RECIPES.find((r) => r.slug === slug) || MOCK_RECIPES[0];
    }
  },
};
