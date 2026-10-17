import Link from "next/link";
import { LoginForm } from "@/components/forms/LoginForm";

export default function LoginPage() {
  return (
    <main className="mx-auto flex min-h-[65vh] w-full max-w-md flex-col justify-center px-4 py-12">
      <Link href="/" className="mb-8 text-center text-2xl font-extrabold">
        Culinary<span className="text-brand">Blog</span>
      </Link>
      <section className="space-y-6 rounded-xl border border-slate-200 bg-white p-6 sm:p-8">
        <header className="space-y-2 text-center">
          <h1 className="text-2xl font-bold">Đăng nhập</h1>
          <p className="text-sm text-content-secondary">
            Đăng nhập để quản lý và chia sẻ công thức của bạn.
          </p>
        </header>
        <LoginForm />
      </section>
    </main>
  );
}
