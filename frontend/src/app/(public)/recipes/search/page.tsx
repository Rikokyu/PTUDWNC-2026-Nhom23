"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { MainLayout } from "@/components/layout/main-layout";
import { RecipeGrid } from "@/components/recipes/recipe-grid";
import { ErrorMessage } from "@/components/shared/error-message";
import { Pagination } from "@/components/shared/pagination";
import { SearchBar } from "@/components/shared/search-bar";
import { searchRecipes } from "@/lib/api/search";
import type { RecipeCardItem } from "@/components/recipes/recipe-card";

function queryFromUrl() {
  const params = new URLSearchParams(window.location.search);
  const page = Number(params.get("page") ?? 1);
  return {
    query: params.get("q") ?? "",
    page: Number.isInteger(page) && page > 0 ? page : 1,
    sort: params.get("sort") ?? "relevance",
  };
}

function updateSearchUrl(
  query: string,
  page: number,
  sort = "relevance",
  replace = false,
) {
  const params = new URLSearchParams(window.location.search);
  if (query) params.set("q", query);
  else params.delete("q");
  if (page > 1) params.set("page", String(page));
  else params.delete("page");
  if (sort !== "relevance") params.set("sort", sort);
  else params.delete("sort");
  const search = params.toString();
  const url = `${window.location.pathname}${search ? `?${search}` : ""}`;
  if (replace) window.history.replaceState({}, "", url);
  else window.history.pushState({}, "", url);
}

export default function RecipeSearchPage() {
  const [query, setQuery] = useState("");
  const [items, setItems] = useState<RecipeCardItem[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sort, setSort] = useState("relevance");
  const [emptyMessage, setEmptyMessage] = useState<string>();
  const requestSequence = useRef(0);

  const loadSearch = useCallback(
    async (
      value: string,
      requestedPage: number,
      requestedSort = "relevance",
    ) => {
      const trimmed = value.trim().normalize("NFC");
      const requestId = ++requestSequence.current;
      setQuery(trimmed);
      setPage(requestedPage);
      setSort(requestedSort);
      setEmptyMessage(undefined);
      if (!trimmed) {
        setItems([]);
        setTotalPages(0);
        setError(null);
        setIsLoading(false);
        return;
      }

      if (trimmed.length < 2 || trimmed.length > 200) {
        setItems([]);
        setTotalPages(0);
        setError("Nhập từ khóa từ 2 đến 200 ký tự.");
        setIsLoading(false);
        return;
      }

      setIsLoading(true);
      setError(null);
      try {
        let results = await searchRecipes({
          q: trimmed,
          page: requestedPage,
          pageSize: 10,
          sort: requestedSort,
        });
        if (requestId !== requestSequence.current) return;
        if (results.totalPages > 0 && requestedPage > results.totalPages) {
          const lastPage = results.totalPages;
          setPage(lastPage);
          updateSearchUrl(trimmed, lastPage, requestedSort, true);
          results = await searchRecipes({
            q: trimmed,
            page: lastPage,
            pageSize: 10,
            sort: requestedSort,
          });
          if (requestId !== requestSequence.current) return;
        }
        setItems(
          results.items.map((recipe) => ({
            id: recipe.id,
            title: recipe.title,
            slug: recipe.slug,
            description: recipe.title,
            thumbnailUrl: recipe.thumbnailUrl,
            cookTimeMinutes: recipe.cookTimeMinutes,
            difficulty: recipe.difficulty,
            categoryName: recipe.categoryName,
          })),
        );
        setTotalPages(results.totalPages);
        setEmptyMessage(results.message);
      } catch (requestError) {
        if (requestId !== requestSequence.current) return;
        const message =
          requestError instanceof Error
            ? requestError.message
            : "Không thể tìm kiếm công thức.";
        setError(message);
        setItems([]);
        setTotalPages(0);
      } finally {
        if (requestId === requestSequence.current) setIsLoading(false);
      }
    },
    [],
  );

  useEffect(() => {
    const initial = queryFromUrl();
    if (initial.query)
      void loadSearch(initial.query, initial.page, initial.sort);
    const onPopState = () => {
      const next = queryFromUrl();
      setQuery(next.query);
      setPage(next.page);
      void loadSearch(next.query, next.page, next.sort);
    };
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, [loadSearch]);

  const submitSearch = () => {
    const trimmed = query.trim();
    updateSearchUrl(trimmed, 1, sort);
    void loadSearch(trimmed, 1, sort);
  };

  const changePage = (nextPage: number) => {
    updateSearchUrl(query, nextPage, sort);
    void loadSearch(query, nextPage, sort);
  };

  return (
    <MainLayout>
      <div className="section-block">
        <div className="section-heading">
          <div>
            <h1>Tìm kiếm công thức</h1>
            <p className="muted">
              Tìm từ 2 đến 200 ký tự, có hoặc không dấu tiếng Việt.
            </p>
          </div>
        </div>

        <SearchBar
          value={query}
          onChange={setQuery}
          onSubmit={submitSearch}
          placeholder="Ví dụ: pho bo, canh chua, bun cha"
        />

        <div className="my-4 flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm text-slate-500">
            Mặc định ưu tiên công thức phù hợp nhất với từ khóa.
          </p>
          <label
            className="flex items-center gap-2 text-sm"
            htmlFor="search-sort"
          >
            Sắp xếp
            <select
              id="search-sort"
              value={sort}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2"
              onChange={(event) => {
                const nextSort = event.target.value;
                updateSearchUrl(query, 1, nextSort);
                void loadSearch(query, 1, nextSort);
              }}
            >
              <option value="relevance">Phù hợp nhất</option>
              <option value="-createdAt">Mới nhất</option>
              <option value="createdAt">Cũ nhất</option>
              <option value="title">Tên món A–Z</option>
              <option value="cookTime">Thời gian nấu tăng dần</option>
            </select>
          </label>
        </div>
        {error ? (
          <ErrorMessage
            message={error}
            onRetry={() => void loadSearch(query, page, sort)}
          />
        ) : null}
        <RecipeGrid
          items={items}
          isLoading={isLoading}
          emptyDescription={emptyMessage}
          emptyTitle={
            query
              ? "Không tìm thấy công thức phù hợp"
              : "Nhập từ khóa để tìm công thức"
          }
        />
        {!isLoading && !error ? (
          <Pagination
            currentPage={page}
            totalPages={totalPages}
            onPageChange={changePage}
          />
        ) : null}
      </div>
    </MainLayout>
  );
}
