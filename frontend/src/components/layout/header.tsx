"use client";

import Link from "next/link";
import { useState } from "react";
import { useUIStore } from "@/store/useUIStore";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ErrorMessage } from "@/components/shared/error-message";

export function Header() {
  const { sidebarOpen, toggleSidebar } = useUIStore();
  const { user, logout, status } = useAuth();
  const isLoading = status === "loading";
  const [logoutOpen, setLogoutOpen] = useState(false);
  const [logoutError, setLogoutError] = useState<string | null>(null);

  const confirmLogout = async () => {
    setLogoutError(null);
    try {
      await logout();
      setLogoutOpen(false);
    } catch (error) {
      setLogoutError(error instanceof Error ? error.message : "Không thể đăng xuất.");
    }
  };

  return (
    <>
      <header className="site-header">
        <div className="container nav-shell">
          <Link href="/" className="brand">
            <span aria-hidden="true">🍳 </span>
            Culinary Blog
          </Link>

          <nav className={`nav${sidebarOpen ? " nav-open" : ""}`} aria-label="Main navigation">
            <Link href="/">Trang chủ</Link>
            <Link href="/recipes">Công thức</Link>
            <Link href="/recipes/search">Tìm kiếm</Link>
            <Link href="/categories">Danh mục</Link>
          </nav>

          <form action="/recipes/search" className="hidden lg:flex" role="search">
            <input name="q" aria-label="Tìm công thức" placeholder="Tìm công thức…" minLength={2} maxLength={200} required className="rounded-lg border p-2" />
            <button type="submit" className="button">Tìm</button>
          </form>
          <div className="nav-actions">
            {user ? (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" aria-label={`Tài khoản ${user.displayName}`}>
                    <span className="avatar">
                      {user.displayName.charAt(0).toLocaleUpperCase()}
                    </span>
                    <span>{user.displayName}</span>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuLabel>{user.email}</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem asChild>
                    <Link href="/profile">Hồ sơ cá nhân</Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    variant="destructive"
                    onSelect={(event) => {
                      event.preventDefault();
                      setLogoutOpen(true);
                    }}
                  >
                    Đăng xuất
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <>
                <Button asChild variant="ghost" className="button-ghost">
                  <Link href="/login">Đăng nhập</Link>
                </Button>
                <Button asChild className="button-primary">
                  <Link href="/register">Đăng ký</Link>
                </Button>
              </>
            )}
            <Button
              type="button"
              variant="ghost"
              className="button-ghost mobile-toggle"
              aria-label="Toggle navigation"
              aria-expanded={sidebarOpen}
              onClick={toggleSidebar}
            >
              {sidebarOpen ? "Đóng" : "Menu"}
            </Button>
          </div>
        </div>
      </header>
      <Dialog open={logoutOpen} onOpenChange={setLogoutOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Đăng xuất?</DialogTitle>
            <DialogDescription>
              Bạn sẽ cần đăng nhập lại để sử dụng tài khoản.
            </DialogDescription>
          </DialogHeader>
          {logoutError ? (
            <ErrorMessage message={logoutError} onRetry={() => void confirmLogout()} />
          ) : null}
          <DialogFooter>
            <Button variant="outline" onClick={() => setLogoutOpen(false)} disabled={isLoading}>
              Hủy
            </Button>
            <Button variant="destructive" onClick={() => void confirmLogout()} disabled={isLoading}>
              {isLoading ? "Đang đăng xuất..." : "Đăng xuất"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
