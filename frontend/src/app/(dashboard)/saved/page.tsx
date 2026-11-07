"use client";

import Link from "next/link";
import { useState } from "react";
import { Clock, X } from "lucide-react";
import { useAuth } from "@/components/auth/AuthProvider";
import { ErrorState } from "@/components/ui/ErrorState";
import { EmptyState } from "@/components/ui/EmptyState";
import { Pagination } from "@/components/ui/Pagination";
import { useRemoveSavedRecipe, useSavedRecipes } from "@/hooks/useSavedRecipes";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import { getApiErrorMessage } from "@/lib/api/error";

const PAGE_SIZE = 12;

export default function SavedRecipesPage() {
  const { user, status } = useAuth();
  const [page, setPage] = useState(1);
  const savedQuery = useSavedRecipes(page, PAGE_SIZE);
  const removeMutation = useRemoveSavedRecipe();

  if (status === "loading") {
    return <p role="status" className="py-12 text-center text-sm text-content-secondary">Đang tải danh sách đã lưu...</p>;
  }

  if (user && !["Author", "Admin"].includes(user.role)) {
    return <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-6 text-sm text-rose-800">Chức năng công thức đã lưu chỉ dành cho Author và Admin.</div>;
  }

  const result = savedQuery.data;

  return (
    <section className="space-y-6">
      <header>
        <p className="text-sm font-semibold text-brand">Thư viện cá nhân</p>
        <h1 className="mt-1 text-2xl font-bold text-content-primary sm:text-3xl">Công thức đã lưu</h1>
        <p className="mt-2 text-sm text-content-secondary">Danh sách công thức Published bạn đã đánh dấu để xem lại.</p>
      </header>

      {savedQuery.isLoading ? (
        <div role="status" className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => (
            <div key={index} className="animate-pulse overflow-hidden rounded-xl border border-slate-200 bg-white">
              <div className="aspect-[4/3] bg-slate-100" />
              <div className="space-y-3 p-5"><div className="h-4 w-2/3 rounded bg-slate-100" /><div className="h-3 w-1/2 rounded bg-slate-100" /></div>
            </div>
          ))}
        </div>
      ) : savedQuery.isError ? (
        <ErrorState
          title="Không tải được công thức đã lưu"
          message={getApiErrorMessage(savedQuery.error)}
          onRetry={() => void savedQuery.refetch()}
        />
      ) : result?.items.length ? (
        <>
          <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
            {result.items.map((recipe) => (
              <article key={recipe.id} className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-subtle">
                <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`} className="block">
                  <img
                    src={resolveApiAssetUrl(recipe.thumbnailUrl) || "/images/image 1.png"}
                    alt={recipe.title}
                    className="aspect-[4/3] w-full bg-slate-100 object-cover"
                    loading="lazy"
                  />
                </Link>
                <div className="space-y-3 p-5">
                  <div className="flex items-center justify-between gap-3 text-xs text-content-tertiary">
                    <span className="truncate font-medium text-brand">{recipe.categoryName}</span>
                    <span className="inline-flex shrink-0 items-center gap-1"><Clock className="h-3.5 w-3.5" />{recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút</span>
                  </div>
                  <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`} className="block text-base font-bold text-content-primary hover:text-brand">
                    {recipe.title}
                  </Link>
                  <button
                    type="button"
                    onClick={() => removeMutation.mutate(recipe.id)}
                    disabled={removeMutation.isPending}
                    className="inline-flex min-h-10 w-full items-center justify-center gap-2 rounded-lg border border-slate-200 px-4 text-sm font-semibold text-slate-700 transition hover:border-rose-300 hover:bg-rose-50 hover:text-rose-700 disabled:opacity-60"
                  >
                    <X className="h-4 w-4" />
                    Bỏ lưu
                  </button>
                  {removeMutation.isError && removeMutation.variables === recipe.id && (
                    <p role="alert" className="text-xs text-rose-700">{getApiErrorMessage(removeMutation.error)}</p>
                  )}
                </div>
              </article>
            ))}
          </div>
          <Pagination
            currentPage={result.page}
            totalPages={result.totalPages}
            onPageChange={setPage}
          />
        </>
      ) : (
        <EmptyState
          title="Bạn chưa lưu công thức nào"
          description="Dùng biểu tượng bookmark trên thẻ hoặc trang chi tiết để lưu công thức Published."
          actionLabel="Khám phá công thức"
          onAction={() => { window.location.href = "/recipes"; }}
        />
      )}
    </section>
  );
}
