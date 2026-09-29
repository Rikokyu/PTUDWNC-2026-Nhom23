"use client";

import React from "react";
import Link from "next/link";
import { MockCategory } from "@/lib/mockData";
import { ChefHat, ArrowRight } from "lucide-react";

export interface CategoryGridProps {
  categories: MockCategory[];
}

export const CategoryGrid: React.FC<CategoryGridProps> = ({ categories }) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      {categories.map((category) => (
        <Link
          key={category.id}
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
            {category.description}
          </p>

          <div className="text-xs font-semibold text-brand inline-flex items-center gap-1 group-hover:translate-x-1 transition-transform">
            Xem món ăn <ArrowRight className="w-3.5 h-3.5" />
          </div>
        </Link>
      ))}
    </div>
  );
};
