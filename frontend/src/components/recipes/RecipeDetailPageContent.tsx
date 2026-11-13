"use client";

import Link from "next/link";
import { useRecipe } from "@/hooks/useRecipes";
import { MOCK_RECIPES } from "@/lib/mockData";
import { ErrorState } from "@/components/ui/ErrorState";
import { RecipeDetailView } from "@/features/recipes/RecipeDetailView";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { resolveApiAssetUrl } from "@/lib/api/axios";
<<<<<<< HEAD
=======
import { SavedRecipeBookmarkButton } from "@/components/recipes/SavedRecipeBookmarkButton";
import { Clock, Flame, Users } from "lucide-react";
>>>>>>> 315339d (Update UI)

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
  const heroImage = resolveApiAssetUrl(
    recipe.images.find((image) => image.isPrimary)?.mediumUrl ??
    recipe.images[0]?.mediumUrl ??
    recipe.images[0]?.originalUrl,
  );

  return (
    <article className="mx-auto max-w-6xl space-y-9 pb-12">
      <Link href="/recipes" className="text-xs font-bold uppercase tracking-[0.15em] text-brand hover:underline">
        ← Tất cả công thức
      </Link>
      <header className="max-w-4xl space-y-5">
        <p className="text-xs font-bold uppercase tracking-[0.25em] text-brand">{recipe.category.name}</p>
        <h1 className="font-display text-4xl font-bold leading-tight text-content-primary sm:text-5xl lg:text-6xl">
          {recipe.title}
        </h1>
        <p className="max-w-3xl text-base leading-8 text-content-secondary sm:text-lg">{recipe.description}</p>
        <p className="border-t border-zinc-300 pt-4 text-sm font-semibold text-content-secondary">
          Công thức bởi {recipe.author?.displayName ?? "Culinary Blog"}
        </p>
      </header>
      {heroImage && (
        <img
          src={heroImage}
          alt={recipe.title}
          className="aspect-[16/9] max-h-[38rem] w-full bg-zinc-100 object-cover"
        />
      )}

      <div className="grid grid-cols-2 gap-5 border-y border-zinc-300 py-5 text-sm sm:grid-cols-4">
        <div className="flex items-center gap-3"><Clock className="h-5 w-5 text-brand" /><span><small className="block text-content-tertiary">Chuẩn bị</small><strong>{recipe.prepTimeMinutes} phút</strong></span></div>
        <div className="flex items-center gap-3"><Flame className="h-5 w-5 text-brand" /><span><small className="block text-content-tertiary">Nấu</small><strong>{recipe.cookTimeMinutes} phút</strong></span></div>
        <div className="flex items-center gap-3"><Users className="h-5 w-5 text-brand" /><span><small className="block text-content-tertiary">Khẩu phần</small><strong>{recipe.servings} người</strong></span></div>
        <div className="flex items-center gap-3"><span className="text-lg text-brand" aria-hidden="true">✦</span><span><small className="block text-content-tertiary">Độ khó</small><strong>{difficulty}</strong></span></div>
      </div>

      <div className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.7fr)] lg:gap-14">
      <section className="border-t-4 border-brand bg-white p-6 shadow-subtle lg:self-start lg:sticky lg:top-24">
        <p className="mb-2 text-xs font-bold uppercase tracking-[0.2em] text-brand">Chuẩn bị</p>
        <h2 className="font-display text-2xl font-bold">Nguyên liệu</h2>
        <ul className="mt-6 divide-y divide-zinc-200">
          {recipe.ingredients.map((ingredient) => (
            <li key={ingredient.id} className="py-3 text-sm leading-6">
              <span className="font-semibold text-content-primary">{ingredient.name}</span>
              {ingredient.quantity && ` — ${ingredient.quantity}`}
              {ingredient.unit && ` ${ingredient.unit}`}
              {ingredient.notes && <span className="block text-content-tertiary">{ingredient.notes}</span>}
            </li>
          ))}
        </ul>
      </section>

      <section className="space-y-6">
        <p className="text-xs font-bold uppercase tracking-[0.2em] text-brand">Vào bếp</p>
        <h2 className="font-display text-3xl font-bold">Cách thực hiện</h2>
        {recipe.steps.map((step) => (
          <article key={step.id} className="grid grid-cols-[3rem_1fr] gap-4 border-t border-zinc-300 pt-6 sm:gap-6">
            <span className="font-display text-3xl font-bold text-brand/70" aria-hidden="true">{String(step.stepNumber).padStart(2, "0")}</span>
            <div><h3 className="font-display text-lg font-bold">
              Bước {step.stepNumber}{step.title ? `: ${step.title}` : ""}
            </h3>
            <p className="mt-3 whitespace-pre-line text-base leading-7 text-content-secondary">
              {step.description}
            </p></div>
          </article>
        ))}
      </section>
      </div>

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
