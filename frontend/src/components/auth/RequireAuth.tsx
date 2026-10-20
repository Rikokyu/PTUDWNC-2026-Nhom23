"use client";

import { useEffect } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "./AuthProvider";

export function RequireAuth({ children }: { children: React.ReactNode }) {
  const { user, status, retry } = useAuth();
  const pathname = usePathname();
  const router = useRouter();
  const adminOnly = pathname === "/dashboard/categories" || pathname.startsWith("/dashboard/categories/");

  useEffect(() => {
    if (status === "guest") {
      const next = window.location.pathname + window.location.search;
      router.replace(`/auth/login?next=${encodeURIComponent(next)}`);
    }
  }, [status, pathname, router]);

  if (status === "loading" || status === "guest") {
    return <p role="status" className="p-8 text-center">Đang kiểm tra đăng nhập…</p>;
  }
  if (status === "error") {
    return (
      <div role="alert" className="p-8 text-center space-y-4">
        <p>Không thể xác minh tài khoản. Vui lòng kiểm tra kết nối và thử lại.</p>
        <button onClick={retry} className="text-brand underline">Thử lại</button>
      </div>
    );
  }
  if (!user || !["Author", "Admin"].includes(user.role) || (adminOnly && user.role !== "Admin")) {
    return (
      <div role="alert" className="p-8 text-center space-y-4">
        <h1 className="text-xl font-bold">Bạn không có quyền truy cập trang này</h1>
        <p>Trang quản lý danh mục chỉ dành cho quản trị viên.</p>
        <Link className="text-brand underline" href="/dashboard">Về trang cá nhân</Link>
      </div>
    );
  }
  return children;
}
