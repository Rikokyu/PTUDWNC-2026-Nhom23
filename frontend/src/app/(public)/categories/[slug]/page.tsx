"use client";

import React, { use } from "react";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { RecipeCard } from "@/components/common/RecipeCard";
import { EmptyState } from "@/components/ui/EmptyState";
import { MOCK_CATEGORIES, MOCK_RECIPES } from "@/lib/mockData";

export default function CategoryDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = use(params);

  const category = MOCK_CATEGORIES.find((c) => c.slug === slug) || MOCK_CATEGORIES[0];
  const categoryRecipes = MOCK_RECIPES.filter((r) => r.categorySlug === category.slug);

  return (
    <div className="space-y-8">
      <Breadcrumb
        items={[
          { label: "Danh mục", href: "/categories" },
          { label: category.name },
        ]}
      />

      <div className="p-8 rounded-2xl bg-slate-50 border border-slate-200 space-y-3">
        <h1 className="text-3xl font-extrabold text-content-primary tracking-tight">
          {category.name}
        </h1>
        <p className="text-sm text-content-secondary max-w-2xl leading-relaxed">
          {category.description}
        </p>
        <p className="text-xs text-content-tertiary font-semibold pt-2">
          Tổng số {categoryRecipes.length} công thức sẵn có
        </p>
      </div>

      {categoryRecipes.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {categoryRecipes.map((recipe) => (
            <RecipeCard key={recipe.id} {...recipe} />
          ))}
        </div>
      ) : (
        <EmptyState
          title="Chưa có công thức trong danh mục này"
          description="Hãy quay lại sau hoặc là người đầu tiên đóng góp công thức mới cho danh mục này."
        />
      )}
    </div>
  );
}
