"use client";

import { use, useEffect, useState } from "react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { Pagination } from "@/components/ui/Pagination";
import { CategoryHero } from "@/features/categories/CategoryHero";
import { useCategory } from "@/hooks/useCategories";

export default function CategoryDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = use(params);
  const [page, setPage] = useState(1);

  useEffect(() => {
    const searchParams = new URLSearchParams(window.location.search);
    const rawPage = searchParams.get("page");
    const parsed = rawPage ? Number.parseInt(rawPage, 10) : NaN;
    if (Number.isSafeInteger(parsed) && parsed >= 1) {
      setPage(parsed);
    }
  }, []);

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
    const url = new URL(window.location.href);
    if (newPage > 1) {
      url.searchParams.set("page", String(newPage));
    } else {
      url.searchParams.delete("page");
    }
    window.history.pushState(null, "", url);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const categoryQuery = useCategory(slug, page);

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
    <div className="space-y-10 pb-12">
      <Breadcrumb
        items={[
          { label: "Danh mục", href: "/categories" },
          { label: data.category.name },
        ]}
      />
      <CategoryHero category={data.category} recipeCount={data.recipes.totalCount} />

      {data.recipes.items.length > 0 ? (
        <>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {data.recipes.items.map((recipe) => (
              <ApiRecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>
          {data.recipes.totalPages > 1 && (
            <Pagination
              currentPage={data.recipes.page}
              totalPages={data.recipes.totalPages}
              onPageChange={handlePageChange}
            />
          )}
        </>
      ) : (
        <EmptyState
          title="Chưa có công thức trong danh mục này"
          description="Hãy quay lại sau để khám phá các công thức mới."
        />
      )}
    </div>
  );
}
