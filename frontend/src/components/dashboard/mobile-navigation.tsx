"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { cn } from "@/lib/utils";

export function DashboardMobileNavigation() {
  const pathname = usePathname();
  const { user } = useAuth();
  const items = [
    { href: "/dashboard", label: "Tổng quan" },
    { href: "/dashboard/recipes", label: "Công thức" },
    { href: "/dashboard/recipes/new", label: "Viết bài" },
    ...(user?.role === "Admin"
      ? [{ href: "/dashboard/categories", label: "Danh mục" }]
      : []),
    { href: "/dashboard/profile", label: "Hồ sơ" },
  ];
  return (
    <nav
      aria-label="Điều hướng dashboard"
      className="flex gap-2 overflow-x-auto border-b border-slate-200 bg-white px-4 py-3 md:hidden"
    >
      {items.map((item) => (
        <Link
          key={item.href}
          href={item.href}
          aria-current={pathname === item.href ? "page" : undefined}
          className={cn(
            "shrink-0 rounded-lg px-3 py-2 text-xs font-medium",
            pathname === item.href
              ? "bg-orange-100 text-orange-800"
              : "text-slate-500 hover:bg-slate-50",
          )}
        >
          {item.label}
        </Link>
      ))}
    </nav>
  );
}
