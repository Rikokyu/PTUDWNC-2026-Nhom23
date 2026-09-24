import Link from "next/link";

export default function HomePage() {
  return (
    <main>
      <h1>Culinary Blog</h1>
      <p>Kham pha va chia se cong thuc nau an.</p>
      <Link href="/recipes">Xem cong thuc</Link>
    </main>
  );
}
