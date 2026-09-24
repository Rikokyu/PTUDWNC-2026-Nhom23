"use client";

import { useInfiniteQuery } from "@tanstack/react-query";
import apiClient from "@/lib/api/axios";
import type { CommentDto, PaginatedResult } from "@/types/api";

export function useInfiniteComments(recipeId: string) {
  return useInfiniteQuery({
    queryKey: ["recipes", "comments", recipeId],
    queryFn: ({ pageParam }) =>
      apiClient
        .get<
          PaginatedResult<CommentDto>
        >(`/api/v1/recipes/${recipeId}/comments`, { params: { page: pageParam, pageSize: 10 } })
        .then((response) => response.data),
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.hasNextPage ? lastPage.page + 1 : undefined,
  });
}
