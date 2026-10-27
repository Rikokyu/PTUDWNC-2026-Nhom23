import { RecipeContentManager } from "@/components/recipes/RecipeContentManager";

export default async function EditRecipePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <RecipeContentManager recipeId={id} />;
}
