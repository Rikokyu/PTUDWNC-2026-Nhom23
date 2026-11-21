import { resolveApiAssetUrl } from "@/lib/api/axios";
import type { CategoryApiDto } from "@/types/api";

export function CategoryHero({ category, recipeCount }: { category: CategoryApiDto; recipeCount: number }) {
  const imageUrl = resolveApiAssetUrl(category.imageUrl);

  return (
    <header className="grid overflow-hidden rounded-3xl bg-[#202b22] text-white shadow-card sm:grid-cols-2">
      <div className="flex flex-col justify-center p-7 sm:p-10">
        <p className="mb-4 text-xs font-bold uppercase tracking-[0.25em] text-[#e5c49d]">Danh mục / Công thức</p>
        <h1 className="font-display text-4xl font-bold tracking-tight text-white sm:text-5xl">{category.name}</h1>
        <p className="mt-4 max-w-xl text-base leading-7 text-stone-200">{category.description || "Khám phá những món ăn được yêu thích trong danh mục này."}</p>
        <p className="mt-6 text-xs font-bold uppercase tracking-widest text-[#e5c49d]">{recipeCount} công thức</p>
      </div>
      <div className="relative min-h-52 bg-[#9f9b7b] sm:min-h-80">
        {imageUrl && <img src={imageUrl} alt={`Món ăn thuộc danh mục ${category.name}`} className="absolute inset-0 h-full w-full object-cover" decoding="async" onError={(event) => { event.currentTarget.style.display = "none"; }} />}
      </div>
    </header>
  );
}
