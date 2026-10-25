"use client";

import { use } from "react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useCategory } from "@/hooks/useCategories";

export default function CategoryDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = use(params);
  const categoryQuery = useCategory(slug);

  if (categoryQuery.isLoading) return <RecipeCardSkeleton />;
  if (categoryQuery.isError) {
    return (
      <ErrorState
        title="Không tải được danh mục"
        message="Danh mục không tồn tại hoặc API chưa khả dụng."
        onRetry={() => void categoryQuery.refetch()}
      />
    );
  }
  const data = categoryQuery.data;
  if (!data) return <ErrorState message="Máy chủ không trả về dữ liệu danh mục." />;

  return (
    <div className="space-y-8">
      <Breadcrumb
        items={[
          { label: "Danh mục", href: "/categories" },
          { label: data.category.name },
        ]}
      />
      <header className="space-y-3 rounded-2xl border border-slate-200 bg-orange-50/60 p-6 sm:p-8">
        <h1 className="text-3xl font-extrabold tracking-tight text-content-primary">
          {data.category.name}
        </h1>
        {data.category.description && (
          <p className="max-w-2xl text-sm leading-6 text-content-secondary">
            {data.category.description}
          </p>
        )}
        <p className="text-xs font-semibold text-content-tertiary">
          {data.recipes.totalCount} công thức
        </p>
      </header>

      {data.recipes.items.length > 0 ? (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {data.recipes.items.map((recipe) => (
            <ApiRecipeCard key={recipe.id} recipe={recipe} />
          ))}
        </div>
      ) : (
        <EmptyState
          title="Chưa có công thức trong danh mục này"
          description="Hãy quay lại sau để khám phá các công thức mới."
        />
      )}
    </div>
  );
}
