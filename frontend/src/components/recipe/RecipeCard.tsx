import Link from "next/link";
import type { RecipeListDto } from "@/types/api";

export function RecipeCard({ recipe }: { recipe: RecipeListDto }) {
  return (
    <article>
      <h2>
        <Link href={`/recipes/${recipe.slug}`}>{recipe.title}</Link>
      </h2>
      <p>{recipe.description}</p>
    </article>
  );
}
