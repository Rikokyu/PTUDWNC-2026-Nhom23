"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { recipeApi } from "@/lib/api/recipes";
import { useAuthStore } from "@/store/useAuthStore";
import type { ApiRecipeStatus } from "@/types/api";

const statusLabels: Record<ApiRecipeStatus, string> = {
  0: "Bản nháp",
  1: "Đã đăng",
  2: "Đã lưu trữ",
};

export default function ManagedRecipesPage() {
  const router = useRouter();
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.role.toLocaleLowerCase() === "admin";
  const [page, setPage] = useState(1);
  const recipesQuery = useQuery({
    queryKey: ["managed-recipes", page, isAdmin],
    queryFn: () => recipeApi.getManagedList(page, 10, isAdmin),
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
    <main className="mx-auto max-w-5xl space-y-6 px-4 py-8">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-content-primary">Công thức của tôi</h1>
          <p className="mt-1 text-sm text-content-secondary">
            Quản lý nguyên liệu, các bước và hình ảnh cho công thức.
          </p>
        </div>
        <Link href="/dashboard/recipes/new">
          <Button leftIcon={<Plus className="h-4 w-4" />}>Tạo công thức</Button>
        </Link>
      </header>

      <section className="divide-y divide-slate-200 rounded-xl border border-slate-200 bg-white">
        {result.items.map((recipe) => (
          <article
            key={recipe.id}
            className="flex flex-wrap items-center justify-between gap-4 p-4"
          >
            <div className="min-w-0">
              <Link
                href={`/dashboard/recipes/${recipe.id}/edit`}
                className="font-semibold text-content-primary hover:text-primary"
              >
                {recipe.title}
              </Link>
              <p className="mt-1 text-sm text-content-secondary">
                {recipe.categoryName} · {statusLabels[recipe.status]}
              </p>
            </div>
            <Link href={`/dashboard/recipes/${recipe.id}/edit`}>
              <Button variant="outline" size="sm">Quản lý nội dung</Button>
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
    </main>
  );
}
