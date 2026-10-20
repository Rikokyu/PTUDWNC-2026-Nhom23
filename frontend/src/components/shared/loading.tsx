import { Skeleton } from "@/components/ui/skeleton";

export function Loading({ label = "Đang tải..." }: { label?: string }) {
  return (
    <div className="stack" aria-live="polite" role="status">
      <span className="sr-only">{label}</span>
      <div className="recipe-grid" aria-hidden="true">
        {Array.from({ length: 3 }, (_, index) => (
          <div className="recipe-card" key={index}>
            <Skeleton className="recipe-card__image" />
            <div className="recipe-card__body stack">
              <Skeleton className="h-4 w-1/3" />
              <Skeleton className="h-6 w-3/4" />
              <Skeleton className="h-4 w-full" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
