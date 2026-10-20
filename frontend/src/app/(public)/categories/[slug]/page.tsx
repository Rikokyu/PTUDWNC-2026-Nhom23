"use client";

import { useCallback, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { MainLayout } from "@/components/layout/main-layout";
import { RecipeGrid } from "@/components/recipes/recipe-grid";
import { ErrorMessage } from "@/components/shared/error-message";
import { Pagination } from "@/components/shared/pagination";
import { Badge } from "@/components/ui/badge";
import { categoryApi } from "@/lib/api/custom-categories";
import type { CategoryDetailDto } from "@/types/custom-api";
import type { RecipeCardItem } from "@/components/recipes/recipe-card";

const pageSize = 9;

export default function CategoryPage() {
  const params = useParams<{ slug: string }>();
  const [category, setCategory] = useState<CategoryDetailDto | null>(null);
  const [items, setItems] = useState<RecipeCardItem[]>([]);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadCategory = useCallback(async (requestedPage: number) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await categoryApi.getBySlug(params.slug, requestedPage, pageSize);
      setCategory(result);
      setPage(result.recipes.page);
      setItems(
        result.recipes.items.map((recipe) => ({
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
    } catch (requestError) {
      setCategory(null);
      setItems([]);
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Không thể tải danh mục công thức.",
      );
    } finally {
      setIsLoading(false);
    }
  }, [params.slug]);

  useEffect(() => {
    const value = Number(new URLSearchParams(window.location.search).get("page") ?? 1);
    const initialPage = Number.isInteger(value) && value > 0 ? value : 1;
    setPage(initialPage);
    void loadCategory(initialPage);
  }, [loadCategory]);

  const changePage = (nextPage: number) => {
    const params = new URLSearchParams(window.location.search);
    if (nextPage > 1) params.set("page", String(nextPage));
    else params.delete("page");
    const query = params.toString();
    window.history.pushState(
      {},
      "",
      `${window.location.pathname}${query ? `?${query}` : ""}`,
    );
    void loadCategory(nextPage);
  };

  return (
    <MainLayout>
      <section className="section-block">
        {category ? (
          <div className="section-heading">
            <div>
              <Badge variant="secondary">{category.category.recipeCount} công thức</Badge>
              <h1>{category.category.name}</h1>
              <p className="muted">{category.category.description}</p>
            </div>
          </div>
        ) : null}
        {error ? (
          <ErrorMessage message={error} onRetry={() => void loadCategory(page)} />
        ) : null}
        {!error ? (
          <RecipeGrid
            items={items}
            isLoading={isLoading}
            emptyTitle="Danh mục này chưa có công thức"
          />
        ) : null}
        {!isLoading && !error && category ? (
          <Pagination
            currentPage={category.recipes.page}
            totalPages={category.recipes.totalPages}
            onPageChange={changePage}
          />
        ) : null}
      </section>
    </MainLayout>
  );
}
