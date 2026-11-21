import { CategoryRecipeManager } from "@/features/categories/CategoryRecipeManager";

export default async function AdminCategoryDetailPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  return <CategoryRecipeManager slug={slug} />;
}
