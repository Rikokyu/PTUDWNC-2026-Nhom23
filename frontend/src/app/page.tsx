"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArrowRight, Clock } from "lucide-react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Footer } from "@/components/layout/Footer";
import { Navbar } from "@/components/layout/Navbar";
import { SearchBar } from "@/components/common/SearchBar";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useCategories } from "@/hooks/useCategories";
import { useRecipes } from "@/hooks/useRecipes";

const QUICK_SEARCHES = ["Phở", "Bún chả", "Bánh mì", "Salad", "Cơm tấm"];

export default function HomePage() {
  const router = useRouter();
  const categoriesQuery = useCategories();
  const recipesQuery = useRecipes({ page: 1, pageSize: 8, sort: "-createdAt" });
  const featuredRecipe = recipesQuery.data?.items[0];
  const gridRecipes = recipesQuery.data?.items.slice(1) ?? [];

  return (
    <div className="flex min-h-screen flex-col bg-surface-muted">
      <Navbar />
      <main className="flex-1">
        <section className="relative isolate min-h-[min(82vh,760px)] overflow-hidden bg-content-primary text-white">
          <img
            src="/images/hero-pho-editorial.webp"
            alt="Bát phở bò với rau thơm trong gian bếp Việt"
            className="absolute inset-0 h-full w-full object-cover motion-safe:animate-hero-zoom"
            fetchPriority="high"
          />
          <div className="absolute inset-0 bg-black/35 sm:bg-gradient-to-r sm:from-black/55 sm:via-black/20 sm:to-transparent" />
          <div className="absolute inset-0 bg-gradient-to-t from-black/40 via-transparent to-black/10" />

          <div className="relative mx-auto flex min-h-[min(82vh,760px)] w-full max-w-[1280px] flex-col justify-center px-4 pb-14 pt-28 sm:px-6 lg:px-8">
            <div className="max-w-2xl space-y-6 motion-safe:animate-fade-up">
              <p className="text-xs font-bold uppercase tracking-[0.28em] text-white/80">
                Câu chuyện từ căn bếp Việt
              </p>
              <h1 className="font-display max-w-xl text-4xl font-bold leading-[1.14] tracking-tight text-white sm:text-5xl lg:text-6xl">
                Hôm nay nấu món gì?
              </h1>
              <p className="max-w-lg text-base leading-7 text-white/80 sm:text-lg">
                Khám phá công thức thân quen, tìm cảm hứng mới và cùng nhau nấu những bữa ăn đáng nhớ.
              </p>

              <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                <Link
                  href="/recipes"
                  className="inline-flex h-12 items-center justify-center rounded-full bg-brand px-7 text-sm font-bold text-white transition hover:bg-brand-hover"
                >
                  Khám phá công thức
                </Link>
                {featuredRecipe ? (
                  <Link
                    href={`/recipes/${encodeURIComponent(featuredRecipe.slug)}`}
                    className="inline-flex h-12 items-center justify-center gap-2 rounded-full border border-white/35 bg-white/10 px-6 text-sm font-semibold text-white backdrop-blur-md transition hover:bg-white/20"
                  >
                    Xem món nổi bật
                    <ArrowRight className="h-4 w-4" aria-hidden="true" />
                  </Link>
                ) : null}
              </div>

              <div className="max-w-xl pt-2">
                <SearchBar
                  showFilterButton={false}
                  className="rounded-full border-white/20 bg-white/95 shadow-none"
                  onSearch={(query) => {
                    if (query.trim()) {
                      router.push(`/recipes/search?q=${encodeURIComponent(query.trim())}`);
                    }
                  }}
                />
                <div className="mt-3 flex flex-wrap items-center gap-2">
                  <span className="text-xs font-medium text-white/60">Gợi ý:</span>
                  {QUICK_SEARCHES.map((term) => (
                    <button
                      key={term}
                      type="button"
                      onClick={() => router.push(`/recipes/search?q=${encodeURIComponent(term)}`)}
                      className="rounded-full border border-white/25 bg-white/10 px-3 py-1 text-xs font-medium text-white/90 transition hover:bg-white/20"
                    >
                      {term}
                    </button>
                  ))}
                </div>
              </div>

              {featuredRecipe ? (
                <p className="inline-flex items-center gap-2 text-sm text-white/70">
                  <Clock className="h-4 w-4" aria-hidden="true" />
                  {featuredRecipe.title} · {featuredRecipe.prepTimeMinutes + featuredRecipe.cookTimeMinutes} phút
                </p>
              ) : null}
            </div>
          </div>
        </section>

        <section className="border-b border-zinc-200/80 bg-white">
          <div className="mx-auto w-full max-w-[1280px] px-4 py-8 sm:px-6 lg:px-8">
            <div className="mb-5 flex items-end justify-between gap-3">
              <div>
                <h2 className="font-display text-2xl font-bold text-content-primary">Danh mục</h2>
                <p className="mt-1 text-sm text-content-secondary">Chọn nhóm món để bắt đầu.</p>
              </div>
              <Link href="/categories" className="text-sm font-semibold text-brand hover:underline">
                Tất cả
              </Link>
            </div>
            {categoriesQuery.isLoading ? (
              <p className="text-sm text-content-secondary">Đang tải danh mục...</p>
            ) : categoriesQuery.isError ? (
              <p className="text-sm text-rose-700">Không tải được danh mục.</p>
            ) : categoriesQuery.data?.length ? (
              <div className="-mx-1 flex gap-3 overflow-x-auto px-1 pb-1">
                {categoriesQuery.data.map((category) => (
                  <Link
                    key={category.id}
                    href={`/categories/${encodeURIComponent(category.slug)}`}
                    className="group inline-flex shrink-0 items-center gap-3 border border-zinc-200 bg-surface-muted px-4 py-3 transition hover:border-brand/40 hover:bg-brand-light"
                  >
                    <span className="text-sm font-semibold text-content-primary group-hover:text-brand">
                      {category.name}
                    </span>
                    <span className="text-xs tabular-nums text-content-tertiary">{category.recipeCount}</span>
                  </Link>
                ))}
              </div>
            ) : (
              <p className="text-sm text-content-secondary">Chưa có danh mục.</p>
            )}
          </div>
        </section>

        <section className="mx-auto w-full max-w-[1280px] px-4 py-12 sm:px-6 lg:px-8">
          <header className="mb-8 flex items-end justify-between gap-4">
            <div>
              <h2 className="font-display text-3xl font-bold text-content-primary">Công thức mới</h2>
              <p className="mt-1 text-sm text-content-secondary">Món vừa xuất bản từ cộng đồng.</p>
            </div>
            <Link
              href="/recipes"
              className="inline-flex items-center gap-1 text-sm font-semibold text-brand hover:underline"
            >
              Xem tất cả <ArrowRight className="h-4 w-4" />
            </Link>
          </header>

          {recipesQuery.isLoading && (
            <div className="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
              {Array.from({ length: 6 }, (_, index) => (
                <RecipeCardSkeleton key={index} />
              ))}
            </div>
          )}
          {recipesQuery.isError && (
            <ErrorState
              title="Không tải được công thức mới"
              message="Chưa thể tải món ăn lúc này. Bạn hãy thử lại sau ít phút."
              onRetry={() => void recipesQuery.refetch()}
            />
          )}
          {recipesQuery.data?.items.length === 0 && (
            <p className="border border-zinc-200 bg-white p-8 text-center text-sm text-content-secondary">
              Chưa có công thức được xuất bản.
            </p>
          )}
          {recipesQuery.data && recipesQuery.data.items.length > 0 && (
            <div className="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
              {(gridRecipes.length > 0 ? gridRecipes : recipesQuery.data.items).map((recipe) => (
                <ApiRecipeCard key={recipe.id} recipe={recipe} />
              ))}
            </div>
          )}
        </section>
      </main>
      <Footer />
    </div>
  );
}
