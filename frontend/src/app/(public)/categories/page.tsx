"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { SearchX } from "lucide-react";
import { AutocompleteSearch } from "@/components/common/AutocompleteSearch";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { useCategories } from "@/hooks/useCategories";
import { CategoryGrid } from "@/features/categories/CategoryGrid";
import type { CategoryApiDto } from "@/types/api";

function normalizeSearchText(value: string) {
  return value.toLocaleLowerCase("vi").normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "").replace(/đ/g, "d");
}

function searchCategories(categories: CategoryApiDto[], term: string) {
  const normalizedTerm = normalizeSearchText(term.trim());
  if (!normalizedTerm) return categories;
  return categories
    .map((category) => {
      const name = normalizeSearchText(category.name);
      const description = normalizeSearchText(category.description ?? "");
      const rank = name === normalizedTerm ? 0
        : name.startsWith(normalizedTerm) ? 1
        : name.includes(normalizedTerm) ? 2
        : description.includes(normalizedTerm) ? 3
        : 4;
      return { category, rank };
    })
    .filter(({ rank }) => rank < 4)
    .sort((a, b) => a.rank - b.rank
      || b.category.recipeCount - a.category.recipeCount
      || a.category.name.localeCompare(b.category.name, "vi"))
    .map(({ category }) => category);
}

export default function CategoriesPage() {
  const router = useRouter();
  const categoriesQuery = useCategories();
  const [draft, setDraft] = useState("");
  const [term, setTerm] = useState("");
  const categories = categoriesQuery.data ?? [];
  const matchingCategories = searchCategories(categories, term);
  const suggestions = draft.trim()
    ? searchCategories(categories, draft).slice(0, 6).map((category) => ({
        id: category.id,
        label: category.name,
        description: `${category.recipeCount} công thức${category.description ? ` · ${category.description}` : ""}`,
      }))
    : [];

  return (
    <div className="space-y-6 pb-10">
      <Breadcrumb items={[{ label: "Tất cả danh mục" }]} />
      <header className="border-b border-stone-300 pb-5 sm:pb-6">
        <p className="editorial-kicker mb-3">Khám phá theo chủ đề</p>
        <h1 className="font-display text-4xl font-bold tracking-tight text-content-primary sm:text-5xl">
          Danh mục món ăn
        </h1>
        <p className="mt-3 max-w-2xl text-base leading-7 text-content-secondary">
          Từ bữa cơm nhà đến những món mới lạ, chọn một chủ đề để bắt đầu vào bếp.
        </p>
      </header>

      <section aria-label="Tìm kiếm danh mục" className="max-w-3xl">
        <AutocompleteSearch
          label="Tìm danh mục món ăn"
          placeholder="Tìm theo tên hoặc mô tả danh mục..."
          value={draft}
          onChange={setDraft}
          onSearch={setTerm}
          onSelect={(suggestion) => {
            const category = categories.find((item) => item.id === suggestion.id);
            if (category) router.push(`/categories/${encodeURIComponent(category.slug)}`);
          }}
          suggestions={suggestions}
          isLoading={categoriesQuery.isLoading}
        />
      </section>

      {categoriesQuery.isLoading && (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <RecipeCardSkeleton key={index} />)}
        </div>
      )}
      {categoriesQuery.isError && (
        <ErrorState
          title="Không tải được danh mục"
          message="Hãy kiểm tra API và thử lại."
          onRetry={() => void categoriesQuery.refetch()}
        />
      )}
      {categoriesQuery.data?.length === 0 && (
        <p className="rounded-xl border border-slate-200 bg-white p-8 text-center text-sm text-content-secondary">
          Hiện chưa có danh mục nào.
        </p>
      )}
      {categoriesQuery.data && categoriesQuery.data.length > 0 && matchingCategories.length === 0 && (
        <EmptyState
          icon={<SearchX className="h-7 w-7" aria-hidden="true" />}
          title="Không tìm thấy danh mục"
          description={`Không có danh mục phù hợp với “${term}”. Hãy thử từ khóa khác.`}
          actionLabel="Xem tất cả danh mục"
          onAction={() => {
            setDraft("");
            setTerm("");
          }}
        />
      )}
      {matchingCategories.length > 0 && (
        <CategoryGrid categories={matchingCategories} />
      )}
    </div>
  );
}
