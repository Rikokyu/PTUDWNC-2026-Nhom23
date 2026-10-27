import Link from "next/link";
import { Clock, Flame } from "lucide-react";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import type { RecipeApiSummaryDto } from "@/types/api";

const difficultyLabels: Record<number, string> = {
  1: "Dễ",
  2: "Trung bình",
  3: "Khó",
  4: "Chuyên gia",
};

export function ApiRecipeCard({ recipe }: { recipe: RecipeApiSummaryDto }) {
  return (
    <article className="overflow-hidden rounded-xl border border-slate-200 bg-white transition hover:border-slate-300">
      <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`} className="block">
        <img
          src={resolveApiAssetUrl(recipe.thumbnailUrl) || "/images/image 1.png"}
          alt={recipe.title}
          className="aspect-[4/3] w-full bg-slate-100 object-cover"
          loading="lazy"
        />
        <div className="space-y-3 p-5">
          <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-content-tertiary">
            <span>{recipe.categoryName}</span>
            <span className="flex items-center gap-1">
              <Flame className="h-3.5 w-3.5" />
              {difficultyLabels[recipe.difficulty] ?? "Chưa phân loại"}
            </span>
          </div>
          <h2 className="line-clamp-2 text-base font-bold text-content-primary">
            {recipe.title}
          </h2>
          <div className="flex items-center gap-1 text-xs text-content-secondary">
            <Clock className="h-3.5 w-3.5" />
            {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút
            <span className="ml-auto">Khẩu phần: {recipe.servings}</span>
          </div>
        </div>
      </Link>
    </article>
  );
}