"use client";

import { useState, type FormEvent } from "react";
import Link from "next/link";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Pagination } from "@/components/ui/Pagination";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useAdminRecipes } from "@/hooks/useRecipes";
import { getApiErrorMessage } from "@/lib/api/error";
import { Button } from "@/components/ui/Button";
import { useAuth } from "@/components/auth/AuthProvider";
import { Badge } from "@/components/ui/Badge";
import { ArrowUpRight, Search, X } from "lucide-react";

export default function AdminRecipesPage() {
  const { user } = useAuth();
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const recipesQuery = useAdminRecipes({ page, pageSize: 12, sort: "-createdAt", search }, user?.role === "Admin");
  const result = recipesQuery.data;

  function submitSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPage(1);
    setSearch(searchInput.trim());
  }

  function clearSearch() {
    setSearchInput("");
    setSearch("");
    setPage(1);
  }

  if (user?.role !== "Admin") return null;

  return (
    <div className="space-y-7">
      <header className="border-b border-stone-200 pb-6">
        <p className="mb-2 text-xs font-bold uppercase tracking-[0.2em] text-brand">Quản trị nội dung</p>
        <h1 className="font-display text-3xl font-bold text-content-primary">Quản lý công thức</h1>
        <p className="mt-1 text-sm text-content-secondary">
          Theo dõi bài đã xuất bản của tác giả và công thức thuộc tài khoản quản trị.
        </p>
      </header>

      <form onSubmit={submitSearch} role="search" className="flex flex-wrap gap-3 rounded-2xl border border-stone-200 bg-white p-4 shadow-subtle sm:p-5">
        <label htmlFor="admin-recipe-search" className="sr-only">Tìm công thức theo tên</label>
        <div className="relative min-w-56 flex-1">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-content-tertiary" aria-hidden="true" />
          <input id="admin-recipe-search" type="search" value={searchInput} onChange={(event) => setSearchInput(event.target.value)} maxLength={100} placeholder="Nhập tên công thức có dấu hoặc không dấu..." className="h-10 w-full rounded-lg border border-stone-300 bg-white pl-10 pr-3 text-sm text-content-primary outline-none placeholder:text-content-tertiary focus:border-brand focus:ring-2 focus:ring-brand/20" />
        </div>
        <Button type="submit" leftIcon={<Search className="h-4 w-4" />}>Tìm kiếm</Button>
        {search && <Button type="button" variant="outline" leftIcon={<X className="h-4 w-4" />} onClick={clearSearch}>Xóa lọc</Button>}
      </form>

      {recipesQuery.isLoading ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <RecipeCardSkeleton key={index} />)}
        </div>
      ) : recipesQuery.isError ? (
        <ErrorState
          title="Không tải được công thức"
          message={getApiErrorMessage(recipesQuery.error)}
          onRetry={() => void recipesQuery.refetch()}
        />
      ) : !result || result.items.length === 0 ? (
        <EmptyState title={search ? "Không tìm thấy công thức" : "Chưa có công thức"} description={search ? `Không có kết quả cho “${search}”. Thử tên có dấu hoặc không dấu khác.` : "Chưa có công thức nào trong danh sách."} />
      ) : (
        <>
          <p role="status" className="text-sm text-content-secondary">{search ? `${result.totalCount} kết quả cho “${search}”` : `${result.totalCount} công thức`}</p>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {result.items.map((recipe) => (
              <div key={recipe.id} className="overflow-hidden rounded-2xl bg-white pb-4 shadow-subtle">
                <ApiRecipeCard recipe={recipe} />
                <div className="flex items-center justify-between gap-2 px-4 pt-3">
                  <Badge size="sm" variant={recipe.status === 1 ? "accent" : "neutral"}>{recipe.status === 1 ? "Đã xuất bản" : recipe.status === 0 ? "Bản nháp" : "Đã lưu trữ"}</Badge>
                  <Link href={`/dashboard/recipes/${encodeURIComponent(recipe.id)}/edit?slug=${encodeURIComponent(recipe.slug)}`}>
                    <Button size="sm" variant="outline" rightIcon={<ArrowUpRight className="h-4 w-4" />}>Chỉnh sửa</Button>
                  </Link>
                </div>
              </div>
            ))}
          </div>
          <Pagination currentPage={result.page} totalPages={result.totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
