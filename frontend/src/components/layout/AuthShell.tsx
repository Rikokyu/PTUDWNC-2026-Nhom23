import type { ReactNode } from "react";
import Link from "next/link";

export function AuthShell({ title, description, children }: { title: string; description: string; children: ReactNode }) {
  return (
    <main id="main-content" className="grid min-h-screen bg-surface-cream lg:grid-cols-2">
      <div className="relative hidden overflow-hidden bg-content-primary lg:flex lg:flex-col lg:justify-between lg:p-12 xl:p-16">
        <div className="absolute inset-0 bg-[url('/images/hero-pho-editorial.webp')] bg-cover bg-center" />
        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/30 to-black/35" />
        <Link href="/" className="relative font-display text-2xl font-bold text-white">Culinary<span className="text-brand">Blog</span></Link>
        <div className="relative max-w-md">
          <p className="mb-4 text-xs font-bold uppercase tracking-[0.25em] text-white/75">Câu chuyện từ căn bếp Việt</p>
          <p className="font-display text-4xl font-bold leading-tight text-white xl:text-5xl">Món ngon bắt đầu từ niềm vui nấu nướng.</p>
        </div>
      </div>
      <div className="flex min-h-screen flex-col justify-center px-5 py-12 sm:px-10 lg:px-16 xl:px-24">
        <div className="mx-auto w-full max-w-md">
          <Link href="/" className="mb-12 inline-block font-display text-xl font-bold text-content-primary lg:hidden">Culinary<span className="text-brand">Blog</span></Link>
          <p className="mb-3 text-xs font-bold uppercase tracking-[0.2em] text-brand">Chào mừng đến với Culinary Blog</p>
          <h1 className="font-display text-3xl font-bold text-content-primary sm:text-4xl">{title}</h1>
          <p className="mt-3 text-sm leading-7 text-content-secondary">{description}</p>
          <div className="mt-8 border-t border-stone-200 pt-8">{children}</div>
        </div>
      </div>
    </main>
  );
}
