"use client";

import React, { useState } from "react";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { SearchBar } from "@/components/common/SearchBar";
import { RecipeCard } from "@/components/common/RecipeCard";
import { CategoryPill } from "@/components/common/CategoryPill";
import { Select } from "@/components/ui/Select";
import { Pagination } from "@/components/ui/Pagination";
import { EmptyState } from "@/components/ui/EmptyState";
import { MOCK_RECIPES, MOCK_CATEGORIES } from "@/lib/mockData";
import { SlidersHorizontal } from "lucide-react";

export default function RecipesPage() {
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("all");
  const [difficultyFilter, setDifficultyFilter] = useState("all");
  const [sortBy, setSortBy] = useState("newest");
  const [currentPage, setCurrentPage] = useState(1);

  // Filtering logic
  const filteredRecipes = MOCK_RECIPES.filter((recipe) => {
    const matchesSearch =
      !searchQuery ||
      recipe.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      recipe.summary.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesCategory =
      selectedCategory === "all" || recipe.categorySlug === selectedCategory;

    const matchesDifficulty =
      difficultyFilter === "all" || recipe.difficulty === difficultyFilter;

    return matchesSearch && matchesCategory && matchesDifficulty;
  });

  // Sorting
  const sortedRecipes = [...filteredRecipes].sort((a, b) => {
    if (sortBy === "popular") return b.totalReviews - a.totalReviews;
    if (sortBy === "rating") return b.rating - a.rating;
    if (sortBy === "quick") return (a.prepTimeMinutes + a.cookTimeMinutes) - (b.prepTimeMinutes + b.cookTimeMinutes);
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  });

  return (
    <div className="space-y-8">
      {/* Breadcrumb */}
      <Breadcrumb items={[{ label: "Tất cả công thức" }]} />

      {/* Page Header */}
      <div className="space-y-2">
        <h1 className="text-3xl font-extrabold text-content-primary tracking-tight">
          Khám Phá Công Thức Nấu Ăn
        </h1>
        <p className="text-sm text-content-secondary max-w-2xl">
          Bộ sưu tập các món ăn ngon mỗi ngày được hướng dẫn bởi các đầu bếp và người yêu ẩm thực trên cả nước.
        </p>
      </div>

      {/* Search & Filter Bar Section */}
      <div className="space-y-4">
        <SearchBar
          initialValue={searchQuery}
          onSearch={(query) => setSearchQuery(query)}
        />

        {/* Filters Controls Row */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pt-2">
          {/* Category Filter Pills */}
          <div className="flex items-center gap-2 overflow-x-auto no-scrollbar pb-1">
            <CategoryPill
              name="Tất cả"
              slug="all"
              isActive={selectedCategory === "all"}
              onClick={() => setSelectedCategory("all")}
            />
            {MOCK_CATEGORIES.map((cat) => (
              <CategoryPill
                key={cat.id}
                name={cat.name}
                slug={cat.slug}
                isActive={selectedCategory === cat.slug}
                onClick={() => setSelectedCategory(cat.slug)}
              />
            ))}
          </div>

          {/* Select dropdowns for difficulty & sorting */}
          <div className="flex items-center gap-3 shrink-0">
            <div className="w-36">
              <Select
                value={difficultyFilter}
                onChange={(e) => setDifficultyFilter(e.target.value)}
                options={[
                  { label: "Mọi độ khó", value: "all" },
                  { label: "Dễ làm", value: "Easy" },
                  { label: "Trung bình", value: "Medium" },
                  { label: "Cầu kỳ", value: "Hard" },
                ]}
              />
            </div>
            <div className="w-40">
              <Select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                options={[
                  { label: "Mới nhất", value: "newest" },
                  { label: "Đánh giá cao", value: "rating" },
                  { label: "Xem nhiều nhất", value: "popular" },
                  { label: "Nấu nhanh nhất", value: "quick" },
                ]}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Results Count Summary */}
      <div className="flex items-center justify-between border-t border-slate-100 pt-4 text-xs text-content-tertiary">
        <span>
          Hiển thị <strong className="text-content-primary">{sortedRecipes.length}</strong> công thức phù hợp
        </span>
      </div>

      {/* Recipe Grid or Empty State */}
      {sortedRecipes.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {sortedRecipes.map((recipe) => (
            <RecipeCard key={recipe.id} {...recipe} />
          ))}
        </div>
      ) : (
        <EmptyState
          title="Không tìm thấy công thức"
          description="Rất tiếc, không có công thức nào phù hợp với bộ lọc của bạn. Hãy thử thay đổi từ khóa hoặc xóa bớt tiêu chí lọc."
          actionLabel="Xóa bộ lọc"
          onAction={() => {
            setSearchQuery("");
            setSelectedCategory("all");
            setDifficultyFilter("all");
          }}
        />
      )}

      {/* Pagination */}
      <Pagination
        currentPage={currentPage}
        totalPages={Math.ceil(sortedRecipes.length / 6) || 1}
        onPageChange={(page) => setCurrentPage(page)}
      />
    </div>
  );
}
