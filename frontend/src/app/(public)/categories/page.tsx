"use client";

import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useCategories } from "@/hooks/useCategories";
import { CategoryGrid } from "@/features/categories/CategoryGrid";

export default function CategoriesPage() {
  const categoriesQuery = useCategories();

  return (
    <div className="space-y-10 pb-12">
      <Breadcrumb items={[{ label: "Tất cả danh mục" }]} />
      <header className="border-b border-zinc-300 pb-8 sm:pb-10">
        <p className="mb-4 text-xs font-bold uppercase tracking-[0.25em] text-brand">Khám phá theo chủ đề</p>
        <h1 className="font-display text-4xl font-bold tracking-tight text-content-primary sm:text-5xl">
          Danh mục món ăn
        </h1>
        <p className="mt-4 max-w-2xl text-base leading-7 text-content-secondary">
          Từ bữa cơm nhà đến những món mới lạ, chọn một chủ đề để bắt đầu vào bếp.
        </p>
      </header>

      {categoriesQuery.isLoading && (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <RecipeCardSkeleton key={index} />)}
        </div>
      )}
      {categoriesQuery.isError && (
        <ErrorState
          title="Không tải được danh mục"
          message="Hãy kiểm tra API và thử lại."
          onRetry={() => void categoriesQuery.refetch()}
        />
      )}
      {categoriesQuery.data?.length === 0 && (
        <p className="rounded-xl border border-slate-200 bg-white p-8 text-center text-sm text-content-secondary">
          Hiện chưa có danh mục nào.
        </p>
      )}
      {categoriesQuery.data && categoriesQuery.data.length > 0 && (
        <CategoryGrid categories={categoriesQuery.data} />
      )}
    </div>
  );
}
