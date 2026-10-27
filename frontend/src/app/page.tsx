"use client";
import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { RecipeGrid } from "@/components/recipes/recipe-grid";
import { ErrorMessage } from "@/components/shared/error-message";
import { Pagination } from "@/components/shared/pagination";
import { useCategories } from "@/hooks/useCategories";
import { recipeApi } from "@/lib/api/custom-recipes";

export default function HomePage() {
  const [categoryId, setCategory] = useState("");
  const [page, setPage] = useState(1);
  const categories = useCategories();
  const recipes = useQuery({ queryKey: ["homepage-latest", categoryId, page],
    queryFn: () => recipeApi.getList({ page, pageSize: 9, categoryId: categoryId || undefined, sort: "-createdAt" }) });
  return <MainLayout>
    <section className="hero-section"><div><span className="pill">Culinary Blog</span>
      <h1>Khám phá món ăn Việt ngon, dễ làm và đầy cảm hứng.</h1>
      <p>Công thức mới nhất từ cộng đồng, sắp xếp rõ ràng để bạn nấu thật dễ dàng.</p>
      <div className="hero-actions"><Link className="button button-primary" href="/recipes">Xem công thức</Link><Link className="button button-ghost" href="/recipes/search">Tìm kiếm nâng cao</Link></div>
    </div></section>
    <div className="grid items-start gap-6 md:grid-cols-[220px_1fr]">
      <aside className="rounded-xl border bg-white p-4" aria-label="Danh mục công thức">
        <h2>Danh mục</h2>
        {categories.isLoading && <p role="status">Đang tải danh mục…</p>}
        {categories.isError && <ErrorMessage message="Không thể tải danh mục." onRetry={() => void categories.refetch()} />}
        <nav className="flex flex-wrap gap-2 md:flex-col">
          <button className="rounded-lg border p-2 text-left" aria-pressed={!categoryId} onClick={() => { setCategory(""); setPage(1); }}>Tất cả</button>
          {categories.data?.map(category => <button key={category.id} className="rounded-lg border p-2 text-left" aria-pressed={categoryId === category.id} onClick={() => { setCategory(category.id); setPage(1); }}>{category.name}</button>)}
        </nav>
        {!categories.isLoading && !categories.isError && categories.data?.length === 0 && <p>Chưa có danh mục.</p>}
      </aside>
      <section className="section-block min-w-0"><div className="section-heading"><h2>Công thức mới nhất</h2><Link href="/recipes">Xem tất cả</Link></div>
        {recipes.isError ? <ErrorMessage message="Không tải được công thức." onRetry={() => void recipes.refetch()} /> :
          <RecipeGrid items={(recipes.data?.items ?? []).map(recipe => ({ ...recipe, description: recipe.categoryName }))} isLoading={recipes.isLoading} emptyTitle="Chưa có công thức" emptyDescription="Hãy thử danh mục khác hoặc quay lại sau." />}
        <Pagination currentPage={page} totalPages={recipes.data?.totalPages ?? 0} onPageChange={setPage} />
      </section>
    </div>
  </MainLayout>;
}
