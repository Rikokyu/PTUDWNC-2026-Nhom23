export type Difficulty = "Easy" | "Medium" | "Hard" | "Expert";
export type RecipeStatus = "Draft" | "Published" | "Archived";

export interface RecipeListDto {
  id: string;
  title: string;
  slug: string;
  thumbnailUrl: string | null;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: Difficulty;
  categoryName: string;
  categoryId: string;
  status: RecipeStatus;
  createdAt: string;
}

export interface RecipeDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  instructions: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: Difficulty;
  status: RecipeStatus;
  createdAt: string;
  category: RecipeCategoryDto;
  author: RecipeAuthorDto | null;
  ingredients: RecipeIngredientDto[];
  steps: RecipeStepDto[];
  images: RecipeImageDto[];
  nutrition: RecipeNutritionDto | null;
}

export interface RecipeCategoryDto {
  id: string;
  name: string;
  slug: string;
}

export interface RecipeAuthorDto {
  id: string;
  displayName: string;
  email: string;
}

export interface RecipeIngredientDto {
  id: string;
  name: string;
  quantity: string | null;
  unit: string | null;
  notes: string | null;
  orderIndex: number;
}

export interface RecipeStepDto {
  id: string;
  stepNumber: number;
  title: string;
  description: string;
  timerMinutes: number | null;
  imageUrl: string | null;
}

export interface RecipeImageDto {
  id: string;
  originalUrl: string;
  mediumUrl: string | null;
  thumbnailUrl: string | null;
  altText: string | null;
  isPrimary: boolean;
  orderIndex: number;
}

export interface RecipeNutritionDto {
  calories: number | null;
  protein: number | null;
  carbs: number | null;
  fat: number | null;
  fiber: number | null;
  sodium: number | null;
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
  difficulty?: Difficulty;
  maxCookTime?: number;
  sort?: string;
}

export interface RecipeSearchFilters {
  q: string;
  page?: number;
  pageSize?: number;
  sort?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}
