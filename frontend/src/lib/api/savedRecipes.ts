import apiClient from "./axios";
import type { PaginatedResult, RecipeApiSummaryDto } from "@/types/api";

export const savedRecipesApi = {
  getList: async (page = 1, pageSize = 12) => {
    const response = await apiClient.get<PaginatedResult<RecipeApiSummaryDto>>(
      "/api/v1/saved-recipes",
      { params: { page, pageSize } },
    );
    return response.data;
  },

  getIds: async () => {
    const response = await apiClient.get<string[]>("/api/v1/saved-recipes/ids");
    return response.data;
  },

  save: async (recipeId: string) => {
    await apiClient.post(`/api/v1/saved-recipes/${recipeId}`);
  },

  remove: async (recipeId: string) => {
    await apiClient.delete(`/api/v1/saved-recipes/${recipeId}`);
  },
};
