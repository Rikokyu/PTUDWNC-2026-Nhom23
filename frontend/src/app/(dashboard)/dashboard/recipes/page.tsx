"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { ArrowUpRight, Plus } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { recipeApi } from "@/lib/api/recipes";
import { useAuthStore } from "@/store/useAuthStore";
import type { ApiRecipeStatus } from "@/types/api";
import { RecipeThumbnail } from "@/components/common/RecipeThumbnail";

const statusLabels: Record<ApiRecipeStatus, string> = {
  0: "Bản nháp",
  1: "Đã đăng",
  2: "Đã lưu trữ",
};

export default function ManagedRecipesPage() {
  const router = useRouter();
  const accessToken = useAuthStore((state) => state.accessToken);
  const [page, setPage] = useState(1);
  const recipesQuery = useQuery({
    queryKey: ["managed-recipes", "mine", page],
    queryFn: () => recipeApi.getManagedList(page, 10, false),
    enabled: Boolean(accessToken),
  });

  useEffect(() => {
    if (!accessToken) router.replace("/auth/login");
  }, [accessToken, router]);

  if (!accessToken) {
    return <p className="py-12 text-center text-sm">Đang chuyển đến trang đăng nhập...</p>;
  }

  if (recipesQuery.isLoading) return <RecipeCardSkeleton />;
  if (recipesQuery.isError) {
    return <ErrorState onRetry={() => void recipesQuery.refetch()} />;
  }

  const result = recipesQuery.data;
  if (!result) {
    return <ErrorState message="Không nhận được dữ liệu công thức từ máy chủ." />;
  }

  return (
    <div className="mx-auto max-w-5xl space-y-7">
      <header className="flex flex-wrap items-end justify-between gap-4 border-b border-stone-200 pb-6">
        <div>
          <p className="mb-2 text-xs font-bold uppercase tracking-[0.2em] text-brand">Không gian tác giả</p>
          <h1 className="font-display text-3xl font-bold text-content-primary">Công thức của tôi</h1>
          <p className="mt-1 text-sm text-content-secondary">
            Quản lý bài viết, nguyên liệu, các bước và hình ảnh.
          </p>
        </div>
        <Link href="/dashboard/recipes/new">
          <Button leftIcon={<Plus className="h-4 w-4" />}>Tạo công thức</Button>
        </Link>
      </header>

      <div className="flex items-center justify-between text-sm text-content-secondary"><span>{result.totalCount} công thức</span><span>Mới nhất trước</span></div>
      <section className="divide-y divide-stone-100 overflow-hidden rounded-2xl border border-stone-200 bg-white shadow-subtle">
        {result.items.map((recipe) => (
          <article
            key={recipe.id}
            className="flex flex-wrap items-center justify-between gap-4 p-4 transition-colors hover:bg-stone-50 sm:p-5"
          >
            <div className="flex min-w-0 items-center gap-4">
              <RecipeThumbnail src={recipe.thumbnailUrl} alt={recipe.title} className="h-16 w-16 shrink-0 rounded-xl object-cover sm:h-20 sm:w-20" />
              <div className="min-w-0">
              <Link
                href={`/dashboard/recipes/${recipe.id}/edit`}
                className="font-display text-base font-bold text-content-primary hover:text-brand"
              >
                {recipe.title}
              </Link>
              <div className="mt-2 flex flex-wrap items-center gap-2">
                <Badge size="sm" variant={recipe.status === 1 ? "accent" : recipe.status === 0 ? "brand" : "neutral"}>{statusLabels[recipe.status]}</Badge>
                <span className="text-xs text-content-tertiary">{recipe.categoryName || "Chưa phân loại"}</span>
              </div>
              </div>
            </div>
            <Link href={`/dashboard/recipes/${recipe.id}/edit`}>
              <Button variant="outline" size="sm" rightIcon={<ArrowUpRight className="h-4 w-4" />}>Chỉnh sửa</Button>
            </Link>
          </article>
        ))}
        {result.items.length === 0 && (
          <p className="p-8 text-center text-sm text-content-secondary">
            Chưa có công thức nào. Hãy tạo công thức đầu tiên.
          </p>
        )}
      </section>

      {result.totalPages > 1 && (
        <nav className="flex items-center justify-center gap-4" aria-label="Phân trang công thức">
          <Button
            variant="outline"
            size="sm"
            disabled={!result.hasPreviousPage}
            onClick={() => setPage((current) => current - 1)}
          >
            Trước
          </Button>
          <span className="text-sm text-content-secondary">
            Trang {result.page} / {result.totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={!result.hasNextPage}
            onClick={() => setPage((current) => current + 1)}
          >
            Tiếp
          </Button>
        </nav>
      )}
    </div>
  );
}
