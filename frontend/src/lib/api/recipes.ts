import apiClient from "./axios";
import type {
  CreateRecipeRequest,
  PaginatedResult,
  RecipeApiDetailDto,
  RecipeApiImageDto,
  RecipeApiSummaryDto,
  RecipeIngredientRequest,
  RecipeFilters,
  RecipeStepRequest,
} from "@/types/api";

// Existing filter controls remain compatible; HTTP uses the agreed explicit fields.
export function recipeListParams(filters: RecipeFilters): Omit<RecipeFilters, "sort"> {
  const { sort, ...params } = filters;
  if (!params.sortBy) {
    params.sortBy = (sort?.replace(/^-/, "") ?? "createdAt") as NonNullable<RecipeFilters["sortBy"]>;
  }
  params.sortOrder ??= sort ? (sort.startsWith("-") ? "desc" : "asc") : "desc";
  return params;
}

export const recipeApi = {
  getList: async (
    filters: RecipeFilters = {},
  ): Promise<PaginatedResult<RecipeApiSummaryDto>> => {
    const response = await apiClient.get<PaginatedResult<RecipeApiSummaryDto>>(
      "/api/v1/recipes",
      {
        params: recipeListParams(filters),
      },
    );
    return response.data;
  },
  getMine: async (
    filters: RecipeFilters = {},
  ): Promise<PaginatedResult<RecipeApiSummaryDto>> => {
    const response = await apiClient.get<PaginatedResult<RecipeApiSummaryDto>>(
      "/api/v1/me/recipes",
      { params: recipeListParams(filters) },
    );
    return response.data;
  },
  getAdmin: async (
    filters: RecipeFilters = {},
  ): Promise<PaginatedResult<RecipeApiSummaryDto>> => {
    const response = await apiClient.get<PaginatedResult<RecipeApiSummaryDto>>(
      "/api/v1/admin/recipes",
      { params: recipeListParams(filters) },
    );
    return response.data;
  },
  search: async (
    searchTerm: string,
    page = 1,
    pageSize = 10,
    filters: Pick<RecipeFilters, "categoryId" | "difficulty" | "maxCookTime" | "minServings"> = {},
  ): Promise<PaginatedResult<RecipeApiSummaryDto>> => {
    const response = await apiClient.get<PaginatedResult<RecipeApiSummaryDto>>(
      "/api/v1/recipes/search",
      { params: { ...filters, q: searchTerm, page, pageSize } },
    );
    return response.data;
  },
  getBySlug: async (slug: string): Promise<RecipeApiDetailDto> => {
    const response = await apiClient.get<RecipeApiDetailDto>(
      `/api/v1/recipes/${encodeURIComponent(slug)}`,
    );
    return response.data;
  },
  create: async (request: CreateRecipeRequest): Promise<RecipeApiDetailDto> => {
    const response = await apiClient.post<{ data: RecipeApiDetailDto }>(
      "/api/v1/recipes",
      request,
    );
    return response.data.data;
  },
  getManagedById: async (
    recipeId: string,
    isAdmin: boolean,
  ): Promise<RecipeApiDetailDto> => {
    const endpoint = isAdmin
      ? "/api/v1/admin/recipes"
      : "/api/v1/me/recipes";
    let page = 1;
    let slug: string | undefined;
    let totalPages = 1;

    while (page <= totalPages) {
      const response = await apiClient.get<
        PaginatedResult<RecipeApiSummaryDto>
      >(endpoint, { params: { page, pageSize: 50 } });
      totalPages = response.data.totalPages;
      slug = response.data.items.find((recipe) => recipe.id === recipeId)?.slug;
      if (slug) break;
      page++;
    }

    if (!slug) {
      throw new Error("Không tìm thấy công thức trong danh sách được phép quản lý.");
    }
    return recipeApi.getBySlug(slug);
  },
  getManagedList: async (
    page: number,
    pageSize: number,
    isAdmin: boolean,
  ): Promise<PaginatedResult<RecipeApiSummaryDto>> => {
    const endpoint = isAdmin
      ? "/api/v1/admin/recipes"
      : "/api/v1/me/recipes";
    const response = await apiClient.get<
      PaginatedResult<RecipeApiSummaryDto>
    >(endpoint, { params: { page, pageSize } });
    return response.data;
  },
  addIngredient: async (
    recipeId: string,
    request: RecipeIngredientRequest,
  ): Promise<void> => {
    await apiClient.post(`/api/v1/recipes/${recipeId}/ingredients`, request);
  },
  updateIngredient: async (
    recipeId: string,
    ingredientId: string,
    request: RecipeIngredientRequest,
  ): Promise<void> => {
    await apiClient.put(
      `/api/v1/recipes/${recipeId}/ingredients/${ingredientId}`,
      request,
    );
  },
  deleteIngredient: async (
    recipeId: string,
    ingredientId: string,
  ): Promise<void> => {
    await apiClient.delete(
      `/api/v1/recipes/${recipeId}/ingredients/${ingredientId}`,
    );
  },
  addStep: async (
    recipeId: string,
    request: RecipeStepRequest,
  ): Promise<void> => {
    await apiClient.post(`/api/v1/recipes/${recipeId}/steps`, request);
  },
  updateStep: async (
    recipeId: string,
    stepId: string,
    request: RecipeStepRequest,
  ): Promise<void> => {
    await apiClient.put(
      `/api/v1/recipes/${recipeId}/steps/${stepId}`,
      request,
    );
  },
  deleteStep: async (recipeId: string, stepId: string): Promise<void> => {
    await apiClient.delete(`/api/v1/recipes/${recipeId}/steps/${stepId}`);
  },
  reorderSteps: async (
    recipeId: string,
    stepIds: string[],
  ): Promise<void> => {
    await apiClient.put(`/api/v1/recipes/${recipeId}/steps/order`, {
      stepIds,
    });
  },
  uploadImage: async (
    recipeId: string,
    file: File,
    altText: string,
  ): Promise<RecipeApiImageDto> => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("altText", altText);
    const response = await apiClient.post<{ data: RecipeApiImageDto }>(
      `/api/v1/recipes/${recipeId}/images`,
      formData,
      { headers: { "Content-Type": undefined } },
    );
    return response.data.data;
  },
  setPrimaryImage: async (
    recipeId: string,
    imageId: string,
  ): Promise<void> => {
    await apiClient.patch(
      `/api/v1/recipes/${recipeId}/images/${imageId}/primary`,
    );
  },
  deleteImage: async (recipeId: string, imageId: string): Promise<void> => {
    await apiClient.delete(`/api/v1/recipes/${recipeId}/images/${imageId}`);
  },
  deleteRecipe: async (recipeId: string): Promise<void> => {
    await apiClient.delete(`/api/v1/recipes/${recipeId}`);
  },
  setStatus: async (
    recipeId: string,
    action: "publish" | "unpublish" | "archive",
  ): Promise<RecipeApiDetailDto> => {
    const response = await apiClient.patch<{ data: RecipeApiDetailDto }>(
      `/api/v1/recipes/${recipeId}/${action}`,
    );
    return response.data.data;
  },
};
