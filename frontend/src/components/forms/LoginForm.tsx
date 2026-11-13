"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Lock, Mail } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { authApi } from "@/lib/api/auth";
import { getApiErrorMessage, getApiFieldErrors } from "@/lib/api/error";
import { saveAuthSession } from "@/lib/api/auth";
import { safeReturnPath } from "@/lib/auth/session";

export function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setFieldErrors({});
    setIsLoading(true);
    try {
      const session = await authApi.login(email, password);
      saveAuthSession(session);
      router.replace(safeReturnPath(searchParams.get("next")));
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
      setFieldErrors(getApiFieldErrors(requestError));
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && <ErrorState title="Không thể đăng nhập" message={error} />}
      <Input
        label="Địa chỉ Email"
        type="email"
        autoComplete="email"
        placeholder="ten@example.com"
        value={email}
        onChange={(event) => {
          setEmail(event.target.value);
          if (fieldErrors.email || fieldErrors.Email) {
            setFieldErrors((prev) => ({ ...prev, email: [], Email: [] }));
          }
        }}
        error={fieldErrors.email?.[0] || fieldErrors.Email?.[0]}
        leftIcon={<Mail className="h-4 w-4" />}
        required
      />
      <Input
        label="Mật khẩu"
        type="password"
        autoComplete="current-password"
        value={password}
        onChange={(event) => {
          setPassword(event.target.value);
          if (fieldErrors.password || fieldErrors.Password) {
            setFieldErrors((prev) => ({ ...prev, password: [], Password: [] }));
          }
        }}
        error={fieldErrors.password?.[0] || fieldErrors.Password?.[0]}
        leftIcon={<Lock className="h-4 w-4" />}
        required
      />
      <Button type="submit" size="lg" className="w-full" isLoading={isLoading}>
        Đăng nhập
      </Button>
      <p className="text-center text-sm text-content-secondary">
        Chưa có tài khoản?{" "}
        <Link href="/auth/register" className="font-semibold text-brand">
          Đăng ký
        </Link>
      </p>
      <p className="text-center text-sm">
        <Link href="/forgot-password" className="text-brand hover:underline">
          Quên mật khẩu?
        </Link>
      </p>
    </form>
  );
}
