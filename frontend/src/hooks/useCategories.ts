"use client";

import { useQuery } from "@tanstack/react-query";
import { categoryApi } from "@/lib/api/categories";

export const categoryKeys = {
  all: () => ["categories"] as const,
  lists: () => [...categoryKeys.all(), "list"] as const,
  details: () => [...categoryKeys.all(), "detail"] as const,
  detail: (slug: string, page: number) =>
    [...categoryKeys.details(), slug, page] as const,
};

export function useCategories() {
  return useQuery({
    queryKey: categoryKeys.lists(),
    queryFn: () => categoryApi.getAll(),
  });
}

export function useCategory(slug: string, page = 1) {
  return useQuery({
    queryKey: categoryKeys.detail(slug, page),
    queryFn: () => categoryApi.getBySlug(slug, page),
    enabled: Boolean(slug),
  });
}
