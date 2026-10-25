"use client";

import { FormEvent, Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { authApi } from "@/lib/api/auth";
import { getApiErrorMessage } from "@/lib/api/error";

function ResetPasswordForm() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [token] = useState(() => searchParams.get("token") ?? "");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [completed, setCompleted] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (token) router.replace("/reset-password");
  }, [router, token]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    if (!token) {
      setError("Liên kết đặt lại mật khẩu thiếu token.");
      return;
    }
    if (password !== confirmPassword) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }
    setLoading(true);
    try {
      await authApi.resetPassword(token, password);
      setCompleted(true);
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="mx-auto max-w-md space-y-5 py-12">
      <header className="space-y-2 text-center">
        <h1 className="text-2xl font-bold">Đặt lại mật khẩu</h1>
        <p className="text-sm text-content-secondary">
          Liên kết có hiệu lực trong 60 phút. Chọn mật khẩu mới an toàn.
        </p>
      </header>
      {error && <ErrorState title="Không thể đặt lại mật khẩu" message={error} />}
      {completed ? (
        <div className="space-y-4 text-center">
          <p role="status" className="text-sm text-emerald-700">Đã cập nhật mật khẩu. Hãy đăng nhập lại.</p>
          <Link href="/auth/login" className="font-semibold text-brand">Đăng nhập</Link>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4 rounded-xl border border-slate-200 bg-white p-6">
          <Input
            label="Mật khẩu mới"
            type="password"
            autoComplete="new-password"
            minLength={8}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
          />
          <Input
            label="Xác nhận mật khẩu mới"
            type="password"
            autoComplete="new-password"
            minLength={8}
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            required
          />
          <Button type="submit" className="w-full" isLoading={loading}>Cập nhật mật khẩu</Button>
        </form>
      )}
    </main>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<p className="py-12 text-center text-sm">Đang tải...</p>}>
      <ResetPasswordForm />
    </Suspense>
  );
}
