"use client";

import { Suspense, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { KeyRound } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { getApiErrorMessage } from "@/lib/api/error";
import { authApi } from "@/lib/api/auth";

const schema = z.object({
  email: z.string().trim().email("Vui lòng nhập email hợp lệ.").max(320),
  token: z.string().min(1, "Liên kết đặt lại mật khẩu không hợp lệ."),
  newPassword: z.string()
    .min(8, "Mật khẩu cần ít nhất 8 ký tự.")
    .max(128, "Mật khẩu tối đa 128 ký tự.")
    .regex(/[a-z]/, "Cần có ít nhất một chữ thường.")
    .regex(/[A-Z]/, "Cần có ít nhất một chữ hoa.")
    .regex(/[0-9]/, "Cần có ít nhất một chữ số.")
    .regex(/[^a-zA-Z0-9]/, "Cần có ít nhất một ký tự đặc biệt."),
  confirmPassword: z.string(),
}).refine((values) => values.newPassword === values.confirmPassword, {
  path: ["confirmPassword"],
  message: "Mật khẩu xác nhận không khớp.",
});
type FormValues = z.infer<typeof schema>;

function ResetPasswordForm() {
  const params = useSearchParams();
  const [complete, setComplete] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      email: params.get("email") ?? "",
      token: params.get("token") ?? "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  async function submit(values: FormValues) {
    setError(null);
    try {
      await authApi.resetPassword(values.email, values.token, values.newPassword);
      setComplete(true);
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    }
  }

  return (
    <main className="mx-auto max-w-md px-5 py-14">
      <div className="mb-8 flex h-12 w-12 items-center justify-center rounded-xl bg-amber-50 text-amber-900">
        <KeyRound className="h-6 w-6" aria-hidden="true" />
      </div>
      <h1 className="text-2xl font-bold text-content-primary">Đặt lại mật khẩu</h1>
      {complete ? (
        <div className="mt-6 space-y-5">
          <p role="status" className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900">
            Mật khẩu đã được cập nhật. Bạn có thể đăng nhập bằng mật khẩu mới.
          </p>
          <Link href="/auth/login" className="inline-flex text-sm font-semibold text-brand hover:underline">
            Đến trang đăng nhập
          </Link>
        </div>
      ) : (
        <form className="mt-6 space-y-4" onSubmit={form.handleSubmit(submit)}>
          {error && <p role="alert" className="text-sm text-rose-700">{error}</p>}
          <Input label="Địa chỉ email" type="email" autoComplete="email" error={form.formState.errors.email?.message} {...form.register("email")} />
          <Input label="Mã đặt lại" autoComplete="off" error={form.formState.errors.token?.message} {...form.register("token")} />
          <Input label="Mật khẩu mới" type="password" autoComplete="new-password" error={form.formState.errors.newPassword?.message} {...form.register("newPassword")} />
          <Input label="Xác nhận mật khẩu mới" type="password" autoComplete="new-password" error={form.formState.errors.confirmPassword?.message} {...form.register("confirmPassword")} />
          <Button type="submit" isLoading={form.formState.isSubmitting} className="w-full">
            Cập nhật mật khẩu
          </Button>
        </form>
      )}
    </main>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<main className="mx-auto max-w-md px-5 py-14">Đang tải biểu mẫu...</main>}>
      <ResetPasswordForm />
    </Suspense>
  );
}