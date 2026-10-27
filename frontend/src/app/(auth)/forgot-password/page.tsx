"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { authApi } from "@/lib/api/auth";
import { getApiErrorMessage } from "@/lib/api/error";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [submitted, setSubmitted] = useState(false);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setLoading(true);
    try {
      await authApi.forgotPassword(email.trim());
      setSubmitted(true);
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="mx-auto max-w-md space-y-5 py-12">
      <header className="space-y-2 text-center">
        <h1 className="text-2xl font-bold">Quên mật khẩu</h1>
        <p className="text-sm text-content-secondary">
          Nhập email đăng ký để nhận liên kết đặt lại mật khẩu.
        </p>
      </header>
      {error && <ErrorState title="Không thể gửi yêu cầu" message={error} />}
      {submitted ? (
        <p role="status" className="rounded-lg border border-emerald-300 bg-emerald-50 p-4 text-sm text-emerald-900">
          Nếu email tồn tại, hướng dẫn đặt lại mật khẩu sẽ được gửi. Kiểm tra cả thư mục spam.
        </p>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4 rounded-xl border border-slate-200 bg-white p-6">
          <Input
            label="Địa chỉ email"
            type="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
          />
          <Button type="submit" className="w-full" isLoading={loading}>Gửi liên kết đặt lại</Button>
        </form>
      )}
      <p className="text-center text-sm">
        <Link href="/auth/login" className="font-semibold text-brand">Quay lại đăng nhập</Link>
      </p>
    </main>
  );
}
