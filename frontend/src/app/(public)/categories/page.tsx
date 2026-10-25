"use client";

import Link from "next/link";
import { ArrowRight, ChefHat } from "lucide-react";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useCategories } from "@/hooks/useCategories";

export default function CategoriesPage() {
  const categoriesQuery = useCategories();

  return (
    <div className="space-y-8">
      <Breadcrumb items={[{ label: "Tất cả danh mục" }]} />
      <header className="space-y-2">
        <h1 className="text-3xl font-extrabold tracking-tight text-content-primary">
          Danh mục món ăn
        </h1>
        <p className="max-w-2xl text-sm text-content-secondary">
          Khám phá công thức theo từng chủ đề ẩm thực.
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
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {categoriesQuery.data.map((category) => (
            <Link
              key={category.id}
              href={`/categories/${encodeURIComponent(category.slug)}`}
              className="group flex min-h-48 flex-col justify-between rounded-xl border border-slate-200 bg-white p-6 transition hover:border-brand/50 hover:bg-orange-50/40"
            >
              <div className="flex items-start justify-between gap-4">
                <div>
                  <h2 className="text-lg font-bold text-content-primary group-hover:text-brand">
                    {category.name}
                  </h2>
                  <p className="mt-1 text-xs text-content-tertiary">
                    {category.recipeCount} công thức
                  </p>
                </div>
                <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-orange-50 text-brand">
                  <ChefHat className="h-5 w-5" />
                </span>
              </div>
              <p className="mt-5 line-clamp-3 text-sm leading-6 text-content-secondary">
                {category.description || "Khám phá các công thức trong danh mục này."}
              </p>
              <span className="mt-5 inline-flex items-center gap-1 text-sm font-semibold text-brand">
                Xem công thức <ArrowRight className="h-4 w-4" />
              </span>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
