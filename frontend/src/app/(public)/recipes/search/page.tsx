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

function SearchContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const initialQuery = searchParams.get("q") ?? "";
  const [draft, setDraft] = useState(initialQuery);
  const [term, setTerm] = useState(initialQuery.trim());
  const [page, setPage] = useState(1);
  const [validationError, setValidationError] = useState("");

  useEffect(() => {
    setDraft(initialQuery);
    setTerm(initialQuery.trim());
  }, [initialQuery]);

  const resultsQuery = useQuery({
    queryKey: ["recipe-search", term, page],
    queryFn: () => recipeApi.search(term, page, 9),
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
    router.replace(`/recipes/search?q=${encodeURIComponent(normalizedTerm)}`);
  }

  return (
    <div className="space-y-8">
      <Breadcrumb items={[
        { label: "Công thức", href: "/recipes" },
        { label: "Tìm kiếm" },
      ]} />
      <header className="space-y-2">
        <h1 className="text-3xl font-extrabold tracking-tight text-content-primary">
          Tìm kiếm công thức
        </h1>
        <p className="max-w-2xl text-sm text-content-secondary">
          Tìm theo tên món ăn, mô tả, hướng dẫn hoặc tên nguyên liệu.
        </p>
      </header>

      <form onSubmit={submitSearch} className="flex max-w-2xl items-end gap-3">
        <Input
          label="Từ khóa"
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
          placeholder="Ví dụ: phở bò, nấm, gà..."
          maxLength={100}
        />
        <Button type="submit" className="shrink-0">Tìm kiếm</Button>
      </form>
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
          <p className="text-sm text-content-secondary">
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
