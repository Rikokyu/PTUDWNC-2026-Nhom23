"use client";

import { useState } from "react";
import Link from "next/link";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Pagination } from "@/components/ui/Pagination";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useAdminRecipes } from "@/hooks/useRecipes";
import { getApiErrorMessage } from "@/lib/api/error";
import { Button } from "@/components/ui/Button";

export default function AdminRecipesPage() {
  const [page, setPage] = useState(1);
  const recipesQuery = useAdminRecipes({ page, pageSize: 12, sort: "-createdAt" });
  const result = recipesQuery.data;

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-extrabold text-content-primary">Quản lý công thức</h1>
        <p className="mt-1 text-sm text-content-secondary">
          Hiển thị công thức đã xuất bản của các Author và toàn bộ công thức của Admin.
          Draft/Archived của Author khác được ẩn.
        </p>
      </header>

      {recipesQuery.isLoading ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <RecipeCardSkeleton key={index} />)}
        </div>
      ) : recipesQuery.isError ? (
        <ErrorState
          title="Không tải được công thức"
          message={getApiErrorMessage(recipesQuery.error)}
          onRetry={() => void recipesQuery.refetch()}
        />
      ) : !result || result.items.length === 0 ? (
        <EmptyState title="Chưa có công thức" description="Không tìm thấy công thức trong database." />
      ) : (
        <>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {result.items.map((recipe) => (
              <div key={recipe.id} className="space-y-3">
                <ApiRecipeCard recipe={recipe} />
                <div className="flex items-center justify-between px-1">
                  <span className="text-xs text-content-tertiary">
                    Trạng thái: {recipe.status === 1 ? "Đã xuất bản" : recipe.status === 0 ? "Bản nháp của Admin" : "Đã lưu trữ của Admin"}
                  </span>
                  <Link href={`/dashboard/recipes/${encodeURIComponent(recipe.id)}/edit?slug=${encodeURIComponent(recipe.slug)}`}>
                    <Button size="sm" variant="outline">Chỉnh sửa</Button>
                  </Link>
                </div>
              </div>
            ))}
          </div>
          <Pagination currentPage={result.page} totalPages={result.totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
