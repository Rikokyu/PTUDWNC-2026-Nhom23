"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { Bookmark, ChefHat, Grid, LogOut, PlusCircle, Settings, User, Users } from "lucide-react";
import { cn } from "@/lib/utils";
import { useAuth } from "@/components/auth/AuthProvider";

export const Sidebar: React.FC = () => {
  const pathname = usePathname();
  const { user, logout } = useAuth();

  const menuItems = [
    { label: "Tổng quan", href: "/dashboard", icon: Grid },
    { label: "Công thức của tôi", href: "/dashboard/recipes", icon: ChefHat },
    { label: "Tạo công thức mới", href: "/dashboard/recipes/new", icon: PlusCircle },
    ...(user?.role === "Admin"
      ? [
          { label: "Quản lý danh mục", href: "/dashboard/categories", icon: Grid },
          { label: "Quản lý công thức", href: "/dashboard/admin-recipes", icon: ChefHat },
          { label: "Quản lý tài khoản", href: "/dashboard/users", icon: Users },
        ]
      : []),
    { label: "Hồ sơ cá nhân", href: "/dashboard/profile", icon: User },
    { label: "Công thức đã lưu", href: "/saved", icon: Bookmark },
  ];

  return (
    <aside className="hidden min-h-[calc(100vh-4rem)] w-64 shrink-0 flex-col justify-between border-r border-slate-200 bg-white p-4 md:flex">
      <div className="space-y-6">
        <div>
          <p className="mb-2 px-3 text-[11px] font-semibold uppercase tracking-wider text-content-tertiary">
            Quản trị cá nhân
          </p>
          <nav className="space-y-1">
            {menuItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2.5 text-xs font-medium transition-colors",
                    isActive
                      ? "bg-brand-light font-semibold text-brand-hover"
                      : "text-content-secondary hover:bg-slate-50 hover:text-content-primary",
                  )}
                >
                  <Icon className={cn("h-4 w-4", isActive ? "text-brand" : "text-content-tertiary")} />
                  <span>{item.label}</span>
                </Link>
              );
            })}
          </nav>
        </div>
      </div>

      <div className="space-y-1 border-t border-slate-100 pt-4">
        <Link
          href="/dashboard/settings"
          className="flex items-center gap-3 rounded-lg px-3 py-2 text-xs font-medium text-content-secondary transition-colors hover:bg-slate-50 hover:text-content-primary"
        >
          <Settings className="h-4 w-4 text-content-tertiary" />
          <span>Cài đặt tài khoản</span>
        </Link>
        <button
          onClick={() => void logout()}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-xs font-medium text-rose-600 transition-colors hover:bg-rose-50"
        >
          <LogOut className="h-4 w-4 text-rose-500" />
          <span>Đăng xuất</span>
        </button>
      </div>
    </aside>
  );
};
