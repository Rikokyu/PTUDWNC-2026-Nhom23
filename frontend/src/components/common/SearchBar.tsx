"use client";

import React, { useState } from "react";
import { Search, SlidersHorizontal, X } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { cn } from "@/lib/utils";

export interface SearchBarProps {
  initialValue?: string;
  placeholder?: string;
  onSearch: (query: string) => void;
  onToggleFilters?: () => void;
  showFilterButton?: boolean;
  className?: string;
}

export const SearchBar: React.FC<SearchBarProps> = ({
  initialValue = "",
  placeholder = "Tìm kiếm công thức, tên món ăn hoặc nguyên liệu...",
  onSearch,
  onToggleFilters,
  showFilterButton = true,
  className,
}) => {
  const [query, setQuery] = useState(initialValue);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSearch(query.trim());
  };

  const handleClear = () => {
    setQuery("");
    onSearch("");
  };

  return (
    <form
      onSubmit={handleSubmit}
      className={cn(
        "flex items-center gap-2 bg-white p-2 rounded-xl border border-slate-300 shadow-subtle focus-within:border-brand focus-within:ring-2 focus-within:ring-brand/20 transition-all duration-150",
        className
      )}
    >
      <div className="flex-1 flex items-center gap-3 px-2">
        <Search className="w-5 h-5 text-content-tertiary shrink-0" />
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder={placeholder}
          className="w-full bg-transparent text-sm text-content-primary placeholder:text-slate-400 focus:outline-none"
        />
        {query && (
          <button
            type="button"
            onClick={handleClear}
            className="p-1 rounded-full text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        )}
      </div>

      {showFilterButton && onToggleFilters && (
        <button
          type="button"
          onClick={onToggleFilters}
          className="p-2.5 text-content-secondary hover:text-content-primary hover:bg-slate-100 rounded-lg transition-colors border-l border-slate-200"
          title="Bộ lọc nâng cao"
        >
          <SlidersHorizontal className="w-4 h-4" />
        </button>
      )}

      <Button type="submit" variant="primary" size="md" className="shrink-0">
        Tìm kiếm
      </Button>
    </form>
  );
};
