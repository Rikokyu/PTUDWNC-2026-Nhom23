"use client";

import { useState } from "react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Button } from "@/components/ui/Button";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { Pagination } from "@/components/ui/Pagination";
import { useCategories } from "@/hooks/useCategories";
import { useRecipes } from "@/hooks/useRecipes";
import { getApiErrorMessage } from "@/lib/api/error";
import type { RecipeFilters } from "@/types/api";

const difficultyOptions: NonNullable<RecipeFilters["difficulty"]>[] = [
  "Easy",
  "Medium",
  "Hard",
  "Expert",
];
const sortOptions: NonNullable<RecipeFilters["sort"]>[] = [
  "-createdAt",
  "createdAt",
  "title",
  "cookTime",
  "-cookTime",
];

export function RecipeExplorer() {
  const [page, setPage] = useState(1);
  const [categoryId, setCategoryId] = useState("");
  const [difficulty, setDifficulty] = useState<RecipeFilters["difficulty"]>();
  const [sort, setSort] = useState<RecipeFilters["sort"]>("-createdAt");
  const [searchDraft, setSearchDraft] = useState("");
  const [search, setSearch] = useState("");
  const recipesQuery = useRecipes({
    page,
    pageSize: 9,
    categoryId: categoryId || undefined,
    difficulty,
    sort,
    search: search || undefined,
  });
  const categoriesQuery = useCategories();
  const result = recipesQuery.data;

  return (
    <div className="space-y-8">
      <header className="space-y-2">
        <p className="text-sm font-semibold text-brand">Culinary Blog</p>
        <h1 className="text-3xl font-extrabold tracking-tight text-content-primary">
          Khám phá công thức
        </h1>
        <p className="max-w-2xl text-sm leading-6 text-content-secondary">
          Tìm cảm hứng cho bữa ăn tiếp theo qua những công thức được lưu trong hệ thống.
        </p>
      </header>

      <section
        aria-label="Lọc công thức"
        className="grid gap-3 rounded-xl border border-slate-200 bg-white p-4 sm:grid-cols-2 lg:grid-cols-4"
      >
        <form
          className="flex items-end gap-2 lg:col-span-1"
          onSubmit={(event) => {
            event.preventDefault();
            setSearch(searchDraft.trim());
            setPage(1);
          }}
        >
          <label className="flex-1 text-xs font-semibold text-content-secondary">
            Tìm kiếm
            <input
              value={searchDraft}
              onChange={(event) => setSearchDraft(event.target.value)}
              placeholder="Tên món hoặc nguyên liệu"
              className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm font-normal text-content-primary"
            />
          </label>
          <Button type="submit" size="sm">Tìm</Button>
        </form>
        <label className="text-xs font-semibold text-content-secondary">
          Danh mục
          <select
            value={categoryId}
            onChange={(event) => {
              setCategoryId(event.target.value);
              setPage(1);
            }}
            className="mt-1.5 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-normal text-content-primary"
          >
            <option value="">Tất cả danh mục</option>
            {(categoriesQuery.data ?? []).map((category) => (
              <option key={category.id} value={category.id}>{category.name}</option>
            ))}
          </select>
        </label>
        <label className="text-xs font-semibold text-content-secondary">
          Độ khó
          <select
            value={difficulty ?? ""}
            onChange={(event) => {
              setDifficulty(difficultyOptions.find((option) => option === event.target.value));
              setPage(1);
            }}
            className="mt-1.5 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-normal text-content-primary"
          >
            <option value="">Mọi mức độ</option>
            <option value="Easy">Dễ</option>
            <option value="Medium">Trung bình</option>
            <option value="Hard">Khó</option>
            <option value="Expert">Chuyên gia</option>
          </select>
        </label>
        <label className="text-xs font-semibold text-content-secondary">
          Sắp xếp
          <select
            value={sort}
            onChange={(event) => {
              setSort(sortOptions.find((option) => option === event.target.value) ?? "-createdAt");
              setPage(1);
            }}
            className="mt-1.5 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-normal text-content-primary"
          >
            <option value="-createdAt">Mới nhất</option>
            <option value="createdAt">Cũ nhất</option>
            <option value="title">Tên A–Z</option>
            <option value="cookTime">Nấu nhanh</option>
            <option value="-cookTime">Nấu lâu</option>
          </select>
        </label>
      </section>

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
        <EmptyState
          title="Không tìm thấy công thức"
          description="Hãy thử từ khóa hoặc bộ lọc khác."
          actionLabel="Xóa bộ lọc"
          onAction={() => {
            setCategoryId("");
            setDifficulty(undefined);
            setSearchDraft("");
            setSearch("");
            setPage(1);
          }}
        />
      ) : (
        <>
          <p className="text-sm text-content-tertiary">Tìm thấy {result.totalCount} công thức</p>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {result.items.map((recipe) => <ApiRecipeCard key={recipe.id} recipe={recipe} />)}
          </div>
          <Pagination
            currentPage={result.page}
            totalPages={result.totalPages}
            onPageChange={setPage}
          />
        </>
      )}

      {categoriesQuery.isError && (
        <p role="status" className="text-sm text-rose-700">
          Không tải được danh mục; bộ lọc danh mục hiện không khả dụng.
        </p>
      )}
    </div>
  );
}
