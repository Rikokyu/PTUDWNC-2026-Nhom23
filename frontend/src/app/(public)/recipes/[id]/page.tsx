"use client";

import { useCallback, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { MainLayout } from "@/components/layout/main-layout";
import { ErrorMessage } from "@/components/shared/error-message";
import { Loading } from "@/components/shared/loading";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { recipeApi } from "@/lib/api/recipes";
import type { RecipeDto } from "@/types/api";

export default function RecipeDetailPage() {
  const params = useParams<{ id: string }>();
  const [recipe, setRecipe] = useState<RecipeDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadRecipe = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await recipeApi.getBySlug(params.id);
      setRecipe(result);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Không thể tải công thức.");
      setRecipe(null);
    } finally {
      setIsLoading(false);
    }
  }, [params.id]);

  useEffect(() => {
    void loadRecipe();
  }, [loadRecipe]);

  return (
    <MainLayout>
      {isLoading ? <Loading label="Đang tải công thức..." /> : null}
      {error ? <ErrorMessage message={error} onRetry={() => void loadRecipe()} /> : null}
      {recipe ? (
        <article className="section-block">
          <div className="section-heading">
            <div>
              <Badge variant="secondary">{recipe.category.name}</Badge>
              <h1>{recipe.title}</h1>
              <p className="muted">
                {recipe.prepTimeMinutes} phút chuẩn bị · {recipe.cookTimeMinutes} phút nấu ·{" "}
                {recipe.servings} khẩu phần
              </p>
            </div>
            <Badge variant="outline">{recipe.difficulty}</Badge>
          </div>
          <Card>
            <CardHeader>
              <CardTitle>Giới thiệu</CardTitle>
            </CardHeader>
            <CardContent>
              <p>{recipe.description}</p>
            </CardContent>
          </Card>
          {recipe.ingredients.length ? (
            <Card className="detail-card">
              <CardHeader>
                <CardTitle>Nguyên liệu</CardTitle>
              </CardHeader>
              <CardContent>
                <ul>
                  {recipe.ingredients.map((ingredient) => (
                    <li key={ingredient.id}>
                      {ingredient.name}
                      {ingredient.quantity ? ` — ${ingredient.quantity}` : ""}
                      {ingredient.unit ? ` ${ingredient.unit}` : ""}
                    </li>
                  ))}
                </ul>
              </CardContent>
            </Card>
          ) : null}
          {recipe.steps.length ? (
            <Card className="detail-card">
              <CardHeader>
                <CardTitle>Cách thực hiện</CardTitle>
              </CardHeader>
              <CardContent>
                <ol className="detail-steps">
                  {recipe.steps.map((step) => (
                    <li key={step.id}>
                      {step.title ? <strong>{step.title}: </strong> : null}
                      {step.description}
                    </li>
                  ))}
                </ol>
              </CardContent>
            </Card>
          ) : (
            <Card className="detail-card">
              <CardHeader>
                <CardTitle>Cách thực hiện</CardTitle>
              </CardHeader>
              <CardContent>
                <p>{recipe.instructions}</p>
              </CardContent>
            </Card>
          )}
        </article>
      ) : null}
    </MainLayout>
  );
}
