"use client";

import { use, useEffect, useState } from "react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { Pagination } from "@/components/ui/Pagination";
import { useCategory } from "@/hooks/useCategories";
import { resolveApiAssetUrl } from "@/lib/api/axios";

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
  const heroImageUrl = resolveApiAssetUrl(data.category.imageUrl ?? data.recipes.items.find((recipe) => recipe.thumbnailUrl)?.thumbnailUrl);

  return (
    <div className="space-y-10 pb-12">
      <Breadcrumb
        items={[
          { label: "Danh mục", href: "/categories" },
          { label: data.category.name },
        ]}
      />
      <header className="grid overflow-hidden rounded-3xl bg-[#211d1b] text-white sm:grid-cols-2">
        <div className="flex flex-col justify-center p-7 sm:p-10">
          <p className="mb-4 text-xs font-bold uppercase tracking-[0.25em] text-rose-300">Danh mục / Công thức</p>
          <h1 className="font-display text-4xl font-bold tracking-tight text-white sm:text-5xl">{data.category.name}</h1>
          <p className="mt-4 max-w-xl text-base leading-7 text-stone-200">{data.category.description || "Khám phá những món ăn được yêu thích trong danh mục này."}</p>
          <p className="mt-6 text-xs font-bold uppercase tracking-widest text-rose-300">{data.recipes.totalCount} công thức</p>
        </div>
        <div className="relative min-h-52 bg-gradient-to-br from-[#8a5a45] to-[#d9b892] sm:min-h-80">
          {heroImageUrl && <img src={heroImageUrl} alt={`Món ăn thuộc danh mục ${data.category.name}`} className="absolute inset-0 h-full w-full object-cover" onError={(event) => { event.currentTarget.style.display = "none"; }} />}
        </div>
      </header>

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
