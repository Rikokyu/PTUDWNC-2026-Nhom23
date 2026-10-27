import React from "react";
import Link from "next/link";
import { cn } from "@/lib/utils";

export interface CategoryPillProps {
  name: string;
  slug: string;
  count?: number;
  isActive?: boolean;
  icon?: React.ReactNode;
  onClick?: () => void;
}

export const CategoryPill: React.FC<CategoryPillProps> = ({
  name,
  slug,
  count,
  isActive = false,
  icon,
  onClick,
}) => {
  const content = (
    <div
      onClick={onClick}
      className={cn(
        "inline-flex items-center gap-2 px-4 py-2 rounded-full text-xs font-semibold transition-all duration-150 cursor-pointer select-none border border-slate-200/80",
        isActive
          ? "bg-brand text-white border-brand shadow-xs"
          : "bg-white text-content-secondary hover:text-content-primary hover:bg-slate-50 hover:border-slate-300"
      )}
    >
      {icon && <span className="shrink-0">{icon}</span>}
      <span>{name}</span>
      {typeof count === "number" && (
        <span
          className={cn(
            "text-[10px] px-1.5 py-0.2 rounded-full font-bold",
            isActive ? "bg-white/20 text-white" : "bg-slate-100 text-slate-600"
          )}
        >
          {count}
        </span>
      )}
    </div>
  );

  if (onClick) return content;

  return <Link href={`/categories/${slug}`}>{content}</Link>;
};
