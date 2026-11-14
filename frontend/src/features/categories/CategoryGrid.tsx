"use client";

import Link from "next/link";
import { ArrowUpRight, ChefHat, Pencil, Trash2 } from "lucide-react";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import type { CategoryApiDto } from "@/types/api";

export interface CategoryGridProps {
  categories: CategoryApiDto[];
  onEdit?: (category: CategoryApiDto) => void;
  onDelete?: (category: CategoryApiDto) => void;
  deletingCategoryId?: string | null;
}

export function CategoryGrid({ categories, onEdit, onDelete, deletingCategoryId }: CategoryGridProps) {
  return <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
    {categories.map((category) => {
      const imageUrl = resolveApiAssetUrl(category.imageUrl);
      return <article key={category.id} className="group overflow-hidden rounded-2xl border border-stone-200/80 bg-surface shadow-subtle transition duration-300 hover:-translate-y-1 hover:border-brand-border hover:shadow-card-hover motion-reduce:transform-none">
        <Link href={`/categories/${encodeURIComponent(category.slug)}`} className="block focus-visible:outline-offset-[-4px]">
          <div className="relative aspect-[16/10] overflow-hidden bg-surface-subtle">
            <ChefHat className="absolute left-1/2 top-1/2 h-12 w-12 -translate-x-1/2 -translate-y-1/2 text-stone-400" aria-hidden="true" />
            {imageUrl && <img src={imageUrl} alt={`Món ăn thuộc danh mục ${category.name}`} className="relative h-full w-full object-cover transition-transform duration-500 group-hover:scale-105 motion-reduce:transform-none" loading="lazy" decoding="async" onError={(event) => { event.currentTarget.style.display = "none"; }} />}
            <span className="absolute bottom-3 left-3 rounded-full bg-white/95 px-3 py-1.5 text-xs font-bold text-content-primary shadow-sm">{category.recipeCount} công thức</span>
          </div>
          <div className="space-y-3 p-5 sm:p-6">
            <div className="flex items-start justify-between gap-3">
              <h2 className="font-display text-xl font-bold text-content-primary transition-colors group-hover:text-brand">{category.name}</h2>
              <ArrowUpRight className="mt-1 h-5 w-5 shrink-0 text-brand" aria-hidden="true" />
            </div>
            <p className="line-clamp-2 min-h-10 text-sm leading-5 text-content-secondary">{category.description || "Khám phá những món ngon trong danh mục này."}</p>
          </div>
        </Link>
        {(onEdit || onDelete) && <div className="flex gap-2 border-t border-stone-100 px-5 py-3">
          {onEdit && <button type="button" onClick={() => onEdit(category)} className="inline-flex items-center gap-1.5 rounded-lg px-3 py-2 text-xs font-semibold text-content-secondary hover:bg-stone-100 hover:text-content-primary"><Pencil className="h-3.5 w-3.5" />Chỉnh sửa</button>}
          {onDelete && <button type="button" onClick={() => onDelete(category)} disabled={deletingCategoryId === category.id} className="inline-flex items-center gap-1.5 rounded-lg px-3 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-50 disabled:opacity-50"><Trash2 className="h-3.5 w-3.5" />{deletingCategoryId === category.id ? "Đang xóa…" : "Xóa"}</button>}
        </div>}
      </article>;
    })}
  </div>;
}
