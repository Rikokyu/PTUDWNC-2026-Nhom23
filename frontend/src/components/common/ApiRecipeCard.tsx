import Link from "next/link";
import { ChefHat, Clock, Flame, Users } from "lucide-react";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import type { RecipeApiSummaryDto } from "@/types/api";

const difficultyLabels: Record<number, string> = {
  1: "Dễ",
  2: "Trung bình",
  3: "Khó",
  4: "Chuyên gia",
};

export function ApiRecipeCard({ recipe }: { recipe: RecipeApiSummaryDto }) {
  const totalMinutes = recipe.prepTimeMinutes + recipe.cookTimeMinutes;
  const thumbnailUrl = resolveApiAssetUrl(recipe.thumbnailUrl);

  return (
<<<<<<< HEAD
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
=======
    <article className="group relative overflow-hidden border border-zinc-200 bg-white transition duration-300 hover:-translate-y-1 hover:shadow-card-hover motion-reduce:transform-none">
      <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`} className="block focus-visible:outline-offset-4">
        <div className="relative overflow-hidden">
          {thumbnailUrl ? (
            <img
              src={thumbnailUrl}
              alt={recipe.title}
              className="aspect-[16/10] w-full bg-zinc-100 object-cover transition-transform duration-700 group-hover:scale-[1.04] motion-reduce:transform-none"
              loading="lazy"
            />
          ) : (
            <div className="flex aspect-[16/10] flex-col items-center justify-center gap-3 bg-surface-subtle text-content-tertiary">
              <ChefHat className="h-9 w-9" aria-hidden="true" />
              <span className="text-xs font-semibold uppercase tracking-widest">Chưa có ảnh món ăn</span>
            </div>
          )}
          <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black/45 via-transparent to-transparent opacity-80" />
          <div className="absolute bottom-3 left-3 right-3 flex items-center justify-between gap-2">
            <span className="bg-white/95 px-2.5 py-1 text-[11px] font-bold uppercase tracking-[0.12em] text-content-primary backdrop-blur-sm">
              {recipe.categoryName}
            </span>
            <span className="inline-flex items-center gap-1 bg-black/60 px-2.5 py-1 text-[11px] font-semibold text-white backdrop-blur-sm">
              <Flame className="h-3.5 w-3.5 text-white" />
              <span className="text-white">{difficultyLabels[recipe.difficulty] ?? "Chưa phân loại"}</span>
>>>>>>> 315339d (Update UI)
            </span>
          </div>
        </div>
        <div className="space-y-3 px-4 py-4 sm:px-5 sm:py-5">
          <h2 className="font-display line-clamp-2 min-h-12 text-lg font-bold leading-6 text-content-primary transition-colors group-hover:text-brand">
            {recipe.title}
          </h2>
          <div className="flex items-center gap-4 text-xs font-medium text-content-secondary">
            <span className="inline-flex items-center gap-1.5">
              <Clock className="h-3.5 w-3.5 text-content-tertiary" />
              {totalMinutes} phút
            </span>
            <span className="inline-flex items-center gap-1.5">
              <Users className="h-3.5 w-3.5 text-content-tertiary" />
              {recipe.servings} khẩu phần
            </span>
          </div>
        </div>
      </Link>
    </article>
  );
}