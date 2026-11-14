import Link from "next/link";
import { SearchX } from "lucide-react";

export default function NotFound() {
  return (
    <main className="mx-auto flex min-h-[70vh] max-w-3xl items-center justify-center px-4 py-16">
      <div className="w-full rounded-3xl border border-brand-border bg-gradient-to-b from-white to-brand-light/40 px-6 py-14 text-center shadow-card sm:px-12">
        <span className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-2xl border border-brand-border bg-white text-brand shadow-subtle" aria-hidden="true">
          <SearchX className="h-8 w-8" />
        </span>
        <p className="mb-2 text-sm font-bold uppercase tracking-[0.2em] text-brand">Lỗi 404</p>
        <h1 className="font-display text-3xl font-bold text-content-primary sm:text-4xl">Trang này không còn ở đây</h1>
        <p className="mx-auto mt-4 max-w-md text-sm leading-7 text-content-secondary sm:text-base">
          Đường dẫn có thể đã thay đổi. Bạn có thể về trang chủ hoặc tiếp tục khám phá công thức.
        </p>
        <div className="mt-8 flex flex-wrap justify-center gap-3">
          <Link href="/" className="rounded-lg bg-brand px-5 py-3 text-sm font-semibold text-white transition-colors hover:bg-brand-hover">
            Về trang chủ
          </Link>
          <Link href="/recipes" className="rounded-lg border border-brand-border bg-white px-5 py-3 text-sm font-semibold text-brand transition-colors hover:bg-brand-light">
            Xem công thức
          </Link>
        </div>
      </div>
    </main>
  );
}
