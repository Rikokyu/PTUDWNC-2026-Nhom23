import { MainLayout } from "@/components/layout/main-layout";
import { RecipeDetailPageContent } from "@/components/recipes/RecipeDetailPageContent";

export default async function RecipeDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  return <MainLayout><RecipeDetailPageContent slug={slug} /></MainLayout>;
}
