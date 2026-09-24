import Link from "next/link";

export const revalidate = 3600;

export default function RecipesPage() {
  return (
    <main>
      <h1>Cac cong thuc</h1>
      <p>Danh sach cong thuc duoc render theo ISR.</p>
      <Link href="/recipes/search">Tim kiem cong thuc</Link>
    </main>
  );
}
