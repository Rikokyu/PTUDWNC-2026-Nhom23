export default async function EditRecipePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return (
    <main>
      <h1>Chinh sua cong thuc {id}</h1>
    </main>
  );
}
