"use client";

import { useQuery } from "@tanstack/react-query";
import { recipeApi } from "@/lib/api/recipes";
import type { RecipeFilters } from "@/types/api";

export const recipeKeys = {
  all: () => ["recipes"] as const,
  lists: () => [...recipeKeys.all(), "list"] as const,
  list: (filters: RecipeFilters) => [...recipeKeys.lists(), filters] as const,
  details: () => [...recipeKeys.all(), "detail"] as const,
  detail: (slug: string) => [...recipeKeys.details(), slug] as const,
  mine: (filters: RecipeFilters) => [...recipeKeys.all(), "mine", filters] as const,
  admin: (filters: RecipeFilters) => [...recipeKeys.all(), "admin", filters] as const,
};

export function useRecipes(filters: RecipeFilters = {}) {
  return useQuery({
    queryKey: recipeKeys.list(filters),
    queryFn: () => recipeApi.getList(filters),
  });
}

export function useRecipe(slug: string) {
  return useQuery({
    queryKey: recipeKeys.detail(slug),
    queryFn: () => recipeApi.getBySlug(slug),
    enabled: Boolean(slug),
  });
}

export function useMyRecipes(filters: RecipeFilters = {}) {
  return useQuery({
    queryKey: recipeKeys.mine(filters),
    queryFn: () => recipeApi.getMine(filters),
  });
}

export function useAdminRecipes(filters: RecipeFilters = {}) {
  return useQuery({
    queryKey: recipeKeys.admin(filters),
    queryFn: () => recipeApi.getAdmin(filters),
  });
}
