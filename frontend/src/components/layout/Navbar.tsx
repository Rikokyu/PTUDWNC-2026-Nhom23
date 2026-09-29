"use client";

import React, { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { usePathname } from "next/navigation";
import { Search, Plus, Menu, X, ChefHat, User, Bookmark, LogOut } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { cn } from "@/lib/utils";

export interface NavbarProps {
  user?: {
    name: string;
    email: string;
    avatarUrl?: string;
  } | null;
}

export const Navbar: React.FC<NavbarProps> = ({ user }) => {
  const pathname = usePathname();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userDropdownOpen, setUserDropdownOpen] = useState(false);

  const navLinks = [
    { label: "Trang chủ", href: "/" },
    { label: "Công thức", href: "/recipes" },
    { label: "Danh mục", href: "/categories" },
    { label: "Bộ sưu tập", href: "/collections" },
  ];

  const isActive = (path: string) => {
    if (path === "/" && pathname === "/") return true;
    if (path !== "/" && pathname.startsWith(path)) return true;
    return false;
  };

  return (
    <header className="sticky top-0 z-40 bg-white/95 backdrop-blur-xs border-b border-slate-200/80">
      <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between gap-4">
        {/* Brand Logo */}
        <Link href="/" className="flex items-center gap-3 shrink-0 group">
          <div className="relative w-9 h-9 flex items-center justify-center shrink-0 overflow-hidden rounded-xl transition-transform duration-200 group-hover:scale-105">
            <Image
              src="/icons/logo.png"
              alt="Culinary Blog Logo"
              width={36}
              height={36}
              className="object-contain w-full h-full"
              priority
            />
          </div>
          <div className="flex flex-col">
            <span className="font-extrabold text-lg tracking-tight text-content-primary leading-none">
              Culinary<span className="text-brand">Blog</span>
            </span>
            <span className="text-[10px] font-medium text-content-tertiary tracking-wider uppercase leading-tight">
              Bếp Việt & Công Thức
            </span>
          </div>
        </Link>

        {/* Desktop Nav Links */}
        <nav className="hidden md:flex items-center gap-1">
          {navLinks.map((link) => {
            const active = isActive(link.href);
            return (
              <Link
                key={link.href}
                href={link.href}
                className={cn(
                  "px-3.5 py-2 text-sm font-medium rounded-lg transition-colors",
                  active
                    ? "text-brand font-semibold bg-brand-light/60"
                    : "text-content-secondary hover:text-content-primary hover:bg-slate-50"
                )}
              >
                {link.label}
              </Link>
            );
          })}
        </nav>

        {/* Search & Actions */}
        <div className="flex items-center gap-3">
          <Link
            href="/recipes/search"
            className="hidden sm:flex items-center gap-2 px-3 py-1.5 rounded-lg border border-slate-200 bg-slate-50 text-content-tertiary hover:border-slate-300 hover:text-content-secondary text-xs transition-colors"
          >
            <Search className="w-3.5 h-3.5" />
            <span>Tìm món ăn, nguyên liệu...</span>
            <kbd className="hidden lg:inline-block px-1.5 py-0.5 text-[10px] font-mono bg-white border border-slate-200 rounded text-slate-400">
              ⌘K
            </kbd>
          </Link>

          {user ? (
            <div className="flex items-center gap-3">
              <Link href="/dashboard/recipes/new">
                <Button
                  variant="primary"
                  size="sm"
                  leftIcon={<Plus className="w-4 h-4" />}
                  className="hidden sm:inline-flex"
                >
                  Đăng công thức
                </Button>
              </Link>

              {/* User Dropdown Trigger */}
              <div className="relative">
                <button
                  onClick={() => setUserDropdownOpen(!userDropdownOpen)}
                  className="flex items-center gap-2 p-1 rounded-full border border-slate-200 hover:border-brand transition-colors focus:outline-none"
                >
                  <div className="w-8 h-8 rounded-full bg-brand-light text-brand-hover flex items-center justify-center font-bold text-xs uppercase">
                    {user.name.charAt(0)}
                  </div>
                </button>

                {/* User Dropdown Menu */}
                {userDropdownOpen && (
                  <div
                    className="absolute right-0 mt-2 w-56 bg-white rounded-xl shadow-dropdown border border-slate-200 py-1.5 z-50 animate-in fade-in slide-in-from-top-1 duration-150"
                    onMouseLeave={() => setUserDropdownOpen(false)}
                  >
                    <div className="px-4 py-2.5 border-b border-slate-100">
                      <p className="text-xs font-semibold text-content-primary truncate">
                        {user.name}
                      </p>
                      <p className="text-[11px] text-content-tertiary truncate">
                        {user.email}
                      </p>
                    </div>

                    <div className="py-1">
                      <Link
                        href="/dashboard/profile"
                        onClick={() => setUserDropdownOpen(false)}
                        className="flex items-center gap-2.5 px-4 py-2 text-xs text-content-secondary hover:text-content-primary hover:bg-slate-50 transition-colors"
                      >
                        <User className="w-4 h-4 text-content-tertiary" />
                        Trang cá nhân
                      </Link>
                      <Link
                        href="/dashboard/recipes"
                        onClick={() => setUserDropdownOpen(false)}
                        className="flex items-center gap-2.5 px-4 py-2 text-xs text-content-secondary hover:text-content-primary hover:bg-slate-50 transition-colors"
                      >
                        <ChefHat className="w-4 h-4 text-content-tertiary" />
                        Công thức của tôi
                      </Link>
                      <Link
                        href="/saved"
                        onClick={() => setUserDropdownOpen(false)}
                        className="flex items-center gap-2.5 px-4 py-2 text-xs text-content-secondary hover:text-content-primary hover:bg-slate-50 transition-colors"
                      >
                        <Bookmark className="w-4 h-4 text-content-tertiary" />
                        Công thức đã lưu
                      </Link>
                    </div>

                    <div className="border-t border-slate-100 pt-1">
                      <button
                        onClick={() => setUserDropdownOpen(false)}
                        className="w-full flex items-center gap-2.5 px-4 py-2 text-xs text-rose-600 hover:bg-rose-50 transition-colors text-left"
                      >
                        <LogOut className="w-4 h-4" />
                        Đăng xuất
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Link href="/auth/login">
                <Button variant="ghost" size="sm">
                  Đăng nhập
                </Button>
              </Link>
              <Link href="/auth/register">
                <Button variant="primary" size="sm">
                  Đăng ký
                </Button>
              </Link>
            </div>
          )}

          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="md:hidden p-2 rounded-lg text-content-secondary hover:text-content-primary hover:bg-slate-100 transition-colors"
            aria-label="Toggle menu"
          >
            {mobileMenuOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
          </button>
        </div>
      </div>

      {/* Mobile Drawer Menu */}
      {mobileMenuOpen && (
        <div className="md:hidden border-t border-slate-200 bg-white px-4 pt-3 pb-6 space-y-3 animate-in slide-in-from-top-2 duration-150">
          <div className="mb-3">
            <Link
              href="/recipes/search"
              onClick={() => setMobileMenuOpen(false)}
              className="flex items-center gap-2 px-3 py-2 rounded-lg border border-slate-200 bg-slate-50 text-content-secondary text-xs"
            >
              <Search className="w-4 h-4 text-content-tertiary" />
              <span>Tìm kiếm công thức nấu ăn...</span>
            </Link>
          </div>

          <div className="space-y-1">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                onClick={() => setMobileMenuOpen(false)}
                className={cn(
                  "block px-3 py-2.5 rounded-lg text-sm font-medium transition-colors",
                  isActive(link.href)
                    ? "bg-brand-light text-brand-hover font-semibold"
                    : "text-content-secondary hover:bg-slate-50 hover:text-content-primary"
                )}
              >
                {link.label}
              </Link>
            ))}
          </div>

          {user && (
            <div className="pt-3 border-t border-slate-100 space-y-2">
              <Link
                href="/dashboard/recipes/new"
                onClick={() => setMobileMenuOpen(false)}
              >
                <Button variant="primary" size="md" className="w-full justify-center" leftIcon={<Plus className="w-4 h-4" />}>
                  Đăng công thức mới
                </Button>
              </Link>
            </div>
          )}
        </div>
      )}
    </header>
  );
};
