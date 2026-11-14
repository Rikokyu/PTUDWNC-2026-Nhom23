"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Bookmark, ChefHat, FolderOpen, LayoutDashboard, LogOut, Plus, Settings2, UserRound, Users } from "lucide-react";
import { cn } from "@/lib/utils";
import { useAuth } from "@/components/auth/AuthProvider";

export function Sidebar() {
  const pathname = usePathname();
  const { user, logout } = useAuth();
  const isAdmin = user?.role === "Admin";
  const groups = [
    { title: "Không gian làm việc", items: [
      { label: "Tổng quan", href: "/dashboard", icon: LayoutDashboard },
      { label: "Công thức của tôi", href: "/dashboard/recipes", icon: ChefHat },
      { label: "Tạo công thức", href: "/dashboard/recipes/new", icon: Plus },
      { label: "Đã lưu", href: "/saved", icon: Bookmark },
    ] },
    ...(isAdmin ? [{ title: "Quản trị", items: [
      { label: "Tất cả công thức", href: "/dashboard/admin-recipes", icon: FolderOpen },
      { label: "Danh mục", href: "/dashboard/categories", icon: Settings2 },
      { label: "Tài khoản", href: "/dashboard/users", icon: Users },
    ] }] : []),
    { title: "Tài khoản", items: [
      { label: "Hồ sơ cá nhân", href: "/dashboard/profile", icon: UserRound },
    ] },
  ];
  const isActive = (href: string) => pathname === href || (
    href === "/dashboard/recipes" && pathname.startsWith("/dashboard/recipes/") && pathname !== "/dashboard/recipes/new"
  );

  return <>
    <nav aria-label="Điều hướng quản lý trên điện thoại" className="flex gap-2 overflow-x-auto border-b border-stone-200 bg-surface px-4 py-3 md:hidden">
      {groups.flatMap((group) => group.items).map((item) => <Link key={item.href} href={item.href} aria-current={isActive(item.href) ? "page" : undefined} className={cn(
        "shrink-0 rounded-full border px-3 py-2 text-xs font-semibold transition-colors",
        isActive(item.href) ? "border-accent/30 bg-accent-light text-accent-hover" : "border-stone-200 text-content-secondary hover:border-accent/30"
      )}>{item.label}</Link>)}
    </nav>
    <aside className="hidden min-h-[calc(100vh-4.5rem)] w-64 shrink-0 flex-col border-r border-stone-200 bg-surface px-4 py-7 md:flex">
      <div className="mb-8 px-3">
        <p className="text-[11px] font-bold uppercase tracking-[0.2em] text-brand">Culinary Blog</p>
        <p className="mt-2 font-display text-xl font-bold text-content-primary">Góc làm việc</p>
        <p className="mt-1 text-xs text-content-tertiary">{isAdmin ? "Quản trị viên" : "Tác giả"}</p>
      </div>
      <div className="flex-1 space-y-7">
        {groups.map((group) => <div key={group.title}>
          <p className="mb-2 px-3 text-[10px] font-bold uppercase tracking-[0.16em] text-content-tertiary">{group.title}</p>
          <nav aria-label={group.title} className="space-y-1">
            {group.items.map((item) => { const Icon = item.icon; return <Link key={item.href} href={item.href} aria-current={isActive(item.href) ? "page" : undefined} className={cn(
              "flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors",
              isActive(item.href) ? "bg-accent-light font-semibold text-accent-hover" : "text-content-secondary hover:bg-surface-muted hover:text-content-primary"
            )}><Icon className="h-4 w-4 shrink-0" aria-hidden="true" />{item.label}</Link>; })}
          </nav>
        </div>)}
      </div>
      <div className="border-t border-stone-200 pt-4">
        <p className="truncate px-3 pb-2 text-xs font-medium text-content-secondary">{user?.displayName}</p>
        <button onClick={() => void logout()} className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm font-medium text-rose-700 hover:bg-rose-50"><LogOut className="h-4 w-4" />Đăng xuất</button>
      </div>
    </aside>
  </>;
}
