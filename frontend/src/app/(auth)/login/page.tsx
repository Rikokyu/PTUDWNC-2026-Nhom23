import { LoginForm } from "@/components/forms/LoginForm";
import Link from "next/link";
import { Suspense } from "react";

export default function LoginPage() {
  return (
    <main className="mx-auto max-w-md py-12">
      <h1 className="mb-6 text-center text-2xl font-bold">Đăng nhập Culinary Blog</h1>
      <Suspense fallback={<div className="h-48 animate-pulse rounded-xl bg-slate-100" />}>
        <LoginForm />
      </Suspense>
      <p className="mt-4 text-center text-sm">
        <Link href="/auth/forgot-password" className="font-medium text-brand hover:underline">
          Quên mật khẩu?
        </Link>
      </p>
    </main>
  );
}
