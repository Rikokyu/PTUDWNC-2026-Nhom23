import { RecipeContentManager } from "@/components/recipes/RecipeContentManager";

export default async function EditRecipePage({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ from?: string; categorySlug?: string }>;
}) {
  const { id } = await params;
  const { from, categorySlug } = await searchParams;
  const returnHref = from === "categories"
    ? `/dashboard/categories${categorySlug ? `/${encodeURIComponent(categorySlug)}` : ""}`
    : "/dashboard/recipes";
  return <RecipeContentManager recipeId={id} returnHref={returnHref} />;
}
