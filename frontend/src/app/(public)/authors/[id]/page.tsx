import { AuthorProfilePageContent } from "@/components/authors/AuthorProfilePageContent";

export default async function AuthorProfilePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <AuthorProfilePageContent authorId={id} />;
}
