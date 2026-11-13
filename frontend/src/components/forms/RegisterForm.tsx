"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Lock, Mail, User } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { authApi, saveAuthSession } from "@/lib/api/auth";
import { getApiErrorMessage, getApiFieldErrors } from "@/lib/api/error";

export function RegisterForm() {
  const router = useRouter();
  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [registered, setRegistered] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setFieldErrors({});
    setRegistered(false);
    setIsLoading(true);
    try {
      const session = await authApi.register(displayName, email, password);
      saveAuthSession(session);
      router.push("/dashboard");
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
      setFieldErrors(getApiFieldErrors(requestError));
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && <ErrorState title="Không thể tạo tài khoản" message={error} />}
      {registered && (
        <p role="status" className="rounded-lg border border-emerald-300 bg-emerald-50 px-4 py-3 text-sm text-emerald-900">
          Tạo tài khoản thành công. Bạn có thể đăng nhập bằng thông tin vừa đăng ký.
        </p>
      )}
      <Input
        label="Họ và tên"
        autoComplete="name"
        value={displayName}
        onChange={(event) => {
          setDisplayName(event.target.value);
          if (fieldErrors.displayName || fieldErrors.DisplayName) {
            setFieldErrors((prev) => ({ ...prev, displayName: [], DisplayName: [] }));
          }
        }}
        error={fieldErrors.displayName?.[0] || fieldErrors.DisplayName?.[0]}
        leftIcon={<User className="h-4 w-4" />}
        required
      />
      <Input
        label="Địa chỉ Email"
        type="email"
        autoComplete="email"
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
        autoComplete="new-password"
        minLength={8}
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
        Tạo tài khoản
      </Button>
      <p className="text-center text-sm text-content-secondary">
        Đã có tài khoản?{" "}
        <Link href="/auth/login" className="font-semibold text-brand">
          Đăng nhập
        </Link>
      </p>
    </form>
  );
}
