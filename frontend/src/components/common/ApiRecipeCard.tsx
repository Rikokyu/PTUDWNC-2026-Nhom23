"use client";

import Link from "next/link";
import { ChefHat, Clock, Flame, Users } from "lucide-react";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import { SavedRecipeBookmarkButton } from "@/components/recipes/SavedRecipeBookmarkButton";
import { AuthorAvatar } from "@/components/common/AuthorAvatar";
import { useAuthStore } from "@/store/useAuthStore";
import type { RecipeApiSummaryDto } from "@/types/api";

const difficultyLabels: Record<number, string> = {
  1: "Dễ",
  2: "Trung bình",
  3: "Khó",
  4: "Chuyên gia",
};

export function ApiRecipeCard({ recipe }: { recipe: RecipeApiSummaryDto }) {
  const currentUser = useAuthStore((state) => state.user);
  const totalMinutes = recipe.prepTimeMinutes + recipe.cookTimeMinutes;
  const thumbnailUrl = resolveApiAssetUrl(recipe.thumbnailUrl);
  const isOwnRecipe = Boolean(recipe.author && recipe.author.id === currentUser?.id);
  const authorAvatarUrl = resolveApiAssetUrl(recipe.author?.avatarUrl);

  return (
    <article className="group relative overflow-hidden rounded-2xl border border-stone-200/80 bg-surface shadow-subtle transition duration-300 hover:-translate-y-1 hover:border-brand-border hover:shadow-card-hover motion-reduce:transform-none">
      <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`} className="block focus-visible:outline-offset-4">
        <div className="relative overflow-hidden">
          {thumbnailUrl ? (
            <img
              src={thumbnailUrl}
              alt={recipe.title}
              className="aspect-[16/10] w-full bg-surface-subtle object-cover transition-transform duration-700 group-hover:scale-[1.04] motion-reduce:transform-none"
              loading="lazy"
              decoding="async"
            />
          ) : (
            <div className="flex aspect-[16/10] flex-col items-center justify-center gap-3 bg-surface-subtle text-content-tertiary">
              <ChefHat className="h-9 w-9" aria-hidden="true" />
              <span className="text-xs font-semibold uppercase tracking-widest">Chưa có ảnh món ăn</span>
            </div>
          )}
          <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black/45 via-transparent to-transparent opacity-80" />
          <div className="absolute bottom-3 left-3 right-3 flex items-center justify-between gap-2">
            <span className="rounded-full bg-white/95 px-2.5 py-1 text-[11px] font-bold uppercase tracking-[0.12em] text-content-primary backdrop-blur-sm">
              {recipe.categoryName}
            </span>
            <span className="inline-flex items-center gap-1 rounded-full bg-black/60 px-2.5 py-1 text-[11px] font-semibold text-white backdrop-blur-sm">
              <Flame className="h-3.5 w-3.5 text-white" />
              <span className="text-white">{difficultyLabels[recipe.difficulty] ?? "Chưa phân loại"}</span>
            </span>
          </div>
        </div>
        <div className="px-4 pt-4 pb-2 sm:px-5 sm:pt-5">
          <h2 className="font-display line-clamp-2 min-h-12 text-lg font-bold leading-6 text-content-primary transition-colors group-hover:text-brand">
            {recipe.title}
          </h2>
        </div>
      </Link>
      <div className="flex items-center justify-between gap-2 px-4 pb-4 sm:px-5 sm:pb-5">
        <div className="flex min-w-0 flex-wrap items-center gap-x-3 gap-y-1 text-[11px] font-medium text-content-secondary sm:text-xs">
          <span className="inline-flex items-center gap-1 whitespace-nowrap">
            <Clock className="h-3.5 w-3.5 text-content-tertiary" aria-hidden="true" />
            {totalMinutes} phút
          </span>
          <span className="inline-flex items-center gap-1 whitespace-nowrap">
            <Users className="h-3.5 w-3.5 text-content-tertiary" aria-hidden="true" />
            {recipe.servings} khẩu phần
          </span>
        </div>
        <Link
          href={recipe.author ? `/authors/${recipe.author.id}` : "/authors/culinary-blog"}
          className="min-w-0 max-w-[5rem] shrink-0 rounded-full transition-opacity hover:opacity-70 min-[420px]:max-w-[10rem] sm:max-w-[6rem] lg:max-w-[7rem]"
          aria-label={recipe.author ? isOwnRecipe ? "Xem trang của tôi" : `Xem trang tác giả ${recipe.author.displayName}` : "Xem công thức của Culinary Blog"}
        >
          <AuthorAvatar
            name={recipe.author?.displayName ?? "Culinary Blog"}
            displayName={isOwnRecipe ? "Tôi" : undefined}
            avatarUrl={authorAvatarUrl}
            size="sm"
            showName
          />
        </Link>
      </div>
      {recipe.status === 1 && (
        <div className="absolute right-3 top-3 z-10">
          <SavedRecipeBookmarkButton recipeId={recipe.id} returnTo={`/recipes/${encodeURIComponent(recipe.slug)}`} />
        </div>
      )}
    </article>
  );
}
