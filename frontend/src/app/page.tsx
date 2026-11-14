"use client";

import { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { ArrowRight, Clock } from "lucide-react";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Footer } from "@/components/layout/Footer";
import { Navbar } from "@/components/layout/Navbar";
import { AutocompleteSearch } from "@/components/common/AutocompleteSearch";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useCategories } from "@/hooks/useCategories";
import { useRecipes } from "@/hooks/useRecipes";
import { useRecipeSuggestions } from "@/hooks/useRecipeSuggestions";

const QUICK_SEARCHES = ["Phở", "Bún chả", "Bánh mì", "Salad", "Cơm tấm"];

export default function HomePage() {
  const router = useRouter();
  const [searchDraft, setSearchDraft] = useState("");
  const recipeSuggestions = useRecipeSuggestions(searchDraft);
  const categoriesQuery = useCategories();
  const recipesQuery = useRecipes({ page: 1, pageSize: 8, sort: "-createdAt" });
  const featuredRecipe = recipesQuery.data?.items[0];
  const gridRecipes = recipesQuery.data?.items.slice(1) ?? [];

  return (
    <div className="flex min-h-screen flex-col bg-surface-muted">
      <Navbar />
      <main id="main-content" className="flex-1">
        <section className="relative z-10 isolate min-h-[min(82vh,760px)] bg-content-primary text-white">
          <div className="absolute inset-0 overflow-hidden">
            <Image
              src="/images/hero-pho-editorial.webp"
              alt="Bát phở bò với rau thơm trong gian bếp Việt"
              fill
              priority
              sizes="100vw"
              quality={85}
              className="object-cover motion-safe:animate-hero-zoom"
            />
            <div className="absolute inset-0 bg-[#171e18]/55 sm:bg-gradient-to-r sm:from-[#171e18]/85 sm:via-[#171e18]/45 sm:to-[#171e18]/10" />
          </div>

          <div className="relative mx-auto flex min-h-[min(82vh,760px)] w-full max-w-[1280px] flex-col justify-center px-4 pb-14 pt-28 sm:px-6 lg:px-8">
            <div className="max-w-2xl space-y-6 motion-safe:animate-fade-up">
              <p className="flex items-center gap-3 text-xs font-bold uppercase tracking-[0.25em] text-[#f0d1ae] before:h-px before:w-8 before:bg-[#f0d1ae]">
                Câu chuyện từ căn bếp Việt
              </p>
              <h1 className="font-display max-w-xl text-5xl font-bold leading-[1.08] tracking-tight text-white sm:text-6xl lg:text-[4.75rem]">
                Hôm nay nấu <span className="block font-serif font-normal italic text-[#f0d1ae]">món gì?</span>
              </h1>
              <p className="max-w-lg text-base leading-7 text-white/80 sm:text-lg">
                Khám phá công thức thân quen, tìm cảm hứng mới và cùng nhau nấu những bữa ăn đáng nhớ.
              </p>

              <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                <Link
                  href="/recipes"
                  className="inline-flex h-12 items-center justify-center rounded-full bg-brand px-7 text-sm font-bold text-white shadow-card transition-colors hover:bg-brand-hover"
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
                <AutocompleteSearch
                  label="Tìm công thức"
                  placeholder="Tìm món ăn hoặc nguyên liệu..."
                  value={searchDraft}
                  onChange={setSearchDraft}
                  suggestions={recipeSuggestions.suggestions}
                  isLoading={recipeSuggestions.isLoading}
                  onSearch={(query) => {
                    if (query) {
                      router.push(`/recipes?search=${encodeURIComponent(query)}`);
                    }
                  }}
                />
                <div className="mt-3 flex flex-wrap items-center gap-2">
                  <span className="text-xs font-medium text-white/60">Gợi ý:</span>
                  {QUICK_SEARCHES.map((term) => (
                    <button
                      key={term}
                      type="button"
                      onClick={() => router.push(`/recipes?search=${encodeURIComponent(term)}`)}
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

        <section className="border-b border-stone-200/80 bg-surface">
          <div className="mx-auto w-full max-w-[1280px] px-4 py-10 sm:px-6 sm:py-12 lg:px-8">
            <div className="mb-6 flex items-end justify-between gap-3">
              <div>
                <p className="editorial-kicker mb-2">Khám phá theo chủ đề</p>
                <h2 className="font-display text-2xl font-bold text-content-primary sm:text-3xl">Danh mục món ăn</h2>
                <p className="mt-1 text-sm text-content-secondary">Chọn nhóm món để bắt đầu.</p>
              </div>
              <Link href="/categories" className="inline-flex min-h-10 shrink-0 items-center rounded-full border border-red-500 bg-brand-light px-4 text-sm font-semibold text-brand transition-colors hover:border-red-600 hover:bg-brand hover:text-white">
                Tất cả
              </Link>
            </div>
            {categoriesQuery.isLoading ? (
              <div role="status" className="flex gap-3 overflow-hidden" aria-label="Đang tải danh mục">
                {Array.from({ length: 5 }, (_, index) => <span key={index} className="h-12 w-28 shrink-0 animate-pulse rounded-full bg-stone-200/70" />)}
              </div>
            ) : categoriesQuery.isError ? (
              <p className="text-sm text-rose-700">Không tải được danh mục.</p>
            ) : categoriesQuery.data?.length ? (
              <nav className="no-scrollbar -mx-1 flex gap-3 overflow-x-auto px-1 pb-2 pt-1" aria-label="Danh mục món ăn">
                {categoriesQuery.data.map((category) => (
                  <Link
                    key={category.id}
                    href={`/categories/${encodeURIComponent(category.slug)}`}
                    className="group inline-flex min-h-11 shrink-0 items-center gap-2 rounded-full border border-red-500 bg-surface-cream py-1.5 pl-4 pr-1.5 transition-all duration-200 hover:-translate-y-0.5 hover:border-red-600 hover:bg-brand-light hover:shadow-subtle motion-reduce:transform-none"
                  >
                    <span className="text-sm font-semibold text-content-primary transition-colors group-hover:text-brand-hover">
                      {category.name}
                    </span>
                    <span className="inline-flex h-7 min-w-7 items-center justify-center rounded-full bg-white px-1.5 text-xs font-bold tabular-nums text-brand transition-colors group-hover:bg-brand group-hover:text-white">{category.recipeCount}</span>
                  </Link>
                ))}
              </nav>
            ) : (
              <p className="text-sm text-content-secondary">Chưa có danh mục.</p>
            )}
          </div>
        </section>

        <section className="mx-auto w-full max-w-[1280px] px-4 py-14 sm:px-6 sm:py-16 lg:px-8">
          <header className="mb-8 flex items-end justify-between gap-4">
            <div>
              <p className="editorial-kicker mb-2">Từ căn bếp cộng đồng</p>
              <h2 className="font-display text-3xl font-bold text-content-primary sm:text-4xl">Công thức mới</h2>
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
            <p className="rounded-2xl border border-zinc-200 bg-white p-8 text-center text-sm text-content-secondary">
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
