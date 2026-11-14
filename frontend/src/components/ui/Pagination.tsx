import React from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "./Button";

export interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  className?: string;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  onPageChange,
  className,
}) => {
  if (totalPages <= 1) return null;

  const nearbyPages = Array.from(new Set([1, currentPage - 1, currentPage, currentPage + 1, totalPages]
    .filter((page) => page >= 1 && page <= totalPages))).sort((a, b) => a - b);
  const pages: Array<number | "ellipsis"> = [];
  nearbyPages.forEach((page, index) => {
    if (index > 0) {
      const gap = page - nearbyPages[index - 1];
      if (gap === 2) pages.push(page - 1);
      else if (gap > 2) pages.push("ellipsis");
    }
    pages.push(page);
  });

  return (
    <nav aria-label="Phân trang" className={cn("mt-8 flex items-center justify-between gap-2 border-t border-stone-200 pt-6", className)}>
      <Button
        variant="outline"
        size="sm"
        disabled={currentPage === 1}
        onClick={() => onPageChange(currentPage - 1)}
        leftIcon={<ChevronLeft className="w-4 h-4" />}
      >
        Trang trước
      </Button>

      <div className="hidden md:flex items-center gap-1">
        {pages.map((page, index) => {
          if (page === "ellipsis") return <span key={`ellipsis-${index}`} className="px-1 text-content-tertiary" aria-hidden="true">…</span>;
          const isActive = page === currentPage;
          return (
            <button
              key={page}
              type="button"
              aria-label={`Trang ${page}`}
              aria-current={isActive ? "page" : undefined}
              onClick={() => onPageChange(page)}
              className={cn(
                "w-8 h-8 text-xs font-semibold rounded-lg transition-colors flex items-center justify-center",
                isActive
                  ? "bg-brand text-white shadow-xs"
                  : "text-content-secondary hover:bg-slate-100 hover:text-content-primary"
              )}
            >
              {page}
            </button>
          );
        })}
      </div>

      <span className="text-xs text-content-tertiary md:hidden">
        Trang {currentPage} / {totalPages}
      </span>

      <Button
        variant="outline"
        size="sm"
        disabled={currentPage === totalPages}
        onClick={() => onPageChange(currentPage + 1)}
        rightIcon={<ChevronRight className="w-4 h-4" />}
      >
        Trang sau
      </Button>
    </nav>
  );
};
