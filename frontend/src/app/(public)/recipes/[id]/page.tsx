import { notFound } from "next/navigation";

export default async function RecipeDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id: slug } = await params;
  if (!slug) notFound();

  return (
    <main>
      <h1>Chi tiet cong thuc</h1>
      <p>Slug: {slug}</p>
    </main>
  );
}
