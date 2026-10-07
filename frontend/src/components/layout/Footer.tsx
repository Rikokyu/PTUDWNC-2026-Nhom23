"use client";

import React from "react";
import Link from "next/link";
import Image from "next/image";
import { Send } from "lucide-react";
import { Button } from "@/components/ui/Button";

export const Footer: React.FC = () => {
  return (
    <footer className="bg-slate-900 text-white pt-16 pb-12 border-t border-slate-800">
      <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-10 pb-12 border-b border-slate-800">
          {/* Brand Info */}
          <div className="lg:col-span-2 space-y-4">
            <Link href="/" className="flex items-center gap-3">
              <div className="relative w-9 h-9 flex items-center justify-center shrink-0 overflow-hidden rounded-xl">
                <Image
                  src="/icons/logo.png"
                  alt="Culinary Blog Logo"
                  width={36}
                  height={36}
                  className="object-contain w-full h-full"
                />
              </div>
              <span className="font-extrabold text-xl tracking-tight text-white">
                Culinary<span className="text-brand">Blog</span>
              </span>
            </Link>
            <p className="text-xs text-slate-400 leading-relaxed max-w-sm">
              Nền tảng chia sẻ công thức nấu ăn thuần Việt và quốc tế. Tìm kiếm ý tưởng cho bữa ăn gia đình chất lượng, dinh dưỡng và dễ chuẩn bị mỗi ngày.
            </p>
            {/* Micro Pastel Purple accent (<2% total budget) */}
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-slate-800/80 border border-slate-700/60 text-[11px] text-slate-300">
              <span className="w-1.5 h-1.5 rounded-full bg-pastel-purple animate-pulse" />
              Cập nhật công thức mới mỗi ngày
            </div>
          </div>

          {/* Quick Links */}
          <div className="space-y-3">
            <h4 className="text-xs font-semibold text-slate-200 uppercase tracking-wider">
              Khám phá
            </h4>
            <ul className="space-y-2 text-xs text-slate-400">
              <li>
                <Link href="/recipes" className="hover:text-brand transition-colors">
                  Tất cả công thức
                </Link>
              </li>
              <li>
                <Link href="/categories/mon-chinh" className="hover:text-brand transition-colors">
                  Món chính gia đình
                </Link>
              </li>
              <li>
                <Link href="/categories/mon-chay" className="hover:text-brand transition-colors">
                  Món chay thanh tịnh
                </Link>
              </li>
              <li>
                <Link href="/categories/banh-ngot" className="hover:text-brand transition-colors">
                  Bánh & Món tráng miệng
                </Link>
              </li>
              <li>
                <Link href="/categories/do-uong" className="hover:text-brand transition-colors">
                  Đồ uống & Pha chế
                </Link>
              </li>
            </ul>
          </div>

          {/* Platform Info */}
          <div className="space-y-3">
            <h4 className="text-xs font-semibold text-slate-200 uppercase tracking-wider">
              Về nền tảng
            </h4>
            <ul className="space-y-2 text-xs text-slate-400">
              <li>
                <Link href="/about" className="hover:text-brand transition-colors">
                  Giới thiệu Culinary Blog
                </Link>
              </li>
              <li>
                <Link href="/community" className="hover:text-brand transition-colors">
                  Quy chuẩn cộng đồng
                </Link>
              </li>
              <li>
                <Link href="/guidelines" className="hover:text-brand transition-colors">
                  Hướng dẫn đăng bài
                </Link>
              </li>
              <li>
                <Link href="/contact" className="hover:text-brand transition-colors">
                  Liên hệ & Hợp tác
                </Link>
              </li>
            </ul>
          </div>

          {/* Newsletter Signup */}
          <div className="space-y-3">
            <h4 className="text-xs font-semibold text-slate-200 uppercase tracking-wider">
              Nhận tin bếp tuần
            </h4>
            <p className="text-xs text-slate-400">
              Đăng ký để nhận thực đơn gợi ý tuần này vào hộp thư của bạn.
            </p>
            <form onSubmit={(e) => e.preventDefault()} className="space-y-2">
              <input
                type="email"
                placeholder="Email của bạn..."
                className="w-full bg-slate-800 border border-slate-700 text-xs text-white px-3.5 py-2.5 rounded-lg focus:outline-none focus:border-brand focus:ring-1 focus:ring-brand placeholder:text-slate-500 transition-colors"
              />
              <Button
                type="submit"
                variant="primary"
                size="sm"
                className="w-full justify-center"
                rightIcon={<Send className="w-3.5 h-3.5" />}
              >
                Đăng ký ngay
              </Button>
            </form>
          </div>
        </div>

        {/* Footer Bottom */}
        <div className="pt-8 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-500">
          <p>© 2026 Culinary Blog. Tất cả quyền được bảo lưu.</p>
          <div className="flex items-center gap-6">
            <Link href="/privacy" className="hover:text-slate-300 transition-colors">
              Chính sách bảo mật
            </Link>
            <Link href="/terms" className="hover:text-slate-300 transition-colors">
              Điều khoản sử dụng
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
};
