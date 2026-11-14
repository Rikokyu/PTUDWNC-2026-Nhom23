import React, { useState } from "react";
import Link from "next/link";
import { Clock, Flame, Bookmark, ChefHat } from "lucide-react";
import { Badge } from "@/components/ui/Badge";
import { RatingStars } from "./RatingStars";
import { AuthorAvatar } from "./AuthorAvatar";
import { cn } from "@/lib/utils";

export interface RecipeCardProps {
  id: string;
  slug: string;
  title: string;
  summary?: string;
  description?: string;
  imageUrl?: string;
  primaryImageUrl?: string;
  categoryName: string;
  categorySlug?: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  difficulty: "Dễ" | "Trung bình" | "Khó" | "Easy" | "Medium" | "Hard";
  rating?: number;
  totalReviews?: number;
  authorName?: string;
  authorAvatar?: string;
  author?: {
    name: string;
    avatarUrl?: string;
  };
  isSaved?: boolean;
  isFeatured?: boolean;
  className?: string;
}

export const RecipeCard: React.FC<RecipeCardProps> = ({
  slug,
  title,
  summary,
  description,
  imageUrl,
  primaryImageUrl,
  categoryName,
  categorySlug,
  prepTimeMinutes,
  cookTimeMinutes,
  difficulty,
  rating,
  totalReviews,
  authorName,
  authorAvatar,
  author,
  isSaved = false,
  isFeatured = false,
  className,
}) => {
  const [saved, setSaved] = useState(isSaved);

  const displayImage = primaryImageUrl || imageUrl;
  const displaySummary = summary || description || "";
  const nameOfAuthor = authorName || author?.name || "Tác giả";
  const avatarOfAuthor = authorAvatar || author?.avatarUrl;

  const totalTime = prepTimeMinutes + cookTimeMinutes;

  return (
    <article
      className={cn(
        "group flex flex-col justify-between overflow-hidden rounded-2xl border border-stone-200/80 bg-surface shadow-subtle transition-all duration-200 hover:border-brand-border hover:shadow-card",
        className
      )}
    >
      {/* Recipe Image Container */}
      <div className="relative aspect-[4/3] overflow-hidden bg-surface-subtle">
        {displayImage ? <img
          src={displayImage}
          alt={title}
          className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-102"
          loading="lazy"
          decoding="async"
        /> : <div className="flex h-full w-full items-center justify-center text-content-tertiary"><ChefHat className="h-10 w-10" aria-label="Chưa có ảnh món ăn" /></div>}

        {/* Category Tag Overlay */}
        <div className="absolute top-3 left-3 flex items-center gap-2 z-10">
          {categorySlug ? <Link href={`/categories/${categorySlug}`}>
            <Badge variant="brand" size="sm" className="bg-white/95 text-brand-hover backdrop-blur-xs font-semibold">
              {categoryName}
            </Badge>
          </Link> : <Badge variant="brand" size="sm" className="bg-white/95 text-brand-hover backdrop-blur-xs font-semibold">{categoryName}</Badge>}

          {isFeatured && (
            <Badge variant="pastel" size="sm">
              Nổi bật
            </Badge>
          )}
        </div>

        {/* Bookmark Button */}
        <button
          onClick={(e) => {
            e.preventDefault();
            setSaved(!saved);
          }}
          className={cn(
            "absolute top-3 right-3 w-8 h-8 rounded-full bg-white/90 backdrop-blur-xs flex items-center justify-center transition-all duration-150 shadow-xs hover:bg-white focus:outline-none",
            saved ? "text-brand" : "text-slate-500 hover:text-slate-900"
          )}
          aria-label="Lưu công thức"
        >
          <Bookmark className={cn("w-4 h-4", saved && "fill-brand")} />
        </button>
      </div>

      {/* Recipe Details Content */}
      <div className="p-5 flex-1 flex flex-col justify-between space-y-4">
        <div className="space-y-2">
          {/* Metadata Row */}
          <div className="flex items-center justify-between text-[11px] text-content-tertiary">
            <div className="flex items-center gap-3">
              <span className="flex items-center gap-1 font-medium">
                <Clock className="w-3.5 h-3.5 text-slate-400" />
                {totalTime} phút
              </span>
              <span className="flex items-center gap-1 font-medium">
                <Flame className="w-3.5 h-3.5 text-slate-400" />
                {difficulty}
              </span>
            </div>
            {typeof rating === "number" && <RatingStars rating={rating} totalReviews={totalReviews} size="sm" />}
          </div>

          {/* Title */}
          <Link href={`/recipes/${slug}`} className="block group/title">
            <h3 className="text-base font-bold text-content-primary line-clamp-2 group-hover/title:text-brand transition-colors leading-snug">
              {title}
            </h3>
          </Link>

          {/* Summary */}
          {displaySummary && (
            <p className="text-xs text-content-secondary line-clamp-2 leading-relaxed">
              {displaySummary}
            </p>
          )}
        </div>

        {/* Author Footer */}
        <div className="pt-3 border-t border-slate-100 flex items-center justify-between">
          <AuthorAvatar name={nameOfAuthor} avatarUrl={avatarOfAuthor} size="sm" />
          <Link
            href={`/recipes/${slug}`}
            className="text-xs font-semibold text-brand hover:text-brand-hover inline-flex items-center gap-1"
          >
            Xem chi tiết →
          </Link>
        </div>
      </div>
    </article>
  );
};
