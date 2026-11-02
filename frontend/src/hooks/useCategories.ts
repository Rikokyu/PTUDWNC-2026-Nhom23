"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { categoryApi } from "@/lib/api/categories";
import type { CreateCategoryRequest } from "@/lib/api/categories";
import type { UpdateCategoryRequest } from "@/lib/api/categories";

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

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateCategoryRequest) =>
      categoryApi.create(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: categoryKeys.lists(),
      });
    },
  });
}

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      request,
    }: {
      id: string;
      request: UpdateCategoryRequest;
    }) => categoryApi.update(id, request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: categoryKeys.lists(),
      });
      await queryClient.invalidateQueries({
        queryKey: categoryKeys.details(),
      });
    },
  });
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => categoryApi.delete(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: categoryKeys.lists(),
      });
      await queryClient.invalidateQueries({
        queryKey: categoryKeys.details(),
      });
    },
  });
}
