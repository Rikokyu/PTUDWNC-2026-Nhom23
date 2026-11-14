import React from "react";
import { cn } from "@/lib/utils";

export interface SkeletonProps {
  className?: string;
}

export const Skeleton: React.FC<SkeletonProps> = ({ className }) => {
  return (
    <div
      className={cn("animate-pulse rounded-md bg-stone-200/80", className)}
    />
  );
};

export const RecipeCardSkeleton: React.FC = () => {
  return (
    <div className="overflow-hidden rounded-2xl border border-stone-200/80 bg-surface p-0 shadow-subtle">
      <Skeleton className="w-full h-48 rounded-none" />
      <div className="p-4 space-y-3">
        <Skeleton className="h-4 w-1/4 rounded-full" />
        <Skeleton className="h-5 w-5/6" />
        <Skeleton className="h-4 w-3/4" />
        <div className="pt-2 flex items-center justify-between border-t border-slate-100">
          <Skeleton className="h-4 w-1/3" />
          <Skeleton className="h-4 w-1/4" />
        </div>
      </div>
    </div>
  );
};
