"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArrowRight, ChefHat } from "lucide-react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Footer } from "@/components/layout/Footer";
import { Navbar } from "@/components/layout/Navbar";
import { SearchBar } from "@/components/common/SearchBar";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useCategories } from "@/hooks/useCategories";
import { useRecipes } from "@/hooks/useRecipes";

export default function HomePage() {
  const router = useRouter();
  const categoriesQuery = useCategories();
  const recipesQuery = useRecipes({ page: 1, pageSize: 8, sort: "-createdAt" });

  return (
    <div className="flex min-h-screen flex-col bg-white">
      <Navbar />
      <main className="mx-auto w-full max-w-[1280px] flex-1 px-4 py-8 sm:px-6 lg:px-8">
        <section className="grid gap-8 rounded-2xl bg-orange-50 px-6 py-10 sm:px-10 lg:grid-cols-[1fr_auto] lg:items-center">
          <div className="max-w-2xl space-y-4">
            <p className="text-sm font-semibold text-brand">Culinary Blog</p>
            <h1 className="text-3xl font-extrabold tracking-tight text-content-primary sm:text-4xl">
              Món ngon bắt đầu từ cảm hứng mỗi ngày
            </h1>
            <p className="text-sm leading-6 text-content-secondary">
              Khám phá công thức nấu ăn, nguyên liệu và hướng dẫn từng bước từ cộng đồng bếp nhà.
            </p>
            <SearchBar
              showFilterButton={false}
              onSearch={(query) => {
                if (query.trim()) {
                  router.push(`/recipes/search?q=${encodeURIComponent(query.trim())}`);
                }
              }}
            />
          </div>
          <div className="hidden h-36 w-36 items-center justify-center rounded-full bg-white text-brand lg:flex">
            <ChefHat className="h-20 w-20" aria-hidden="true" />
          </div>
        </section>

        <div className="mt-10 grid gap-8 lg:grid-cols-[240px_minmax(0,1fr)]">
          <aside className="h-fit rounded-xl border border-slate-200 bg-white p-5">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="font-bold text-content-primary">Danh mục</h2>
              <Link href="/categories" className="text-xs font-semibold text-brand hover:underline">
                Tất cả
              </Link>
            </div>
            {categoriesQuery.isLoading ? (
              <p className="text-sm text-content-secondary">Đang tải danh mục...</p>
            ) : categoriesQuery.isError ? (
              <p className="text-sm text-rose-700">Không tải được danh mục.</p>
            ) : categoriesQuery.data?.length ? (
              <ul className="space-y-1">
                {categoriesQuery.data.map((category) => (
                  <li key={category.id}>
                    <Link
                      href={`/categories/${encodeURIComponent(category.slug)}`}
                      className="flex items-center justify-between rounded-lg px-3 py-2 text-sm text-content-secondary hover:bg-orange-50 hover:text-brand"
                    >
                      <span>{category.name}</span>
                      <span className="text-xs text-content-tertiary">{category.recipeCount}</span>
                    </Link>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-content-secondary">Chưa có danh mục.</p>
            )}
          </aside>

          <section className="min-w-0 space-y-5">
            <header className="flex items-end justify-between gap-4">
              <div>
                <p className="text-sm font-semibold text-brand">Mới nhất</p>
                <h2 className="mt-1 text-2xl font-bold text-content-primary">Công thức mới</h2>
              </div>
              <Link href="/recipes" className="inline-flex items-center gap-1 text-sm font-semibold text-brand hover:underline">
                Xem tất cả <ArrowRight className="h-4 w-4" />
              </Link>
            </header>

            {recipesQuery.isLoading && (
              <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
                {Array.from({ length: 6 }, (_, index) => <RecipeCardSkeleton key={index} />)}
              </div>
            )}
            {recipesQuery.isError && (
              <ErrorState
                title="Không tải được công thức mới"
                message="Hãy kiểm tra Backend/API rồi thử lại."
                onRetry={() => void recipesQuery.refetch()}
              />
            )}
            {recipesQuery.data?.items.length === 0 && (
              <p className="rounded-xl border border-slate-200 p-8 text-center text-sm text-content-secondary">
                Chưa có công thức được xuất bản.
              </p>
            )}
            {recipesQuery.data && recipesQuery.data.items.length > 0 && (
              <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
                {recipesQuery.data.items.map((recipe) => (
                  <ApiRecipeCard key={recipe.id} recipe={recipe} />
                ))}
              </div>
            )}
          </section>
        </div>
      </main>
      <Footer />
    </div>
  );
}
