"use client";

import Link from "next/link";
import { useRecipe } from "@/hooks/useRecipes";
import { MOCK_RECIPES } from "@/lib/mockData";
import { ErrorState } from "@/components/legacy-ui/ErrorState";
import { RecipeDetailView } from "@/features/recipes/RecipeDetailView";
import { RecipeCardSkeleton } from "@/components/legacy-ui/LoadingState";

const demoEnabled = process.env.NEXT_PUBLIC_USE_DEMO_DATA === "true";

export function RecipeDetailPageContent({ slug }: { slug: string }) {
  const recipeQuery = useRecipe(slug);
  const demoRecipe = demoEnabled
    ? MOCK_RECIPES.find((recipe) => recipe.slug === slug)
    : undefined;

  if (recipeQuery.isLoading) {
    return <RecipeCardSkeleton />;
  }

  if (recipeQuery.isError) {
    if (demoRecipe) {
      return (
        <div className="space-y-4">
          <p role="status" className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-900">
            Đang hiển thị dữ liệu mẫu vì API chưa kết nối được. Đây không phải dữ liệu từ máy chủ.
          </p>
          <RecipeDetailView recipe={demoRecipe} />
        </div>
      );
    }
    return (
      <ErrorState
        title="Không tải được công thức"
        message="Công thức không tồn tại hoặc API chưa khả dụng."
        onRetry={() => void recipeQuery.refetch()}
      />
    );
  }

  const recipe = recipeQuery.data;
  if (!recipe) {
    return (
      <ErrorState
        title="Không tìm thấy công thức"
        message="Máy chủ không trả về dữ liệu cho công thức này."
      />
    );
  }

  const difficulty = ["", "Dễ", "Trung bình", "Khó", "Chuyên gia"][
    recipe.difficulty
  ] ?? "Chưa phân loại";
  const heroImage =
    recipe.images.find((image) => image.isPrimary)?.mediumUrl ??
    recipe.images[0]?.mediumUrl ??
    recipe.images[0]?.originalUrl;

  return (
    <article className="mx-auto max-w-4xl space-y-8">
      <Link href="/recipes" className="text-sm font-semibold text-brand hover:underline">
        ← Tất cả công thức
      </Link>
      {heroImage && (
        <img
          src={heroImage}
          alt={recipe.title}
          className="max-h-[28rem] w-full rounded-2xl bg-slate-100 object-cover"
        />
      )}
      <header className="space-y-3">
        <p className="text-sm font-semibold text-brand">{recipe.category.name}</p>
        <h1 className="text-3xl font-extrabold text-content-primary sm:text-4xl">
          {recipe.title}
        </h1>
        <p className="leading-7 text-content-secondary">{recipe.description}</p>
        <p className="text-sm text-content-tertiary">
          {recipe.author?.displayName ?? "Culinary Blog"} · {difficulty} ·{" "}
          {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút · {recipe.servings} khẩu phần
        </p>
      </header>

      <section className="rounded-xl border border-slate-200 bg-white p-5">
        <h2 className="text-xl font-bold">Nguyên liệu</h2>
        <ul className="mt-4 space-y-2">
          {recipe.ingredients.map((ingredient) => (
            <li key={ingredient.id} className="border-b border-slate-100 py-2 text-sm">
              <span className="font-medium">{ingredient.name}</span>
              {ingredient.quantity && ` — ${ingredient.quantity}`}
              {ingredient.unit && ` ${ingredient.unit}`}
              {ingredient.notes && ` (${ingredient.notes})`}
            </li>
          ))}
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-xl font-bold">Cách thực hiện</h2>
        {recipe.steps.map((step) => (
          <article key={step.id} className="rounded-xl border border-slate-200 bg-white p-5">
            <h3 className="font-semibold">
              Bước {step.stepNumber}{step.title ? `: ${step.title}` : ""}
            </h3>
            <p className="mt-2 whitespace-pre-line text-sm leading-6 text-content-secondary">
              {step.description}
            </p>
          </article>
        ))}
      </section>

      {recipe.nutrition && (
        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <h2 className="text-xl font-bold">Dinh dưỡng</h2>
          <dl className="mt-4 grid grid-cols-2 gap-3 text-sm sm:grid-cols-3">
            {Object.entries(recipe.nutrition).map(([label, value]) =>
              value === null ? null : (
                <div key={label}>
                  <dt className="capitalize text-content-tertiary">{label}</dt>
                  <dd className="font-semibold">{value}</dd>
                </div>
              ),
            )}
          </dl>
        </section>
      )}
    </article>
  );
}
