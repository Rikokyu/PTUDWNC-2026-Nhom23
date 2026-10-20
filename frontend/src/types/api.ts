export type Difficulty = "Easy" | "Medium" | "Hard";

export type ApiDifficultyLevel = 1 | 2 | 3 | 4;
export type ApiRecipeStatus = 0 | 1 | 2;

export interface AuthUserDto {
  id: string;
  email: string;
  displayName: string;
  role: string;
}

export interface AuthSessionDto {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  user: AuthUserDto;
}

export interface CreateRecipeRequest {
  title: string;
  description: string;
  categoryId: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: ApiDifficultyLevel;
  instructions: string;
  ingredients: Array<{
    name: string;
    quantity: string | null;
    unit: string | null;
    notes: string | null;
  }>;
  steps: Array<{
    title: string;
    description: string;
    timerMinutes: number | null;
    imageUrl: string | null;
  }>;
}

export interface RecipeApiSummaryDto {
  id: string;
  title: string;
  slug: string;
  thumbnailUrl: string | null;
  categoryName: string;
  categoryId: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: ApiDifficultyLevel;
  status: ApiRecipeStatus;
  createdAt: string;
}

export interface RecipeApiDetailDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  instructions: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: ApiDifficultyLevel;
  status: ApiRecipeStatus;
  createdAt: string;
  rowVersion: number;
  category: RecipeApiCategoryDto;
  author: RecipeApiAuthorDto | null;
  ingredients: RecipeApiIngredientDto[];
  steps: RecipeApiStepDto[];
  images: RecipeApiImageDto[];
  nutrition: RecipeApiNutritionDto | null;
}

export interface RecipeApiCategoryDto {
  id: string;
  name: string;
  slug: string;
}

export interface RecipeApiAuthorDto {
  id: string;
  displayName: string;
  email: string;
}

export interface RecipeApiIngredientDto {
  id: string;
  name: string;
  quantity: string | null;
  unit: string | null;
  notes: string | null;
  orderIndex: number;
}

export interface RecipeApiStepDto {
  id: string;
  stepNumber: number;
  title: string;
  description: string;
  timerMinutes: number | null;
  imageUrl: string | null;
}

export interface RecipeApiImageDto {
  id: string;
  originalUrl: string;
  mediumUrl: string | null;
  thumbnailUrl: string | null;
  altText: string | null;
  isPrimary: boolean;
  orderIndex: number;
}

export interface RecipeApiNutritionDto {
  calories: number | null;
  protein: number | null;
  carbs: number | null;
  fat: number | null;
  fiber: number | null;
  sodium: number | null;
}

export interface CategoryApiDto {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
  recipeCount: number;
}

export interface CategoryDetailApiDto {
  category: CategoryApiDto;
  recipes: PaginatedResult<RecipeApiSummaryDto>;
}

export interface RecipeListDto {
  id: string;
  title: string;
  slug: string;
  description?: string;
  primaryImageUrl?: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: Difficulty;
  categoryName: string;
  authorName: string;
  createdAt: string;
}

export interface RecipeDto extends RecipeListDto {
  ingredients: RecipeIngredientDto[];
  steps: RecipeStepDto[];
  likeCount: number;
  isLiked: boolean;
}

export interface RecipeIngredientDto {
  id: string;
  name: string;
  quantity: number;
  unit: string;
  notes?: string;
}
export interface RecipeStepDto {
  id: string;
  stepNumber: number;
  description: string;
  durationMinutes?: number;
}
export interface CommentDto {
  id: string;
  content: string;
  authorName: string;
  authorAvatar?: string;
  createdAt: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface RecipeFilters {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  difficulty?: Difficulty | "Expert";
  maxCookTime?: number;
  sort?: "-createdAt" | "createdAt" | "title" | "cookTime" | "-cookTime";
  search?: string;
}
