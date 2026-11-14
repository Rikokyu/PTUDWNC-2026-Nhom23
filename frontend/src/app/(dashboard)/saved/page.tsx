"use client";

import Link from "next/link";
import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { ChefHat, Clock, Trash2, X } from "lucide-react";
import { useAuth } from "@/components/auth/AuthProvider";
import { ErrorState } from "@/components/ui/ErrorState";
import { EmptyState } from "@/components/ui/EmptyState";
import { Pagination } from "@/components/ui/Pagination";
import { savedRecipeKeys, useRemoveSavedRecipe, useSavedRecipes } from "@/hooks/useSavedRecipes";
import { recipeKeys } from "@/hooks/useRecipes";
import { recipeApi } from "@/lib/api/recipes";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import { getApiErrorMessage } from "@/lib/api/error";

const PAGE_SIZE = 12;

export default function SavedRecipesPage() {
  const { user, status } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [deletingRecipeId, setDeletingRecipeId] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<{ id: string; message: string } | null>(null);
  const savedQuery = useSavedRecipes(page, PAGE_SIZE);
  const removeMutation = useRemoveSavedRecipe();

  async function deleteRecipe(recipeId: string, title: string) {
    if (!window.confirm(`Xóa công thức “${title}” khỏi hệ thống? Bài sẽ bị ẩn với mọi người; dữ liệu vẫn được giữ để có thể khôi phục sau này.`)) return;
    setDeleteError(null);
    setDeletingRecipeId(recipeId);
    try {
      await recipeApi.deleteRecipe(recipeId);
      await Promise.allSettled([
        queryClient.invalidateQueries({ queryKey: savedRecipeKeys.all(user?.id ?? "") }),
        queryClient.invalidateQueries({ queryKey: recipeKeys.all() }),
      ]);
      if (savedQuery.data?.items.length === 1 && page > 1) setPage(page - 1);
    } catch (error) {
      setDeleteError({ id: recipeId, message: getApiErrorMessage(error) });
    } finally {
      setDeletingRecipeId(null);
    }
  }

  if (status === "loading") {
    return <p role="status" className="py-12 text-center text-sm text-content-secondary">Đang tải danh sách đã lưu...</p>;
  }

  if (user && !["Author", "Admin"].includes(user.role)) {
    return <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-6 text-sm text-rose-800">Chức năng công thức đã lưu chỉ dành cho Author và Admin.</div>;
  }

  const result = savedQuery.data;

  return (
    <section className="space-y-6">
      <header className="border-b border-stone-300 pb-5">
        <p className="editorial-kicker">Thư viện cá nhân</p>
        <h1 className="mt-2 font-display text-3xl font-bold text-content-primary sm:text-4xl">Công thức đã lưu</h1>
        <p className="mt-2 text-sm text-content-secondary">Danh sách công thức Published bạn đã đánh dấu để xem lại.</p>
      </header>

      {savedQuery.isLoading ? (
        <div role="status" className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => (
            <div key={index} className="animate-pulse overflow-hidden rounded-2xl border border-stone-200 bg-surface">
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
              <article key={recipe.id} className="group relative overflow-hidden rounded-2xl border border-stone-200 bg-surface shadow-subtle transition-shadow hover:shadow-card">
                <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`} className="block">
                  {resolveApiAssetUrl(recipe.thumbnailUrl) ? (
                    <img
                      src={resolveApiAssetUrl(recipe.thumbnailUrl)!}
                      alt={recipe.title}
                      className="aspect-[4/3] w-full bg-surface-subtle object-cover transition-transform duration-500 group-hover:scale-[1.03] motion-reduce:transform-none"
                      loading="lazy"
                      decoding="async"
                    />
                  ) : (
                    <div className="flex aspect-[4/3] flex-col items-center justify-center gap-2 bg-slate-100 text-slate-500">
                      <ChefHat className="h-8 w-8" aria-hidden="true" />
                      <span className="text-xs font-medium">Chưa có ảnh món ăn</span>
                    </div>
                  )}
                </Link>
                <div className="saved-recipe-actions absolute right-3 top-3 z-10 flex flex-wrap justify-end gap-2 transition-opacity">
                  <button
                    type="button"
                    onClick={() => removeMutation.mutate(recipe.id)}
                    disabled={removeMutation.isPending || deletingRecipeId === recipe.id}
                    className="inline-flex min-h-10 items-center gap-1.5 rounded-lg bg-white/95 px-3 text-xs font-semibold text-slate-800 shadow-md transition hover:bg-rose-50 hover:text-rose-700 focus-visible:outline focus-visible:outline-2 focus-visible:outline-brand disabled:opacity-60"
                    aria-label={`Bỏ lưu công thức ${recipe.title}`}
                  >
                    <X className="h-4 w-4" aria-hidden="true" /> Bỏ lưu
                  </button>
                  {user?.role === "Admin" && (
                    <button
                      type="button"
                      onClick={() => void deleteRecipe(recipe.id, recipe.title)}
                      disabled={Boolean(deletingRecipeId) || removeMutation.isPending}
                      className="inline-flex min-h-10 items-center gap-1.5 rounded-lg bg-rose-700 px-3 text-xs font-semibold text-white shadow-md transition hover:bg-rose-800 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-rose-700 disabled:opacity-60"
                      aria-label={`Xóa công thức ${recipe.title} khỏi hệ thống`}
                    >
                      <Trash2 className="h-4 w-4" aria-hidden="true" /> Xóa công thức
                    </button>
                  )}
                </div>
                <div className="space-y-3 p-5">
                  <div className="flex items-center justify-between gap-3 text-xs text-content-tertiary">
                    <span className="truncate font-medium text-brand">{recipe.categoryName}</span>
                    <span className="inline-flex shrink-0 items-center gap-1"><Clock className="h-3.5 w-3.5" />{recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút</span>
                  </div>
                  <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`} className="block text-base font-bold text-content-primary hover:text-brand">
                    {recipe.title}
                  </Link>
                  {removeMutation.isError && removeMutation.variables === recipe.id && (
                    <p role="alert" className="text-xs text-rose-700">{getApiErrorMessage(removeMutation.error)}</p>
                  )}
                  {deleteError?.id === recipe.id && (
                    <p role="alert" className="text-xs text-rose-700">{deleteError.message}</p>
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
