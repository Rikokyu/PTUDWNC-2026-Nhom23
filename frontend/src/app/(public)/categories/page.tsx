"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { MainLayout } from "@/components/layout/main-layout";
import { EmptyState } from "@/components/shared/empty-state";
import { ErrorMessage } from "@/components/shared/error-message";
import { Loading } from "@/components/shared/loading";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { categoryApi } from "@/lib/api/categories";
import type { CategoryDto } from "@/types/api";

export default function CategoriesPage() {
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadCategories = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      setCategories(await categoryApi.getList());
    } catch (requestError) {
      setCategories([]);
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Không thể tải danh mục công thức.",
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadCategories();
  }, [loadCategories]);

  return (
    <MainLayout>
      <section className="section-block">
        <div className="section-heading">
          <div>
            <h1>Danh mục công thức</h1>
            <p className="muted">Khám phá công thức theo chủ đề và loại món ăn.</p>
          </div>
        </div>

        {error ? (
          <ErrorMessage message={error} onRetry={() => void loadCategories()} />
        ) : null}
        {isLoading ? <Loading label="Đang tải danh mục..." /> : null}
        {!isLoading && !error && categories.length === 0 ? (
          <EmptyState
            title="Chưa có danh mục nào"
            description="Các danh mục công thức sẽ xuất hiện tại đây."
          />
        ) : null}
        {!isLoading && !error && categories.length > 0 ? (
          <div className="recipe-grid">
            {categories.map((category) => (
              <Card key={category.id} className="recipe-card">
                {category.imageUrl ? (
                  <div className="recipe-card__image">
                    <img src={category.imageUrl} alt={category.name} />
                  </div>
                ) : null}
                <CardHeader>
                  <div className="meta-row">
                    <Badge variant="secondary">{category.recipeCount} công thức</Badge>
                  </div>
                  <CardTitle>{category.name}</CardTitle>
                </CardHeader>
                <CardContent>
                  <p>{category.description || "Khám phá các công thức trong danh mục này."}</p>
                </CardContent>
                <CardFooter>
                  <Button asChild className="button-primary">
                    <Link href={`/categories/${category.slug}`}>Xem danh mục</Link>
                  </Button>
                </CardFooter>
              </Card>
            ))}
          </div>
        ) : null}
      </section>
    </MainLayout>
  );
}
