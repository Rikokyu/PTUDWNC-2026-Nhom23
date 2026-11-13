"use client";

import { FormEvent, Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { Button } from "@/components/ui/Button";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { recipeApi } from "@/lib/api/recipes";
import { useQuery } from "@tanstack/react-query";
import { useCategories } from "@/hooks/useCategories";
import type { RecipeFilters } from "@/types/api";

function SearchContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const initialQuery = searchParams.get("q") ?? "";
  const [draft, setDraft] = useState(initialQuery);
  const [term, setTerm] = useState(initialQuery.trim());
  const [page, setPage] = useState(1);
  const [validationError, setValidationError] = useState("");
  const categories = useCategories();
  const positive = (key: string, minimum: number) => {
    const raw = searchParams.get(key);
    const value = raw === null || raw === "" ? NaN : Number(raw);
    return Number.isSafeInteger(value) && value >= minimum ? value : undefined;
  };
  const filters = {
    categoryId: searchParams.get("categoryId") || undefined,
    difficulty: (["Easy", "Medium", "Hard", "Expert"].includes(searchParams.get("difficulty") ?? "")
      ? searchParams.get("difficulty") : undefined) as RecipeFilters["difficulty"],
    maxCookTime: positive("maxCookTime", 0),
    minServings: positive("minServings", 1),
  };
  function changeFilter(key: string, value: string) {
    const params = new URLSearchParams(searchParams.toString());
    if (value) params.set(key, value);
    else params.delete(key);
    setPage(1);
    router.replace(`/recipes/search?${params}`);
  }

  useEffect(() => {
    setDraft(initialQuery);
    setTerm(initialQuery.trim());
  }, [initialQuery]);

  const resultsQuery = useQuery({
    queryKey: ["recipe-search", term, page, filters],
    queryFn: () => recipeApi.search(term, page, 10, filters),
    enabled: term.length >= 2,
  });

  function submitSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const normalizedTerm = draft.trim();
    if (normalizedTerm.length < 2) {
      setValidationError("Nhập ít nhất 2 ký tự để tìm kiếm.");
      return;
    }
    setValidationError("");
    setPage(1);
    setTerm(normalizedTerm);
    changeFilter("q", normalizedTerm);
  }

  return (
    <div className="space-y-9 pb-12">
      <Breadcrumb items={[
        { label: "Công thức", href: "/recipes" },
        { label: "Tìm kiếm" },
      ]} />
      <header className="border-b border-zinc-300 pb-8 sm:pb-10">
        <p className="mb-4 text-xs font-bold uppercase tracking-[0.25em] text-brand">Culinary Blog / Tìm kiếm</p>
        <h1 className="font-display text-4xl font-bold tracking-tight text-content-primary sm:text-5xl">
          Tìm kiếm công thức
        </h1>
        <p className="mt-4 max-w-2xl text-base leading-7 text-content-secondary">
          Tìm theo tên món ăn, mô tả, hướng dẫn hoặc tên nguyên liệu.
        </p>
      </header>

      <form onSubmit={submitSearch} className="flex max-w-2xl flex-col gap-3 sm:flex-row sm:items-end">
        <Input
          label="Từ khóa"
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
          placeholder="Ví dụ: phở bò, nấm, gà..."
          maxLength={100}
        />
        <Button type="submit" className="shrink-0">Tìm kiếm</Button>
      </form>
      <section aria-label="Lọc kết quả tìm kiếm" className="grid gap-5 border border-zinc-200 bg-white p-5 shadow-subtle sm:grid-cols-2 sm:p-6 lg:grid-cols-4">
        <label className="text-sm font-semibold">Danh mục
          <select className="mt-2 w-full rounded-md border border-zinc-300 bg-surface-cream p-2.5" value={filters.categoryId ?? ""}
            onChange={(event) => changeFilter("categoryId", event.target.value)}>
            <option value="">Tất cả danh mục</option>
            {(categories.data ?? []).map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
        </label>
        <label className="text-sm font-semibold">Độ khó
          <select className="mt-2 w-full rounded-md border border-zinc-300 bg-surface-cream p-2.5" value={filters.difficulty ?? ""}
            onChange={(event) => changeFilter("difficulty", event.target.value)}>
            <option value="">Mọi mức độ</option>
            <option value="Easy">Dễ</option><option value="Medium">Trung bình</option>
            <option value="Hard">Khó</option><option value="Expert">Chuyên gia</option>
          </select>
        </label>
        <Input label="Thời gian nấu tối đa (phút)" type="number" min={0} step={1}
          value={filters.maxCookTime ?? ""} onChange={(event) => changeFilter("maxCookTime", event.target.value)} />
        <Input label="Khẩu phần tối thiểu" type="number" min={1} step={1}
          value={filters.minServings ?? ""} onChange={(event) => changeFilter("minServings", event.target.value)} />
      </section>
      {validationError && <p role="alert" className="text-sm text-rose-700">{validationError}</p>}

      {!term && (
        <p className="rounded-xl border border-slate-200 bg-white p-6 text-sm text-content-secondary">
          Nhập từ khóa để tìm kiếm trong công thức đã xuất bản.
        </p>
      )}
      {term.length >= 2 && resultsQuery.isLoading && (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <RecipeCardSkeleton key={index} />)}
        </div>
      )}
      {resultsQuery.isError && (
        <ErrorState
          title="Tìm kiếm không thành công"
          message="Không thể truy vấn API tìm kiếm. Hãy thử lại."
          onRetry={() => void resultsQuery.refetch()}
        />
      )}
      {resultsQuery.data && resultsQuery.data.items.length > 0 && (
        <>
          <p className="border-b border-zinc-300 pb-4 font-display text-lg font-semibold text-content-primary">
            {resultsQuery.data.totalCount} kết quả cho “{term}”
          </p>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {resultsQuery.data.items.map((recipe) => (
              <ApiRecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>
          {resultsQuery.data.totalPages > 1 && (
            <nav className="flex items-center justify-center gap-4" aria-label="Phân trang kết quả">
              <Button
                variant="outline"
                size="sm"
                disabled={!resultsQuery.data.hasPreviousPage}
                onClick={() => setPage((current) => current - 1)}
              >
                Trước
              </Button>
              <span className="text-sm text-content-secondary">
                Trang {resultsQuery.data.page} / {resultsQuery.data.totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={!resultsQuery.data.hasNextPage}
                onClick={() => setPage((current) => current + 1)}
              >
                Tiếp
              </Button>
            </nav>
          )}
        </>
      )}
      {resultsQuery.data?.items.length === 0 && (
        <EmptyState
          title="Không tìm thấy công thức phù hợp"
          description="Thử một từ khóa khác hoặc kiểm tra chính tả."
          actionLabel="Xóa từ khóa"
          onAction={() => {
            setDraft("");
            setTerm("");
            setPage(1);
            router.replace("/recipes/search");
          }}
        />
      )}
    </div>
  );
}

export default function RecipeSearchPage() {
  return (
    <Suspense fallback={<p className="py-12 text-center text-sm text-slate-500">Đang tải trang tìm kiếm...</p>}>
      <SearchContent />
    </Suspense>
  );
}
