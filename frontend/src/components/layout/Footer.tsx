import Link from "next/link";
import Image from "next/image";
import { ArrowUpRight } from "lucide-react";

export function Footer() {
  return (
    <footer className="border-t border-[#34432f] bg-[#202b22] text-white">
      <div className="mx-auto max-w-[1280px] px-4 py-14 sm:px-6 sm:py-20 lg:px-8">
        <div className="grid gap-12 border-b border-white/15 pb-14 md:grid-cols-[minmax(0,1.5fr)_1fr_1fr]">
          <div>
            <Link href="/" className="inline-flex items-center gap-3">
              <Image src="/icons/logo.png" alt="" width={44} height={44} className="h-11 w-11 object-contain" />
              <span className="font-display text-xl font-bold">Culinary<span className="text-[#e5a981]">Blog</span></span>
            </Link>
            <p className="mt-5 max-w-sm text-sm leading-7 text-white/65">
              Một góc nhỏ dành cho những người thích nấu ăn, chia sẻ công thức và tìm cảm hứng cho bữa cơm mỗi ngày.
            </p>
          </div>
          <nav aria-label="Khám phá" className="space-y-4">
            <h2 className="text-xs font-bold uppercase tracking-[0.2em] text-white/45">Khám phá</h2>
            <Link href="/recipes" className="block text-sm transition-colors hover:text-[#e5a981]">Tất cả công thức</Link>
            <Link href="/categories" className="block text-sm transition-colors hover:text-[#e5a981]">Danh mục món ăn</Link>
            <Link href="/saved" className="block text-sm transition-colors hover:text-[#e5a981]">Công thức đã lưu</Link>
          </nav>
          <div className="space-y-4">
            <h2 className="text-xs font-bold uppercase tracking-[0.2em] text-white/45">Chia sẻ</h2>
            <p className="text-sm leading-6 text-white/65">Món ngon của bạn có thể truyền cảm hứng cho nhiều căn bếp khác.</p>
            <Link href="/dashboard/recipes/new" className="inline-flex items-center gap-2 border-b border-[#e5a981] pb-1 text-sm font-semibold text-white transition-colors hover:text-[#e5a981]">
              Đăng công thức <ArrowUpRight className="h-4 w-4" />
            </Link>
          </div>
        </div>
        <div className="flex flex-col gap-3 pt-6 text-xs text-white/45 sm:flex-row sm:items-center sm:justify-between">
          <p>© 2026 Culinary Blog</p>
          <p>Chuyện bếp Việt, kể bằng những món ăn.</p>
        </div>
      </div>
    </footer>
  );
}
