"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useQueryClient } from "@tanstack/react-query";
import { ArrowRightLeft, CheckSquare2, Pencil, Trash2, Undo2 } from "lucide-react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { Button } from "@/components/ui/Button";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Pagination } from "@/components/ui/Pagination";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useAuth } from "@/components/auth/AuthProvider";
import { CategoryHero } from "@/features/categories/CategoryHero";
import { categoryKeys, useCategories } from "@/hooks/useCategories";
import { categoryApi } from "@/lib/api/categories";
import { useAdminRecipes, recipeKeys } from "@/hooks/useRecipes";
import { recipeApi } from "@/lib/api/recipes";
import { getApiErrorMessage } from "@/lib/api/error";
import type { RecipeApiSummaryDto } from "@/types/api";

export function CategoryRecipeManager({ slug }: { slug: string }) {
  const { user } = useAuth();
  const isAdmin = user?.role === "Admin";
  const categories = useCategories();
  const category = categories.data?.find((item) => item.slug === slug);
  const [page, setPage] = useState(1);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(() => new Set());
  const [targetCategoryId, setTargetCategoryId] = useState("");
  const [selectingAll, setSelectingAll] = useState(false);
  const [bulkBusy, setBulkBusy] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const queryClient = useQueryClient();
  const recipes = useAdminRecipes(
    { categoryId: category?.id, page, pageSize: 12, sortBy: "createdAt", sortOrder: "desc" },
    isAdmin && Boolean(category),
  );
  const totalCount = recipes.data?.totalCount ?? 0;
  const allSelected = totalCount > 0 && selectedIds.size === totalCount;

  useEffect(() => {
    const rawPage = new URLSearchParams(window.location.search).get("page");
    const parsed = rawPage ? Number.parseInt(rawPage, 10) : NaN;
    if (Number.isSafeInteger(parsed) && parsed >= 1) setPage(parsed);
  }, []);

  function changePage(nextPage: number) {
    setPage(nextPage);
    const url = new URL(window.location.href);
    if (nextPage > 1) url.searchParams.set("page", String(nextPage));
    else url.searchParams.delete("page");
    window.history.pushState(null, "", url);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function toggleRecipe(id: string) {
    setSelectedIds((previous) => {
      const next = new Set(previous);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  async function toggleAll() {
    if (!category) return;
    if (allSelected) {
      setSelectedIds(new Set());
      return;
    }
    setSelectingAll(true);
    setError("");
    try {
      const filters = { categoryId: category.id, pageSize: 50, sortBy: "createdAt" as const, sortOrder: "desc" as const };
      const first = await recipeApi.getAdmin({ ...filters, page: 1 });
      const ids = first.items.map((recipe) => recipe.id);
      for (let nextPage = 2; nextPage <= first.totalPages; nextPage++) {
        const result = await recipeApi.getAdmin({ ...filters, page: nextPage });
        ids.push(...result.items.map((recipe) => recipe.id));
      }
      setSelectedIds(new Set(ids));
    } catch (cause) {
      setError(getApiErrorMessage(cause));
    } finally {
      setSelectingAll(false);
    }
  }

  async function runBulk(action: "move" | "delete") {
    if (!category || selectedIds.size === 0) return;
    if (action === "move" && !targetCategoryId) {
      setError("Hãy chọn danh mục đích.");
      return;
    }
    if (!window.confirm(action === "delete"
      ? `Xóa ${selectedIds.size} công thức đã chọn? Công thức sẽ bị ẩn khỏi website.`
      : `Chuyển ${selectedIds.size} công thức sang danh mục đã chọn?`)) return;
    setBulkBusy(true);
    setMessage("");
    setError("");
    try {
      const ids = [...selectedIds];
      const count = action === "delete"
        ? await categoryApi.deleteRecipes(category.id, ids)
        : await categoryApi.moveRecipes(category.id, ids, targetCategoryId);
      setSelectedIds(new Set());
      changePage(1);
      await queryClient.invalidateQueries({ queryKey: recipeKeys.all() });
      await queryClient.invalidateQueries({ queryKey: categoryKeys.all() });
      setMessage(action === "delete" ? `Đã xóa ${count} công thức.` : `Đã chuyển ${count} công thức sang danh mục mới.`);
    } catch (cause) {
      setError(getApiErrorMessage(cause));
    } finally {
      setBulkBusy(false);
    }
  }

  async function act(recipe: RecipeApiSummaryDto, action: "unpublish" | "delete") {
    const confirmed = window.confirm(action === "delete"
      ? `Xóa công thức “${recipe.title}”? Công thức sẽ bị ẩn khỏi website.`
      : `Hủy xuất bản “${recipe.title}” và chuyển về bản nháp?`);
    if (!confirmed) return;
    setBusyId(recipe.id);
    setError("");
    setMessage("");
    try {
      if (action === "delete") await recipeApi.deleteRecipe(recipe.id);
      else await recipeApi.setStatus(recipe.id, "unpublish");
      if (action === "delete") setSelectedIds((previous) => {
        const next = new Set(previous);
        next.delete(recipe.id);
        return next;
      });
      await queryClient.invalidateQueries({ queryKey: recipeKeys.all() });
      await queryClient.invalidateQueries({ queryKey: ["categories"] });
      setMessage(action === "delete" ? "Đã xóa công thức." : "Đã chuyển công thức về bản nháp.");
      if (action === "delete" && recipes.data?.items.length === 1 && page > 1) changePage(page - 1);
    } catch (cause) {
      setError(getApiErrorMessage(cause));
    } finally {
      setBusyId(null);
    }
  }

  if (!isAdmin) return null;
  if (categories.isLoading) return <RecipeCardSkeleton />;
  if (categories.isError) return <ErrorState title="Không tải được danh mục" message={getApiErrorMessage(categories.error)} onRetry={() => void categories.refetch()} />;
  if (!category) return <ErrorState title="Không tìm thấy danh mục" message="Danh mục này không tồn tại hoặc đã bị xóa." />;

  return (
    <div className="space-y-8 pb-12">
      <Breadcrumb items={[{ label: "Quản lý danh mục", href: "/dashboard/categories" }, { label: category.name }]} />
      <CategoryHero category={category} recipeCount={recipes.data?.totalCount ?? category.recipeCount} />
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <p className="editorial-kicker mb-2">Quản trị nội dung</p>
          <h2 className="font-display text-2xl font-bold text-content-primary">Công thức trong danh mục</h2>
        </div>
        <Link href="/dashboard/categories"><Button variant="outline" size="sm">Quay về danh mục</Button></Link>
      </div>
      <div className="flex flex-wrap items-end gap-3 rounded-2xl border border-stone-200 bg-surface-cream p-4">
        <Button size="sm" variant="outline" leftIcon={<CheckSquare2 className="h-4 w-4" />} disabled={bulkBusy || selectingAll || busyId !== null || totalCount === 0} isLoading={selectingAll} onClick={() => void toggleAll()}>
          {allSelected ? "Bỏ chọn tất cả" : "Chọn tất cả"}
        </Button>
        <span className="self-center text-sm text-content-secondary">Đã chọn {selectedIds.size}/{totalCount}</span>
        <label className="min-w-48 flex-1 space-y-1 text-xs font-semibold text-content-secondary sm:max-w-xs">
          Chuyển đến danh mục
          <select value={targetCategoryId} onChange={(event) => setTargetCategoryId(event.target.value)} disabled={bulkBusy || selectingAll} className="mt-1 h-9 w-full rounded-xl border border-stone-300 bg-white px-3 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15">
            <option value="">Chọn danh mục</option>
            {categories.data?.filter((item) => item.id !== category.id).map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}
          </select>
        </label>
        <Button size="sm" variant="outline" leftIcon={<ArrowRightLeft className="h-4 w-4" />} disabled={bulkBusy || selectingAll || busyId !== null || selectedIds.size === 0 || !targetCategoryId} isLoading={bulkBusy} onClick={() => void runBulk("move")}>Chuyển danh mục</Button>
        <Button size="sm" variant="danger" leftIcon={<Trash2 className="h-4 w-4" />} disabled={bulkBusy || selectingAll || busyId !== null || selectedIds.size === 0} isLoading={bulkBusy} onClick={() => void runBulk("delete")}>Xóa đã chọn</Button>
      </div>
      {message && <p role="status" className="text-sm font-medium text-emerald-700">{message}</p>}
      {error && <p role="alert" className="text-sm font-medium text-rose-700">{error}</p>}
      {recipes.isLoading ? (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <RecipeCardSkeleton key={index} />)}
        </div>
      ) : recipes.isError ? (
        <ErrorState title="Không tải được công thức" message={getApiErrorMessage(recipes.error)} onRetry={() => void recipes.refetch()} />
      ) : !recipes.data?.items.length ? (
        <EmptyState title="Chưa có công thức trong danh mục này" description="Danh mục hiện chưa có công thức." />
      ) : (
        <>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {recipes.data.items.map((recipe) => (
              <div key={recipe.id} className="overflow-hidden rounded-2xl border border-stone-200 bg-surface shadow-subtle">
                <ApiRecipeCard recipe={recipe} />
                <div className="flex flex-col gap-3 border-t border-stone-100 px-4 py-4">
                  <label className="flex cursor-pointer items-center gap-2 text-sm font-medium text-content-primary">
                    <input type="checkbox" checked={selectedIds.has(recipe.id)} onChange={() => toggleRecipe(recipe.id)} disabled={bulkBusy || selectingAll || busyId !== null} aria-label={`Chọn công thức ${recipe.title}`} className="h-4 w-4 accent-brand" />
                    Chọn công thức
                  </label>
                  <span className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ${recipe.status === 1 ? "bg-emerald-50 text-emerald-700" : recipe.status === 0 ? "bg-amber-50 text-amber-800" : "bg-stone-100 text-stone-700"}`}>
                    {recipe.status === 1 ? "Đã xuất bản" : recipe.status === 0 ? "Bản nháp" : "Đã lưu trữ"}
                  </span>
                  <div className="grid grid-cols-2 gap-2">
                    <Link className="col-span-2" href={`/dashboard/recipes/${encodeURIComponent(recipe.id)}/edit?from=categories&categorySlug=${encodeURIComponent(category.slug)}`}>
                      <Button size="sm" variant="outline" className="w-full" leftIcon={<Pencil className="h-4 w-4" />}>Chỉnh sửa</Button>
                    </Link>
                    <Button size="sm" variant="outline" className="w-full" disabled={bulkBusy || busyId !== null || recipe.status !== 1} title={recipe.status === 1 ? undefined : "Chỉ công thức đã xuất bản mới có thể hủy xuất bản"} leftIcon={<Undo2 className="h-4 w-4" />} onClick={() => void act(recipe, "unpublish")}>Hủy xuất bản</Button>
                    <Button size="sm" variant="danger" className="w-full" disabled={bulkBusy || busyId !== null} leftIcon={<Trash2 className="h-4 w-4" />} onClick={() => void act(recipe, "delete")}>Xóa</Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
          <Pagination currentPage={page} totalPages={recipes.data.totalPages} onPageChange={changePage} />
        </>
      )}
    </div>
  );
}
