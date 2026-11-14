"use client";

import React, { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { usePathname, useRouter } from "next/navigation";
import { Search, Plus, Menu, X, ChefHat, User, Bookmark, LogOut } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { cn } from "@/lib/utils";
import { authApi } from "@/lib/api/auth";
import { AvatarImage } from "@/components/common/AvatarImage";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import { useAuthStore } from "@/store/useAuthStore";

export interface NavbarProps {
  user?: {
    name: string;
    email: string;
    avatarUrl?: string;
  } | null;
}

export const Navbar: React.FC<NavbarProps> = ({ user }) => {
  const pathname = usePathname();
  const router = useRouter();
  const authUser = useAuthStore((state) => state.user);
  const clearSession = useAuthStore((state) => state.clearSession);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userDropdownOpen, setUserDropdownOpen] = useState(false);
  const currentUser = user ?? (authUser
    ? { name: authUser.displayName, email: authUser.email, avatarUrl: authUser.avatarUrl ?? undefined }
    : null);
  const currentAvatarUrl = resolveApiAssetUrl(currentUser?.avatarUrl);
  const isAdmin = authUser?.role === "Admin";
  const isHome = pathname === "/";

  async function handleLogout() {
    try {
      await authApi.logout();
    } catch {
      // Always clear the local session, even if the API is unavailable.
    } finally {
      clearSession();
      setUserDropdownOpen(false);
      router.push("/");
    }
  }

  const navLinks = [
    { label: "Trang chủ", href: "/" },
    { label: "Công thức", href: "/recipes" },
    { label: "Danh mục", href: "/categories" },
    { label: "Đã lưu", href: "/saved" },
  ];

  const isActive = (path: string) => {
    if (path === "/" && pathname === "/") return true;
    if (path !== "/" && pathname.startsWith(path)) return true;
    return false;
  };

  return (
    <header
      className={cn(
        "z-40 border-b backdrop-blur-xl transition-colors duration-200",
        isHome
          ? "fixed inset-x-0 top-0 border-white/15 bg-[#20271f]/75 text-white"
          : "sticky top-0 border-stone-200/80 bg-surface/95 text-content-primary shadow-glass"
      )}
    >
      <a href="#main-content" className="sr-only rounded-lg bg-surface px-4 py-2 font-semibold text-content-primary focus:not-sr-only focus:absolute focus:left-4 focus:top-3 focus:z-50">Đi đến nội dung</a>
      <div className="mx-auto flex h-[4.5rem] max-w-[1280px] items-center justify-between gap-4 px-4 sm:px-6 lg:px-8">
        <Link href="/" className="group flex shrink-0 items-center gap-3">
          <div className="relative flex h-9 w-9 shrink-0 items-center justify-center overflow-hidden rounded-lg transition-transform duration-200 group-hover:scale-105">
            <Image
              src="/icons/logo.png"
              alt="Culinary Blog Logo"
              width={36}
              height={36}
              className="h-full w-full object-contain"
              priority
            />
          </div>
          <div className="flex flex-col">
            <span
              className={cn(
                "font-display text-lg font-extrabold leading-none tracking-tight",
                isHome ? "text-white" : "text-content-primary"
              )}
            >
              Culinary<span className="text-brand">Blog</span>
            </span>
            <span
              className={cn(
                "text-[10px] font-medium uppercase leading-tight tracking-[0.14em]",
                isHome ? "text-white/60" : "text-content-tertiary"
              )}
            >
              Bếp Việt hiện đại
            </span>
          </div>
        </Link>

        <nav className="hidden items-center gap-1 lg:flex">
          {navLinks.map((link) => {
            const active = isActive(link.href);
            return (
              <Link
                key={link.href}
                href={link.href}
                aria-current={active ? "page" : undefined}
                className={cn(
                  "relative rounded-lg px-3.5 py-2 text-sm font-semibold transition-colors hover:bg-white/10",
                  isHome
                    ? active
                      ? "font-semibold text-white"
                      : "text-white/70 hover:text-white"
                    : active
                      ? "font-semibold text-brand"
                      : "text-content-secondary hover:bg-surface-muted hover:text-brand"
                )}
              >
                {link.label}
                {active ? (
                  <span className="absolute inset-x-3 bottom-0 h-0.5 rounded-full bg-brand" />
                ) : null}
              </Link>
            );
          })}
        </nav>

        <div className="flex items-center gap-3">
          <Link
            href="/recipes/search"
            className={cn(
              "hidden items-center gap-2 rounded-full border px-3 py-1.5 text-xs transition-colors sm:flex",
              isHome
                ? "border-white/25 bg-white/10 text-white/80 hover:bg-white/20 hover:text-white"
                : "border-stone-300 bg-surface-cream text-content-secondary hover:border-brand-border hover:text-brand"
            )}
          >
            <Search className="h-3.5 w-3.5" />
            <span>Tìm món ăn...</span>
          </Link>

          {currentUser ? (
            <div className="flex items-center gap-3">
              <Link href="/dashboard/recipes/new">
                <Button
                  variant="primary"
                  size="sm"
                  leftIcon={<Plus className="h-4 w-4" />}
                  className="hidden rounded-full sm:inline-flex"
                >
                  Đăng công thức
                </Button>
              </Link>

              <div className="relative">
                <button
                  onClick={() => setUserDropdownOpen(!userDropdownOpen)}
                  aria-label="Mở menu tài khoản"
                  aria-expanded={userDropdownOpen}
                  className={cn(
                    "flex items-center gap-2 rounded-full border p-1 transition-colors focus:outline-none",
                    isHome ? "border-white/30 hover:border-white" : "border-zinc-200 hover:border-brand"
                  )}
                >
                  <div className="flex h-8 w-8 items-center justify-center overflow-hidden rounded-full bg-brand text-xs font-bold uppercase text-white">
                    <AvatarImage src={currentAvatarUrl} alt="" className="h-full w-full object-cover" />
                  </div>
                </button>

                {userDropdownOpen && (
                  <div
                    className="absolute right-0 z-50 mt-2 w-56 animate-in fade-in slide-in-from-top-1 rounded-2xl border border-stone-200 bg-surface py-1.5 text-content-primary shadow-dropdown duration-150"
                    onMouseLeave={() => setUserDropdownOpen(false)}
                  >
                    <div className="border-b border-zinc-100 px-4 py-2.5">
                      <p className="truncate text-xs font-semibold text-content-primary">
                        {currentUser.name}
                      </p>
                      <p className="truncate text-[11px] text-content-tertiary">
                        {currentUser.email}
                      </p>
                    </div>

                    <div className="py-1">
                      {isAdmin && (
                        <Link href="/dashboard/categories" onClick={() => setUserDropdownOpen(false)} className="block px-4 py-2 text-xs text-content-secondary hover:bg-zinc-50">
                          Quản lý danh mục
                        </Link>
                      )}
                      <Link
                        href="/dashboard/profile"
                        onClick={() => setUserDropdownOpen(false)}
                        className="flex items-center gap-2.5 px-4 py-2 text-xs text-content-secondary transition-colors hover:bg-zinc-50 hover:text-content-primary"
                      >
                        <User className="h-4 w-4 text-content-tertiary" />
                        Trang cá nhân
                      </Link>
                      <Link
                        href="/dashboard/recipes"
                        onClick={() => setUserDropdownOpen(false)}
                        className="flex items-center gap-2.5 px-4 py-2 text-xs text-content-secondary transition-colors hover:bg-zinc-50 hover:text-content-primary"
                      >
                        <ChefHat className="h-4 w-4 text-content-tertiary" />
                        Công thức của tôi
                      </Link>
                      <Link
                        href="/saved"
                        onClick={() => setUserDropdownOpen(false)}
                        className="flex items-center gap-2.5 px-4 py-2 text-xs text-content-secondary transition-colors hover:bg-zinc-50 hover:text-content-primary"
                      >
                        <Bookmark className="h-4 w-4 text-content-tertiary" />
                        Công thức đã lưu
                      </Link>
                    </div>

                    <div className="border-t border-zinc-100 pt-1">
                      <button
                        onClick={() => void handleLogout()}
                        className="flex w-full items-center gap-2.5 px-4 py-2 text-left text-xs text-rose-600 transition-colors hover:bg-rose-50"
                      >
                        <LogOut className="h-4 w-4" />
                        Đăng xuất
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="hidden items-center gap-2 sm:flex">
              <Link href="/auth/login">
                <Button
                  variant="ghost"
                  size="sm"
                  className={cn(isHome && "text-white hover:bg-white/10 hover:text-white")}
                >
                  Đăng nhập
                </Button>
              </Link>
              <Link href="/auth/register">
                <Button variant="primary" size="sm" className="rounded-full">
                  Đăng ký
                </Button>
              </Link>
            </div>
          )}

          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className={cn(
              "rounded-xl p-2 transition-colors lg:hidden",
              isHome
                ? "text-white hover:bg-white/10"
                : "text-content-secondary hover:bg-zinc-100 hover:text-content-primary"
            )}
            aria-label={mobileMenuOpen ? "Đóng menu" : "Mở menu"}
            aria-expanded={mobileMenuOpen}
          >
            {mobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
        </div>
      </div>

      {mobileMenuOpen && (
        <div
          className={cn(
            "animate-in slide-in-from-top-2 space-y-3 border-t px-4 pb-6 pt-3 duration-150 lg:hidden",
            isHome ? "border-white/10 bg-[#20271f]/95 text-white" : "border-stone-200 bg-surface"
          )}
        >
          <div className="mb-3">
            <Link
              href="/recipes/search"
              onClick={() => setMobileMenuOpen(false)}
              className={cn(
                "flex items-center gap-2 rounded-lg border px-3 py-2 text-xs",
                isHome
                  ? "border-white/20 bg-white/10 text-white/80"
                  : "border-zinc-200 bg-white text-content-secondary"
              )}
            >
              <Search className="h-4 w-4 opacity-70" />
              <span>Tìm kiếm công thức nấu ăn...</span>
            </Link>
          </div>

          <div className="space-y-1">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                onClick={() => setMobileMenuOpen(false)}
                aria-current={isActive(link.href) ? "page" : undefined}
                className={cn(
                  "block rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                  isActive(link.href)
                    ? isHome
                      ? "bg-white/10 font-semibold text-white"
                      : "bg-brand-light font-semibold text-brand-hover"
                    : isHome
                      ? "text-white/75 hover:bg-white/10 hover:text-white"
                      : "text-content-secondary hover:bg-zinc-50 hover:text-content-primary"
                )}
              >
                {link.label}
              </Link>
            ))}
          </div>

          {currentUser ? (
            <div className="space-y-2 border-t border-white/10 pt-3">
              <Link
                href="/dashboard/recipes/new"
                onClick={() => setMobileMenuOpen(false)}
              >
                <Button variant="primary" size="md" className="w-full justify-center rounded-full" leftIcon={<Plus className="h-4 w-4" />}>
                  Đăng công thức mới
                </Button>
              </Link>
            </div>
          ) : (
            <div className="flex gap-2 border-t border-white/10 pt-3">
              <Link href="/auth/login" onClick={() => setMobileMenuOpen(false)}>
                <Button variant="outline" size="sm">Đăng nhập</Button>
              </Link>
              <Link href="/auth/register" onClick={() => setMobileMenuOpen(false)}>
                <Button variant="primary" size="sm" className="rounded-full">Đăng ký</Button>
              </Link>
            </div>
          )}
        </div>
      )}
    </header>
  );
};
