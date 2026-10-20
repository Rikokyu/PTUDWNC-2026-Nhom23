import { EmptyState } from "@/components/shared/empty-state";
import { Loading } from "@/components/shared/loading";
import {
  RecipeCard,
  type RecipeCardItem,
} from "@/components/recipes/recipe-card";

export function RecipeGrid({
  items,
  isLoading,
  emptyDescription,
  emptyTitle = "Chưa có công thức nào",
}: {
  items: RecipeCardItem[];
  isLoading?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
}) {
  if (isLoading) {
    return <Loading label="Đang tải công thức..." />;
  }

  if (items.length === 0) {
    return (
      <EmptyState
        title={emptyTitle}
        description={
          emptyDescription ?? "Hãy thử từ khóa khác hoặc quay lại sau."
        }
        actionLabel="Về trang chủ"
        actionHref="/"
      />
    );
  }

  return (
    <div className="recipe-grid">
      {items.map((recipe) => (
        <RecipeCard key={recipe.id} recipe={recipe} />
      ))}
    </div>
  );
}
