"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { MainLayout } from "@/components/layout/main-layout";
import { RecipeGrid } from "@/components/recipes/recipe-grid";
import { ErrorMessage } from "@/components/shared/error-message";
import { Pagination } from "@/components/shared/pagination";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { recipeApi } from "@/lib/api/recipes";
import type { RecipeCardItem } from "@/components/recipes/recipe-card";

const pageSize = 9;

function readPage() {
  const value = Number(new URLSearchParams(window.location.search).get("page") ?? 1);
  return Number.isInteger(value) && value > 0 ? value : 1;
}

function updateUrl(page: number, sort: string, replace = false) {
  const params = new URLSearchParams(window.location.search);
  if (page > 1) params.set("page", String(page));
  else params.delete("page");
  if (sort !== "-createdAt") params.set("sort", sort);
  else params.delete("sort");
  const query = params.toString();
  const url = `${window.location.pathname}${query ? `?${query}` : ""}`;
  if (replace) window.history.replaceState({}, "", url);
  else window.history.pushState({}, "", url);
}

export default function RecipesPage() {
  const [items, setItems] = useState<RecipeCardItem[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [sort, setSort] = useState("-createdAt");
  const [initialized, setInitialized] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadRecipes = useCallback(async (requestedPage: number, requestedSort: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await recipeApi.getList({
        page: requestedPage,
        pageSize,
        sort: requestedSort,
      });
      if (result.totalPages > 0 && requestedPage > result.totalPages) {
        const lastPage = result.totalPages;
        setPage(lastPage);
        updateUrl(lastPage, requestedSort, true);
        return;
      }
      if (result.totalPages === 0 && requestedPage > 1) {
        setPage(1);
        updateUrl(1, requestedSort, true);
      }
      setItems(result.items.map((recipe) => ({
        id: recipe.id,
        title: recipe.title,
        slug: recipe.slug,
        description: recipe.title,
        thumbnailUrl: recipe.thumbnailUrl,
        cookTimeMinutes: recipe.cookTimeMinutes,
        difficulty: recipe.difficulty,
        categoryName: recipe.categoryName,
      })));
      setTotalPages(result.totalPages);
    } catch (requestError) {
      const message =
        requestError instanceof Error ? requestError.message : "Không thể tải công thức.";
      setError(message);
      setItems([]);
      setTotalPages(0);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const requestedSort = params.get("sort") ?? "-createdAt";
    setPage(readPage());
    setSort(requestedSort);
    setInitialized(true);

    const onPopState = () => {
      setPage(readPage());
      setSort(new URLSearchParams(window.location.search).get("sort") ?? "-createdAt");
    };
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, []);

  useEffect(() => {
    if (initialized) void loadRecipes(page, sort);
  }, [initialized, loadRecipes, page, sort]);

  const changePage = (nextPage: number) => {
    updateUrl(nextPage, sort);
    setPage(nextPage);
  };

  const changeSort = (nextSort: string) => {
    updateUrl(1, nextSort);
    setSort(nextSort);
    setPage(1);
  };

  return (
    <MainLayout>
      <div className="section-block">
        <div className="section-heading">
          <div>
            <h1>Công thức</h1>
            <p className="muted">Khám phá công thức nấu ăn từ dữ liệu mới nhất.</p>
          </div>
          <Button asChild className="button-primary">
            <Link href="/recipes/search">Tìm kiếm nâng cao</Link>
          </Button>
        </div>

        <div className="list-toolbar">
          <label htmlFor="recipe-sort">Sắp xếp</label>
          <Select value={sort} onValueChange={changeSort}>
            <SelectTrigger id="recipe-sort" aria-label="Sắp xếp công thức">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="-createdAt">Mới nhất</SelectItem>
              <SelectItem value="createdAt">Cũ nhất</SelectItem>
              <SelectItem value="title">Tên A–Z</SelectItem>
              <SelectItem value="-title">Tên Z–A</SelectItem>
              <SelectItem value="cookTime">Nấu nhanh nhất</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {error ? <ErrorMessage message={error} onRetry={() => void loadRecipes(page, sort)} /> : null}
        <RecipeGrid items={items} isLoading={isLoading} emptyTitle="Chưa có công thức nào" />
        {!isLoading && !error ? (
          <Pagination currentPage={page} totalPages={totalPages} onPageChange={changePage} />
        ) : null}
      </div>
    </MainLayout>
  );
}
