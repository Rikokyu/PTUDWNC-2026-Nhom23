"use client";

import { useMemo, useState } from "react";
import { useCategories } from "@/hooks/useCategories";
import { useRecipes } from "@/hooks/useRecipes";
import { MOCK_RECIPES } from "@/lib/mockData";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { ErrorState } from "@/components/legacy-ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/legacy-ui/LoadingState";
import { Pagination } from "@/components/legacy-ui/Pagination";
import type { RecipeApiSummaryDto, RecipeFilters } from "@/types/api";

const demoEnabled = process.env.NEXT_PUBLIC_USE_DEMO_DATA === "true";
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

function getDemoRecipes(): RecipeApiSummaryDto[] {
  return MOCK_RECIPES.map((recipe) => ({
    id: recipe.id,
    title: recipe.title,
    slug: recipe.slug,
    thumbnailUrl: recipe.primaryImageUrl ?? null,
    categoryName: recipe.categoryName,
    categoryId: "",
    prepTimeMinutes: recipe.prepTimeMinutes,
    cookTimeMinutes: recipe.cookTimeMinutes,
    servings: recipe.servings,
    difficulty:
      recipe.difficulty === "Easy" ? 1 : recipe.difficulty === "Hard" ? 3 : 2,
    status: 1,
    createdAt: recipe.createdAt,
  }));
}

export function RecipeExplorer() {
  const [page, setPage] = useState(1);
  const [categoryId, setCategoryId] = useState("");
  const [difficulty, setDifficulty] = useState<RecipeFilters["difficulty"]>();
  const [sort, setSort] = useState<RecipeFilters["sort"]>("-createdAt");
  const [search, setSearch] = useState("");
  const filters: RecipeFilters = {
    page,
    pageSize: 9,
    categoryId: categoryId || undefined,
    difficulty,
    sort,
  };
  const recipesQuery = useRecipes(filters);
  const categoriesQuery = useCategories();
  const isDemoFallback = demoEnabled && recipesQuery.isError;
  const categoryName = categoriesQuery.data?.find(
    (category) => category.id === categoryId,
  )?.name;
  const recipes = isDemoFallback
    ? getDemoRecipes()
    : (recipesQuery.data?.items ?? []);
  const filteredRecipes = useMemo(() => {
    const matches = recipes.filter((recipe) => {
      const matchesSearch = recipe.title
        .toLocaleLowerCase()
        .includes(search.trim().toLocaleLowerCase());
      const matchesCategory =
        !isDemoFallback || !categoryName || recipe.categoryName === categoryName;
      const matchesDifficulty =
        !isDemoFallback || !difficulty || recipe.difficulty ===
          (difficulty === "Easy" ? 1 : difficulty === "Medium" ? 2 : difficulty === "Hard" ? 3 : 4);
      return matchesSearch && matchesCategory && matchesDifficulty;
    });
    if (!isDemoFallback) return matches;
    return matches.sort((left, right) => {
      if (sort === "title") return left.title.localeCompare(right.title);
      if (sort === "cookTime") {
        return left.cookTimeMinutes - right.cookTimeMinutes;
      }
      if (sort === "-cookTime") {
        return right.cookTimeMinutes - left.cookTimeMinutes;
      }
      return new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime();
    });
  }, [recipes, search, isDemoFallback, categoryName, difficulty, sort]);

  return (
    <div className="space-y-8">
      <header className="space-y-2">
        <p className="text-sm font-semibold text-brand">Culinary Blog</p>
        <h1 className="text-3xl font-extrabold tracking-tight text-content-primary">
          Khám phá công thức
        </h1>
        <p className="max-w-2xl text-sm leading-6 text-content-secondary">
          Tìm cảm hứng cho bữa ăn tiếp theo qua những công thức dễ theo dõi và
          nguyên liệu quen thuộc.
        </p>
      </header>

      {isDemoFallback && (
        <p role="status" className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-900">
          Đang hiển thị dữ liệu mẫu vì API chưa kết nối được. Đây không phải dữ liệu từ máy chủ.
        </p>
      )}

      <section
        aria-label="Lọc công thức"
        className="grid gap-3 rounded-xl border border-slate-200 bg-white p-4 sm:grid-cols-2 lg:grid-cols-4"
      >
        <label className="text-xs font-semibold text-content-secondary">
          Tìm trong kết quả hiện tại
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Tên công thức"
            className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm font-normal text-content-primary"
          />
        </label>
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
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </label>
        <label className="text-xs font-semibold text-content-secondary">
          Độ khó
          <select
            value={difficulty ?? ""}
            onChange={(event) => {
              setDifficulty(
                difficultyOptions.find((option) => option === event.target.value),
              );
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
              setSort(
                sortOptions.find((option) => option === event.target.value) ??
                  "-createdAt",
              );
              setPage(1);
            }}
            className="mt-1.5 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-normal text-content-primary"
          >
            <option value="-createdAt">Mới nhất</option>
            <option value="createdAt">Cũ nhất</option>
            <option value="title">Tên A–Z</option>
            <option value="-title">Tên Z–A</option>
            <option value="cookTime">Nấu nhanh</option>
            <option value="-cookTime">Nấu lâu</option>
          </select>
        </label>
      </section>

      {recipesQuery.isLoading && (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => (
            <RecipeCardSkeleton key={index} />
          ))}
        </div>
      )}
      {recipesQuery.isError && !isDemoFallback && (
        <ErrorState
          title="Không tải được công thức"
          message="Hãy kiểm tra địa chỉ API và trạng thái backend rồi thử lại."
          onRetry={() => void recipesQuery.refetch()}
        />
      )}
      {!recipesQuery.isLoading && !recipesQuery.isError && filteredRecipes.length === 0 && (
        <p className="rounded-xl border border-slate-200 bg-white p-8 text-center text-sm text-content-secondary">
          Không tìm thấy công thức phù hợp.
        </p>
      )}
      {(!recipesQuery.isError || isDemoFallback) && filteredRecipes.length > 0 && (
        <>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {filteredRecipes.map((recipe) => (
              <ApiRecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>
          {!isDemoFallback && (
            <Pagination
              currentPage={recipesQuery.data?.page ?? page}
              totalPages={recipesQuery.data?.totalPages ?? 1}
              onPageChange={setPage}
            />
          )}
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
