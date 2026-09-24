import type { RecipeDto } from "@/types/api";

export function RecipeDetail({ recipe }: { recipe: RecipeDto }) {
  return (
    <article>
      <h1>{recipe.title}</h1>
      <p>{recipe.description}</p>
    </article>
  );
}
