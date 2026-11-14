"use client";

import { useEffect, useState } from "react";
import { ChevronDown, SearchX, SlidersHorizontal } from "lucide-react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { AutocompleteSearch } from "@/components/common/AutocompleteSearch";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { Pagination } from "@/components/ui/Pagination";
import { useCategories } from "@/hooks/useCategories";
import { useRecipes } from "@/hooks/useRecipes";
import { useRecipeSuggestions } from "@/hooks/useRecipeSuggestions";
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
  "-title",
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
  const [maxCookTime, setMaxCookTime] = useState<number>();
  const [minServings, setMinServings] = useState<number>();
  const [urlReady, setUrlReady] = useState(false);
  const [filtersOpen, setFiltersOpen] = useState(false);
  useEffect(() => {
    const read = () => {
      const params = new URLSearchParams(window.location.search);
      const positive = (key: string, minimum = 1) => {
        const raw = params.get(key);
        const value = raw === null || raw === "" ? NaN : Number(raw);
        return Number.isSafeInteger(value) && value >= minimum ? value : undefined;
      };
      setPage(positive("page") ?? 1);
      setCategoryId(params.get("categoryId") ?? "");
      setDifficulty(difficultyOptions.find(value => value === params.get("difficulty")));
      const canonical = `${params.get("sortOrder") === "asc" ? "" : "-"}${params.get("sortBy") ?? "createdAt"}`;
      setSort(sortOptions.find(value => value === canonical) ?? "-createdAt");
      setSearch(params.get("search") ?? "");
      setSearchDraft(params.get("search") ?? "");
      setMaxCookTime(positive("maxCookTime", 0));
      setMinServings(positive("minServings"));
      setFiltersOpen(Boolean(params.get("categoryId") || params.get("difficulty") || params.get("maxCookTime") || params.get("minServings")));
      setUrlReady(true);
    };
    read();
    window.addEventListener("popstate", read);
    return () => window.removeEventListener("popstate", read);
  }, []);
  useEffect(() => {
    if (!urlReady) return;
    const url = new URL(window.location.href);
    const values = { page: String(page), categoryId, difficulty: difficulty ?? "",
      search, sortBy: sort?.replace(/^-/, "") ?? "createdAt",
      sortOrder: sort?.startsWith("-") ? "desc" : "asc",
      maxCookTime: maxCookTime === undefined ? "" : String(maxCookTime),
      minServings: minServings === undefined ? "" : String(minServings) };
    for (const [key, value] of Object.entries(values)) {
      if (value) url.searchParams.set(key, value);
      else url.searchParams.delete(key);
    }
    url.searchParams.delete("sort");
    window.history.replaceState(null, "", url);
  }, [urlReady, page, categoryId, difficulty, search, sort, maxCookTime, minServings]);
  const recipesQuery = useRecipes({
    page,
    pageSize: 12,
    maxCookTime,
    minServings,
    categoryId: categoryId || undefined,
    difficulty,
    sort,
    search: search || undefined,
  });
  const categoriesQuery = useCategories();
  const recipeSuggestions = useRecipeSuggestions(searchDraft);
  const result = recipesQuery.data;
  const activeFilterCount = Number(Boolean(categoryId)) + Number(Boolean(difficulty))
    + Number(maxCookTime !== undefined) + Number(minServings !== undefined)
    + Number(sort !== "-createdAt");

  return (
    <div className="space-y-6 pb-10">
      <header className="border-b border-stone-300 pb-5 sm:pb-6">
        <p className="editorial-kicker mb-3">Culinary Blog / Khám phá</p>
        <h1 className="font-display text-4xl font-bold leading-tight tracking-tight text-content-primary sm:text-5xl">
          Khám phá công thức
        </h1>
        <p className="mt-3 max-w-2xl text-base leading-7 text-content-secondary">
          Tìm món hợp với bữa ăn hôm nay.
        </p>
      </header>

      <section
        aria-label="Lọc công thức"
        className="space-y-4 rounded-2xl border border-stone-200/80 bg-surface p-4 shadow-subtle sm:p-5"
      >
        <AutocompleteSearch
          label="Tìm công thức"
          placeholder="Tìm theo tên món hoặc nguyên liệu..."
          value={searchDraft}
          onChange={setSearchDraft}
          onSearch={(value) => {
            setSearch(value);
            setPage(1);
          }}
          suggestions={recipeSuggestions.suggestions}
          isLoading={recipeSuggestions.isLoading}
        />
        <button type="button" className="flex min-h-11 w-full items-center justify-between rounded-xl border border-stone-200 bg-surface-cream px-3 text-sm font-semibold text-content-primary lg:hidden" aria-expanded={filtersOpen} aria-controls="recipe-filters" onClick={() => setFiltersOpen((open) => !open)}>
          <span className="inline-flex items-center gap-2"><SlidersHorizontal className="h-4 w-4 text-accent" aria-hidden="true" /> Bộ lọc{activeFilterCount ? ` (${activeFilterCount})` : ""}</span>
          <ChevronDown className={`h-4 w-4 transition-transform ${filtersOpen ? "rotate-180" : ""}`} aria-hidden="true" />
        </button>
        <div id="recipe-filters" className={`${filtersOpen ? "grid" : "hidden"} gap-x-4 gap-y-3 sm:grid-cols-2 lg:grid lg:grid-cols-5`}>
        <label className="text-xs font-semibold text-content-secondary">
          Danh mục
          <select
            value={categoryId}
            onChange={(event) => {
              setCategoryId(event.target.value);
              setPage(1);
            }}
            className="mt-2 min-h-11 w-full rounded-xl border border-stone-300 bg-surface-cream px-3 py-2.5 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
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
            className="mt-2 min-h-11 w-full rounded-xl border border-stone-300 bg-surface-cream px-3 py-2.5 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
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
            className="mt-2 min-h-11 w-full rounded-xl border border-stone-300 bg-surface-cream px-3 py-2.5 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
          >
            <option value="-createdAt">Mới nhất</option>
            <option value="createdAt">Cũ nhất</option>
            <option value="title">Tên A–Z</option>
            <option value="-title">Tên Z–A</option>
            <option value="cookTime">Nấu nhanh</option>
            <option value="-cookTime">Nấu lâu</option>
          </select>
        </label>
        <label className="text-xs font-semibold text-content-secondary">
          Thời gian nấu tối đa (phút)
          <input type="number" min={0} step={1} value={maxCookTime ?? ""}
            onChange={(event) => {
              const value = event.target.valueAsNumber;
              setMaxCookTime(Number.isSafeInteger(value) && value >= 0 ? value : undefined);
              setPage(1);
            }}
            className="mt-2 min-h-11 w-full rounded-xl border border-stone-300 bg-surface-cream px-3 py-2.5 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
            placeholder="Không giới hạn" />
        </label>
        <label className="text-xs font-semibold text-content-secondary">
          Số khẩu phần tối thiểu
          <input type="number" min={1} step={1} value={minServings ?? ""}
            onChange={(event) => {
              const value = event.target.valueAsNumber;
              setMinServings(Number.isSafeInteger(value) && value >= 1 ? value : undefined);
              setPage(1);
            }}
            className="mt-2 min-h-11 w-full rounded-xl border border-stone-300 bg-surface-cream px-3 py-2.5 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
            placeholder="Mọi khẩu phần" />
        </label>
        </div>
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
          icon={<SearchX className="h-7 w-7" aria-hidden="true" />}
          title="Chưa có món nào phù hợp"
          description="Thử đổi từ khóa hoặc bỏ bớt điều kiện lọc để xem thêm công thức nhé."
          actionLabel="Xem tất cả công thức"
          onAction={() => {
            setMaxCookTime(undefined);
            setMinServings(undefined);
            setSort("-createdAt");
            setCategoryId("");
            setDifficulty(undefined);
            setSearchDraft("");
            setSearch("");
            setPage(1);
          }}
        />
      ) : (
        <>
          <div className="flex items-center gap-4 border-b border-stone-300 pb-4">
            <p className="font-display text-lg font-semibold text-content-primary">{result.totalCount} công thức</p>
            <span className="h-px flex-1 bg-stone-200" aria-hidden="true" />
          </div>
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
