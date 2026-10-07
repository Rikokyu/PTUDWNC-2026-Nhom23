"use client";

import React, { useState, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { SearchBar } from "@/components/common/SearchBar";
import { RecipeCard } from "@/components/common/RecipeCard";
import { CategoryPill } from "@/components/common/CategoryPill";
import { EmptyState } from "@/components/ui/EmptyState";
import { MOCK_RECIPES, MOCK_CATEGORIES } from "@/lib/mockData";

function SearchContent() {
  const searchParams = useSearchParams();
  const initialQuery = searchParams.get("q") || "";

  const [query, setQuery] = useState(initialQuery);
  const [selectedCategory, setSelectedCategory] = useState("all");

  const searchResults = MOCK_RECIPES.filter((r) => {
    const q = query.toLowerCase();
    const matchesText =
      !q ||
      r.title.toLowerCase().includes(q) ||
      r.summary.toLowerCase().includes(q) ||
      r.ingredients.some((ing) => ing.name.toLowerCase().includes(q));

    const matchesCat =
      selectedCategory === "all" || r.categorySlug === selectedCategory;

    return matchesText && matchesCat;
  });

  return (
    <div className="space-y-8">
      <Breadcrumb
        items={[
          { label: "Công thức", href: "/recipes" },
          { label: "Tìm kiếm công thức" },
        ]}
      />

      <div className="space-y-2">
        <h1 className="text-3xl font-extrabold text-content-primary tracking-tight">
          Tìm Kiếm Công Thức Nấu Ăn
        </h1>
        <p className="text-sm text-content-secondary max-w-xl">
          Nhập tên món ăn, loại nguyên liệu hoặc phương pháp chế biến bạn muốn tìm.
        </p>
      </div>

      <div className="max-w-2xl space-y-4">
        <SearchBar
          initialValue={query}
          onSearch={(q) => setQuery(q)}
          placeholder="Ví dụ: Phở bò, Sườn nướng, Bơ, Nấm..."
        />

        <div className="flex items-center gap-2 overflow-x-auto no-scrollbar pt-2">
          <CategoryPill
            name="Tất cả"
            slug="all"
            isActive={selectedCategory === "all"}
            onClick={() => setSelectedCategory("all")}
          />
          {MOCK_CATEGORIES.map((c) => (
            <CategoryPill
              key={c.id}
              name={c.name}
              slug={c.slug}
              isActive={selectedCategory === c.slug}
              onClick={() => setSelectedCategory(c.slug)}
            />
          ))}
        </div>
      </div>

      <div className="border-t border-slate-100 pt-6">
        <div className="flex items-center justify-between text-xs text-content-tertiary mb-6">
          <span>
            {query ? (
              <>
                Kết quả tìm kiếm cho: <strong className="text-content-primary">"{query}"</strong> ({searchResults.length} món)
              </>
            ) : (
              <>Tất cả công thức tìm thấy ({searchResults.length} món)</>
            )}
          </span>
        </div>

        {searchResults.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {searchResults.map((recipe) => (
              <RecipeCard key={recipe.id} {...recipe} />
            ))}
          </div>
        ) : (
          <EmptyState
            title="Không tìm thấy kết quả phù hợp"
            description="Hãy thử lại với từ khóa khác như 'bò', 'gà', 'chay', 'matcha'..."
            actionLabel="Xóa từ khóa tìm kiếm"
            onAction={() => {
              setQuery("");
              setSelectedCategory("all");
            }}
          />
        )}
      </div>
    </div>
  );
}

export default function RecipeSearchPage() {
  return (
    <Suspense fallback={<div className="py-12 text-center text-sm text-slate-500">Đang tải trang tìm kiếm...</div>}>
      <SearchContent />
    </Suspense>
  );
}
