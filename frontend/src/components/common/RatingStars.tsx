import React from "react";
import { Star } from "lucide-react";
import { cn } from "@/lib/utils";

export interface RatingStarsProps {
  rating: number; // 0 to 5
  totalReviews?: number;
  size?: "sm" | "md";
  className?: string;
}

export const RatingStars: React.FC<RatingStarsProps> = ({
  rating,
  totalReviews,
  size = "sm",
  className,
}) => {
  const iconSize = size === "sm" ? "w-3.5 h-3.5" : "w-4 h-4";

  return (
    <div className={cn("inline-flex items-center gap-1.5", className)}>
      <div className="flex items-center gap-0.5 text-accent">
        {[1, 2, 3, 4, 5].map((star) => (
          <Star
            key={star}
            className={cn(
              iconSize,
              star <= Math.round(rating)
                ? "fill-accent text-accent"
                : "text-slate-300 fill-slate-100"
            )}
          />
        ))}
      </div>
      <span className="text-xs font-semibold text-content-primary">
        {rating.toFixed(1)}
      </span>
      {typeof totalReviews === "number" && (
        <span className="text-xs text-content-tertiary">({totalReviews})</span>
      )}
    </div>
  );
};
