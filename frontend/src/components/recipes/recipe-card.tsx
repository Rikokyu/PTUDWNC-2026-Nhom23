import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";

export type RecipeCardItem = {
  id: string;
  title: string;
  slug: string;
  description: string;
  thumbnailUrl?: string | null;
  cookTimeMinutes: number;
  difficulty: "Easy" | "Medium" | "Hard" | "Expert";
  categoryName: string;
};

export function RecipeCard({ recipe }: { recipe: RecipeCardItem }) {
  return (
    <Card className="recipe-card">
      <div className="recipe-card__image">
        {recipe.thumbnailUrl ? (
          <img src={recipe.thumbnailUrl} alt={recipe.title} />
        ) : (
          <div className="recipe-card__placeholder">Culinary Blog</div>
        )}
      </div>
      <CardContent className="recipe-card__body">
        <div className="meta-row">
          <Badge variant="secondary">{recipe.categoryName}</Badge>
          <span className="muted">{recipe.difficulty}</span>
        </div>
        <h3>{recipe.title}</h3>
        <p>{recipe.description}</p>
        <div className="meta-row">
          <span className="muted">⏱️ {recipe.cookTimeMinutes} phút</span>
        </div>
      </CardContent>
      <CardFooter>
        <Button asChild className="button-primary">
          <Link href={`/recipes/${recipe.slug}`}>Xem chi tiết</Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
