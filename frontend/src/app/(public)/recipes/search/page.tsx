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
  };
}

function updateSearchUrl(query: string, page: number, replace = false) {
  const params = new URLSearchParams(window.location.search);
  if (query) params.set("q", query);
  else params.delete("q");
  if (page > 1) params.set("page", String(page));
  else params.delete("page");
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
  const requestSequence = useRef(0);

  const loadSearch = useCallback(async (value: string, requestedPage: number) => {
    const trimmed = value.trim();
    const requestId = ++requestSequence.current;
    setQuery(trimmed);
    setPage(requestedPage);
    if (!trimmed) {
      setItems([]);
      setTotalPages(0);
      setError(null);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const results = await searchRecipes({
        q: trimmed,
        page: requestedPage,
        pageSize: 10,
      });
      if (requestId !== requestSequence.current) return;
      if (results.totalPages > 0 && requestedPage > results.totalPages) {
        const lastPage = results.totalPages;
        setPage(lastPage);
        updateSearchUrl(trimmed, lastPage, true);
        return;
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
    } catch (requestError) {
      if (requestId !== requestSequence.current) return;
      const message =
        requestError instanceof Error ? requestError.message : "Không thể tìm kiếm công thức.";
      setError(message);
      setItems([]);
      setTotalPages(0);
    } finally {
      if (requestId === requestSequence.current) setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const initial = queryFromUrl();
    if (initial.query) void loadSearch(initial.query, initial.page);
    const onPopState = () => {
      const next = queryFromUrl();
      setQuery(next.query);
      setPage(next.page);
      void loadSearch(next.query, next.page);
    };
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, [loadSearch]);

  const submitSearch = () => {
    const trimmed = query.trim();
    updateSearchUrl(trimmed, 1);
    void loadSearch(trimmed, 1);
  };

  const changePage = (nextPage: number) => {
    updateSearchUrl(query, nextPage);
    void loadSearch(query, nextPage);
  };

  return (
    <MainLayout>
      <div className="section-block">
        <div className="section-heading">
          <div>
            <h1>Tìm kiếm công thức</h1>
            <p className="muted">Tìm theo tên món hoặc từ khóa tiếng Việt.</p>
          </div>
        </div>

        <SearchBar
          value={query}
          onChange={setQuery}
          onSubmit={submitSearch}
          placeholder="Ví dụ: pho bo, canh chua, bun cha"
        />

        {error ? <ErrorMessage message={error} onRetry={() => void loadSearch(query, page)} /> : null}
        <RecipeGrid
          items={items}
          isLoading={isLoading}
          emptyTitle={query ? "Không tìm thấy công thức phù hợp" : "Nhập từ khóa để tìm công thức"}
        />
        {!isLoading && !error ? (
          <Pagination currentPage={page} totalPages={totalPages} onPageChange={changePage} />
        ) : null}
      </div>
    </MainLayout>
  );
}
