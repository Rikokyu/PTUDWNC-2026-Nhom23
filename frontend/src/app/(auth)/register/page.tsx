"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { AuthLayout } from "@/components/layout/auth-layout";
import { ErrorMessage } from "@/components/shared/error-message";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAuthStore } from "@/store/auth-store";

export default function RegisterPage() {
  const router = useRouter();
  const register = useAuthStore((state) => state.register);
  const isLoading = useAuthStore((state) => state.isLoading);
  const [error, setError] = useState<string | null>(null);
  const [registered, setRegistered] = useState(false);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    const formData = new FormData(event.currentTarget);
    const password = String(formData.get("password") ?? "");
    if (password !== String(formData.get("confirmPassword") ?? "")) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }

    try {
      await register(
        String(formData.get("email") ?? ""),
        String(formData.get("displayName") ?? ""),
        password,
      );
      event.currentTarget.reset();
      setRegistered(true);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Không thể đăng ký.");
    }
  };

  return (
    <AuthLayout>
      <h1>Đăng ký</h1>
      {registered ? (
        <Alert className="auth-feedback">
          <div>
            <AlertTitle>Đã tạo tài khoản</AlertTitle>
            <AlertDescription>
              Đăng ký thành công. Hãy đăng nhập bằng thông tin vừa đăng ký.
            </AlertDescription>
          </div>
          <Button onClick={() => router.replace("/login")}>Đến đăng nhập</Button>
        </Alert>
      ) : (
        <form className="form-shell" onSubmit={(event) => void submit(event)}>
          <label>
            Họ tên
            <Input name="displayName" autoComplete="name" minLength={2} maxLength={100} required />
          </label>
          <label>
            Email
            <Input name="email" type="email" autoComplete="email" required />
          </label>
          <label>
            Mật khẩu
            <Input
              name="password"
              type="password"
              autoComplete="new-password"
              minLength={8}
              maxLength={128}
              required
            />
          </label>
          <label>
            Xác nhận mật khẩu
            <Input
              name="confirmPassword"
              type="password"
              autoComplete="new-password"
              minLength={8}
              maxLength={128}
              required
            />
          </label>
          {error ? <ErrorMessage message={error} /> : null}
          <Button type="submit" className="button-primary full-width" disabled={isLoading}>
            {isLoading ? "Đang tạo tài khoản..." : "Tạo tài khoản"}
          </Button>
        </form>
      )}
      <p className="muted text-center">
        Đã có tài khoản? <Link href="/login">Đăng nhập</Link>
      </p>
    </AuthLayout>
  );
}
