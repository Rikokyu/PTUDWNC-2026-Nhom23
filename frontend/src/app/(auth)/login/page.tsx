"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { AuthLayout } from "@/components/layout/auth-layout";
import { ErrorMessage } from "@/components/shared/error-message";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAuthStore } from "@/store/auth-store";

export default function LoginPage() {
  const router = useRouter();
  const login = useAuthStore((state) => state.login);
  const isLoading = useAuthStore((state) => state.isLoading);
  const [error, setError] = useState<string | null>(null);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    const formData = new FormData(event.currentTarget);
    try {
      await login(
        String(formData.get("email") ?? ""),
        String(formData.get("password") ?? ""),
      );
      router.replace("/profile");
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Không thể đăng nhập.");
    }
  };

  return (
    <AuthLayout>
      <h1>Đăng nhập</h1>
      <form className="form-shell" onSubmit={(event) => void submit(event)}>
        <label>
          Email
          <Input name="email" type="email" autoComplete="email" required />
        </label>
        <label>
          Mật khẩu
          <Input name="password" type="password" autoComplete="current-password" required />
        </label>
        {error ? <ErrorMessage message={error} /> : null}
        <Button type="submit" className="button-primary full-width" disabled={isLoading}>
          {isLoading ? "Đang xác thực..." : "Đăng nhập"}
        </Button>
      </form>
      <p className="muted text-center">
        Chưa có tài khoản? <Link href="/register">Đăng ký</Link>
      </p>
    </AuthLayout>
  );
}
