"use client";

import React from "react";
import { RecipeCard } from "@/components/common/RecipeCard";
import { EmptyState } from "@/components/ui/EmptyState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { ExtendedRecipeDto } from "@/lib/mockData";

export interface RecipeGridProps {
  recipes: ExtendedRecipeDto[];
  isLoading?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  onClearFilters?: () => void;
}

export const RecipeGrid: React.FC<RecipeGridProps> = ({
  recipes,
  isLoading = false,
  emptyTitle = "Không tìm thấy công thức",
  emptyDescription = "Chưa có công thức nào phù hợp với yêu cầu của bạn.",
  onClearFilters,
}) => {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <RecipeCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  if (!recipes || recipes.length === 0) {
    return (
      <EmptyState
        title={emptyTitle}
        description={emptyDescription}
        actionLabel={onClearFilters ? "Xóa bộ lọc" : undefined}
        onAction={onClearFilters}
      />
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
      {recipes.map((recipe) => (
        <RecipeCard key={recipe.id} {...recipe} />
      ))}
    </div>
  );
};
