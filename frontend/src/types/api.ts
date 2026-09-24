export type Difficulty = "Easy" | "Medium" | "Hard";

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
  difficulty?: Difficulty;
  search?: string;
}
