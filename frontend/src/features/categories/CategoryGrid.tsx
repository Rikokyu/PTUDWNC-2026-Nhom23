"use client";

import React from "react";
import Link from "next/link";
import { ChefHat, ArrowRight, Pencil, Trash2 } from "lucide-react";
import type { CategoryApiDto } from "@/types/api";

export interface CategoryGridProps {
  categories: CategoryApiDto[];
  onEdit?: (category: CategoryApiDto) => void;
  onDelete?: (category: CategoryApiDto) => void;
  deletingCategoryId?: string | null;
}

export const CategoryGrid: React.FC<CategoryGridProps> = ({
  categories,
  onEdit,
  onDelete,
  deletingCategoryId,
}) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      {categories.map((category) => (
        <div key={category.id}>
        <Link
          href={`/categories/${category.slug}`}
          className="group relative rounded-xl overflow-hidden border border-slate-200 bg-white p-6 shadow-subtle hover:shadow-card hover:border-slate-300 transition-all duration-200 flex flex-col justify-between"
        >
          <div className="flex items-start justify-between gap-4 mb-4">
            <div>
              <h3 className="text-lg font-bold text-content-primary group-hover:text-brand transition-colors">
                {category.name}
              </h3>
              <p className="text-xs text-content-tertiary mt-1">
                {category.recipeCount} công thức ngon
              </p>
            </div>
            <div className="w-10 h-10 rounded-lg bg-brand-light text-brand-hover flex items-center justify-center shrink-0 border border-brand-border/30">
              <ChefHat className="w-5 h-5" />
            </div>
          </div>

          <p className="text-xs text-content-secondary leading-relaxed mb-6">
            {category.description || "Khám phá các công thức thuộc danh mục này."}
          </p>

          <div className="text-xs font-semibold text-brand inline-flex items-center gap-1 group-hover:translate-x-1 transition-transform">
            Xem món ăn <ArrowRight className="w-3.5 h-3.5" />
          </div>
        </Link>
        {onEdit && (
          <button
            type="button"
            onClick={() => onEdit(category)}
            aria-label={`Sửa danh mục ${category.name}`}
            className="mt-2 inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-xs font-semibold text-content-secondary transition-colors hover:border-brand hover:text-brand focus:outline-none focus:ring-2 focus:ring-brand/30"
          >
            <Pencil className="h-3.5 w-3.5" />
            Sửa danh mục
          </button>
        )}
        {onDelete && (
          <button
            type="button"
            onClick={() => onDelete(category)}
            disabled={deletingCategoryId === category.id}
            aria-label={`Xóa danh mục ${category.name}`}
            className="ml-2 mt-2 inline-flex items-center gap-2 rounded-lg border border-rose-200 bg-white px-3 py-2 text-xs font-semibold text-rose-700 transition-colors hover:border-rose-400 hover:bg-rose-50 focus:outline-none focus:ring-2 focus:ring-rose-300 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <Trash2 className="h-3.5 w-3.5" />
            {deletingCategoryId === category.id ? "Đang xóa…" : "Xóa danh mục"}
          </button>
        )}
        </div>
      ))}
    </div>
  );
};
