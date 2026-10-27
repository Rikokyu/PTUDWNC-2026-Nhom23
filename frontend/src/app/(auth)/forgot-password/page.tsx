"use client";

import Link from "next/link";
import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ArrowLeft, Mail } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { getApiErrorMessage } from "@/lib/api/error";
import { authApi } from "@/lib/api/auth";

const schema = z.object({ email: z.string().trim().email("Vui lòng nhập email hợp lệ.").max(320) });
type FormValues = z.infer<typeof schema>;

export default function ForgotPasswordPage() {
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { email: "" },
  });

  async function submit(values: FormValues) {
    setError(null);
    try {
      await authApi.forgotPassword(values.email);
      setSent(true);
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    }
  }

  return (
    <main className="mx-auto max-w-md px-5 py-14">
      <div className="mb-8 flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-50 text-emerald-800">
        <Mail className="h-6 w-6" aria-hidden="true" />
      </div>
      <h1 className="text-2xl font-bold text-content-primary">Quên mật khẩu?</h1>
      <p className="mt-2 text-sm leading-6 text-content-secondary">
        Nhập email tài khoản để nhận liên kết đặt lại mật khẩu.
      </p>
      {sent ? (
        <div className="mt-6 space-y-5">
          <p role="status" className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900">
            Nếu email tồn tại trong hệ thống, hướng dẫn đặt lại mật khẩu sẽ được gửi đến hộp thư của bạn.
          </p>
          <Link href="/auth/login" className="inline-flex items-center gap-2 text-sm font-semibold text-brand hover:underline">
            <ArrowLeft className="h-4 w-4" aria-hidden="true" /> Trở về đăng nhập
          </Link>
        </div>
      ) : (
        <form className="mt-6 space-y-5" onSubmit={form.handleSubmit(submit)}>
          {error && <p role="alert" className="text-sm text-rose-700">{error}</p>}
          <Input
            label="Địa chỉ email"
            type="email"
            autoComplete="email"
            placeholder="ten@example.com"
            leftIcon={<Mail className="h-4 w-4" />}
            error={form.formState.errors.email?.message}
            {...form.register("email")}
          />
          <Button type="submit" isLoading={form.formState.isSubmitting} className="w-full">
            Gửi hướng dẫn
          </Button>
          <Link href="/auth/login" className="flex items-center justify-center gap-2 text-sm font-medium text-content-secondary hover:text-content-primary">
            <ArrowLeft className="h-4 w-4" aria-hidden="true" /> Trở về đăng nhập
          </Link>
        </form>
      )}
    </main>
  );
}