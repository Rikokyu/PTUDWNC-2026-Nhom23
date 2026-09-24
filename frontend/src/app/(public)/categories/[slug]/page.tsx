export const revalidate = 3600;

export default async function CategoryPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  return (
    <main>
      <h1>Danh muc: {slug}</h1>
    </main>
  );
}
