"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { savedRecipesApi } from "@/lib/api/savedRecipes";
import { useAuth } from "@/components/auth/AuthProvider";

export const savedRecipeKeys = {
  all: (userId: string) => ["saved-recipes", userId] as const,
  list: (userId: string, page: number, pageSize: number) =>
    [...savedRecipeKeys.all(userId), "list", page, pageSize] as const,
  ids: (userId: string) => [...savedRecipeKeys.all(userId), "ids"] as const,
  count: (recipeId: string) => ["saved-recipes", "count", recipeId] as const,
};

function useSavedRecipeAccess() {
  const auth = useAuth();
  const canManage = auth.status === "authenticated"
    && (auth.user?.role === "Author" || auth.user?.role === "Admin");

  return {
    ...auth,
    canManage,
    userId: canManage ? auth.user!.id : "",
  };
}

export function useSavedRecipeIds() {
  const { canManage, userId } = useSavedRecipeAccess();

  return useQuery({
    queryKey: savedRecipeKeys.ids(userId),
    queryFn: savedRecipesApi.getIds,
    enabled: canManage,
    staleTime: 30_000,
  });
}

export function useSavedRecipes(page: number, pageSize = 12) {
  const { canManage, userId } = useSavedRecipeAccess();

  return useQuery({
    queryKey: savedRecipeKeys.list(userId, page, pageSize),
    queryFn: () => savedRecipesApi.getList(page, pageSize),
    enabled: canManage,
  });
}

export function useRecipeSaveCount(recipeId: string, enabled = true) {
  return useQuery({
    queryKey: savedRecipeKeys.count(recipeId),
    queryFn: () => savedRecipesApi.getCount(recipeId),
    enabled: enabled && Boolean(recipeId),
  });
}

function useSavedRecipeMutation(action: "save" | "remove") {
  const queryClient = useQueryClient();
  const { userId } = useSavedRecipeAccess();
  const idsKey = savedRecipeKeys.ids(userId);

  return useMutation({
    mutationFn: (recipeId: string) =>
      action === "save"
        ? savedRecipesApi.save(recipeId)
        : savedRecipesApi.remove(recipeId),
    onMutate: async (recipeId) => {
      await queryClient.cancelQueries({ queryKey: idsKey });
      const previousIds = queryClient.getQueryData<string[]>(idsKey);
      const countKey = savedRecipeKeys.count(recipeId);
      await queryClient.cancelQueries({ queryKey: countKey });
      const previousCount = queryClient.getQueryData<number>(countKey);
      if (previousCount !== undefined) {
        queryClient.setQueryData(countKey, Math.max(0,
          previousCount + (action === "save" ? 1 : -1)));
      }
      queryClient.setQueryData<string[]>(idsKey, (current = []) =>
        action === "save"
          ? current.includes(recipeId) ? current : [...current, recipeId]
          : current.filter((id) => id !== recipeId),
      );
      return { previousIds, previousCount };
    },
    onError: (_error, recipeId, context) => {
      if (context?.previousIds) {
        queryClient.setQueryData(idsKey, context.previousIds);
      }
      if (context?.previousCount !== undefined) {
        queryClient.setQueryData(savedRecipeKeys.count(recipeId), context.previousCount);
      }
    },
    onSettled: async (_data, _error, recipeId) => {
      await queryClient.invalidateQueries({ queryKey: savedRecipeKeys.all(userId) });
      await queryClient.invalidateQueries({ queryKey: savedRecipeKeys.count(recipeId) });
    },
  });
}

export function useSaveRecipe() {
  return useSavedRecipeMutation("save");
}

export function useRemoveSavedRecipe() {
  return useSavedRecipeMutation("remove");
}
