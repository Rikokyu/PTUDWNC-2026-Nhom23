"use client";

import { useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import { Bookmark } from "lucide-react";
import { useAuth } from "@/components/auth/AuthProvider";
import { getApiErrorMessage } from "@/lib/api/error";
import { cn } from "@/lib/utils";
import { useRemoveSavedRecipe, useSaveRecipe, useSavedRecipeIds } from "@/hooks/useSavedRecipes";

interface SavedRecipeBookmarkButtonProps {
  recipeId: string;
  className?: string;
  showLabel?: boolean;
  returnTo?: string;
}

export function SavedRecipeBookmarkButton({
  recipeId,
  className,
  showLabel = false,
  returnTo,
}: SavedRecipeBookmarkButtonProps) {
  const router = useRouter();
  const pathname = usePathname();
  const { user, status } = useAuth();
  const savedIdsQuery = useSavedRecipeIds();
  const saveMutation = useSaveRecipe();
  const removeMutation = useRemoveSavedRecipe();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const isSaved = savedIdsQuery.data?.includes(recipeId) ?? false;
  const isPending = saveMutation.isPending || removeMutation.isPending;
  const canSave = user?.role === "Author" || user?.role === "Admin";

  if (status === "authenticated" && !canSave) {
    return null;
  }

  const handleClick = async () => {
    setErrorMessage(null);
    if (status === "guest") {
      const next = returnTo ?? pathname ?? "/";
      router.push(`/auth/login?next=${encodeURIComponent(next)}`);
      return;
    }
    if (status !== "authenticated" || !canSave || savedIdsQuery.isLoading) {
      return;
    }

    try {
      await (isSaved
        ? removeMutation.mutateAsync(recipeId)
        : saveMutation.mutateAsync(recipeId));
    } catch (error) {
      setErrorMessage(getApiErrorMessage(error));
    }
  };

  const label = status === "authenticated" && isSaved ? "Đã lưu" : "Lưu công thức";

  return (
    <span className="relative inline-flex">
      <button
        type="button"
        onClick={() => void handleClick()}
        disabled={status === "loading" || savedIdsQuery.isLoading || isPending}
        aria-label={label}
        aria-pressed={isSaved}
        title={status === "guest" ? "Đăng nhập để lưu công thức" : label}
        className={cn(
          "inline-flex items-center justify-center gap-2 rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-brand/40 disabled:cursor-wait disabled:opacity-60",
          showLabel
            ? "min-h-10 rounded-lg border px-4 text-sm font-semibold"
            : "h-9 w-9 border shadow-sm",
          isSaved
            ? "border-brand bg-brand-light text-brand-hover"
            : "border-slate-200 bg-white/95 text-slate-600 hover:border-brand hover:text-brand",
          className,
        )}
      >
        <Bookmark className={cn("h-4 w-4", isSaved && "fill-current")} />
        {showLabel && <span>{label}</span>}
      </button>
      {isSaved && !showLabel && (
        <span className="pointer-events-none absolute right-0 top-full z-20 mt-1 whitespace-nowrap rounded-md bg-brand px-2 py-0.5 text-[10px] font-semibold text-white shadow-sm">
          Đã lưu
        </span>
      )}
      {errorMessage && (
        <span
          role="alert"
          className="absolute right-0 top-full z-20 mt-1 w-56 rounded-md bg-rose-700 px-2 py-1 text-left text-xs text-white shadow"
        >
          {errorMessage}
        </span>
      )}
    </span>
  );
}
