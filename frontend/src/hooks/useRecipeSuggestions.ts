"use client";

import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { recipeApi } from "@/lib/api/recipes";

export function useRecipeSuggestions(draft: string) {
  const [debouncedTerm, setDebouncedTerm] = useState("");
  useEffect(() => {
    const normalized = draft.trim();
    if (normalized.length < 1) {
      setDebouncedTerm("");
      return;
    }
    const timeout = window.setTimeout(() => setDebouncedTerm(normalized), 250);
    return () => window.clearTimeout(timeout);
  }, [draft]);

  const query = useQuery({
    queryKey: ["recipe-suggestions", debouncedTerm],
    queryFn: () => debouncedTerm.length >= 2
      ? recipeApi.search(debouncedTerm, 1, 6)
      : recipeApi.getList({ search: debouncedTerm, page: 1, pageSize: 6 }),
    enabled: debouncedTerm.length >= 1,
    staleTime: 60_000,
  });

  return {
    suggestions: draft.trim() === debouncedTerm
      ? (query.data?.items ?? []).map((recipe) => ({
          id: recipe.id,
          label: recipe.title,
          description: recipe.categoryName,
        }))
      : [],
    isLoading: draft.trim().length >= 1
      && (draft.trim() !== debouncedTerm || query.isLoading),
  };
}
