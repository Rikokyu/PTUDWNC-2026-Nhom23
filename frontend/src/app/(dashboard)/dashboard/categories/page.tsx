"use client";

import { CategoryGrid } from "@/features/categories/CategoryGrid";
import { useCategories } from "@/hooks/useCategories";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Skeleton } from "@/components/ui/LoadingState";
import { getApiErrorMessage } from "@/lib/api/error";

export default function DashboardCategoriesPage() {
  const categoriesQuery = useCategories();

  return (
    <div className="space-y-8">
      <header>
        <h1 className="text-2xl font-extrabold text-content-primary">Quản lý danh mục</h1>
        <p className="mt-1 text-sm text-content-secondary">
          Danh sách danh mục hiện có trong PostgreSQL. Trang này chỉ dành cho Admin.
        </p>
      </header>
      {categoriesQuery.isLoading ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <Skeleton key={index} className="h-44 rounded-xl" />)}
        </div>
      ) : categoriesQuery.isError ? (
        <ErrorState
          title="Không tải được danh mục"
          message={getApiErrorMessage(categoriesQuery.error)}
          onRetry={() => void categoriesQuery.refetch()}
        />
      ) : (categoriesQuery.data?.length ?? 0) === 0 ? (
        <EmptyState title="Chưa có danh mục" description="Database chưa có dữ liệu danh mục." />
      ) : (
        <CategoryGrid categories={categoriesQuery.data ?? []} />
      )}
    </div>
  );
}
