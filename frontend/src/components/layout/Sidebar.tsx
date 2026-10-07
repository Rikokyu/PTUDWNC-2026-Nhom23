"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { ChefHat, Grid, PlusCircle, User, Bookmark, Settings, LogOut } from "lucide-react";
import { cn } from "@/lib/utils";

export const Sidebar: React.FC = () => {
  const pathname = usePathname();

  const menuItems = [
    { label: "Tổng quan", href: "/dashboard", icon: Grid },
    { label: "Công thức của tôi", href: "/dashboard/recipes", icon: ChefHat },
    { label: "Tạo công thức mới", href: "/dashboard/recipes/new", icon: PlusCircle },
    { label: "Danh mục quản lý", href: "/dashboard/categories", icon: Grid },
    { label: "Hồ sơ cá nhân", href: "/dashboard/profile", icon: User },
    { label: "Công thức đã lưu", href: "/saved", icon: Bookmark },
  ];

  return (
    <aside className="w-64 bg-white border-r border-slate-200 min-h-[calc(100vh-4rem)] p-4 flex flex-col justify-between shrink-0 hidden md:flex">
      <div className="space-y-6">
        <div>
          <p className="px-3 text-[11px] font-semibold text-content-tertiary uppercase tracking-wider mb-2">
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
                    "flex items-center gap-3 px-3 py-2.5 rounded-lg text-xs font-medium transition-colors",
                    isActive
                      ? "bg-brand-light text-brand-hover font-semibold"
                      : "text-content-secondary hover:text-content-primary hover:bg-slate-50"
                  )}
                >
                  <Icon className={cn("w-4 h-4", isActive ? "text-brand" : "text-content-tertiary")} />
                  <span>{item.label}</span>
                </Link>
              );
            })}
          </nav>
        </div>
      </div>

      <div className="pt-4 border-t border-slate-100 space-y-1">
        <Link
          href="/dashboard/settings"
          className="flex items-center gap-3 px-3 py-2 rounded-lg text-xs font-medium text-content-secondary hover:text-content-primary hover:bg-slate-50 transition-colors"
        >
          <Settings className="w-4 h-4 text-content-tertiary" />
          <span>Cài đặt tài khoản</span>
        </Link>
        <button
          onClick={() => {}}
          className="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-xs font-medium text-rose-600 hover:bg-rose-50 transition-colors text-left"
        >
          <LogOut className="w-4 h-4 text-rose-500" />
          <span>Đăng xuất</span>
        </button>
      </div>
    </aside>
  );
};
