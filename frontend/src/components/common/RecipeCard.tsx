import React, { useState } from "react";
import Link from "next/link";
import { Clock, Flame, Bookmark } from "lucide-react";
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
  categorySlug = "mon-chinh",
  prepTimeMinutes,
  cookTimeMinutes,
  difficulty,
  rating = 4.8,
  totalReviews = 24,
  authorName,
  authorAvatar,
  author,
  isSaved = false,
  isFeatured = false,
  className,
}) => {
  const [saved, setSaved] = useState(isSaved);

  const displayImage = primaryImageUrl || imageUrl || "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=600&q=80";
  const displaySummary = summary || description || "";
  const nameOfAuthor = authorName || author?.name || "Đầu bếp Culinary";
  const avatarOfAuthor = authorAvatar || author?.avatarUrl;

  const totalTime = prepTimeMinutes + cookTimeMinutes;

  return (
    <article
      className={cn(
        "group bg-white rounded-xl border border-slate-200/90 overflow-hidden shadow-subtle hover:border-slate-300 hover:shadow-card transition-all duration-200 flex flex-col justify-between",
        className
      )}
    >
      {/* Recipe Image Container */}
      <div className="relative aspect-[4/3] bg-slate-100 overflow-hidden">
        <img
          src={displayImage}
          alt={title}
          className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-102"
          loading="lazy"
        />

        {/* Category Tag Overlay */}
        <div className="absolute top-3 left-3 flex items-center gap-2 z-10">
          <Link href={`/categories/${categorySlug}`}>
            <Badge variant="brand" size="sm" className="bg-white/95 text-brand-hover backdrop-blur-xs font-semibold">
              {categoryName}
            </Badge>
          </Link>

          {/* Micro Pastel Purple Badge for Featured status (<2% total color budget) */}
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
            <RatingStars rating={rating} totalReviews={totalReviews} size="sm" />
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
