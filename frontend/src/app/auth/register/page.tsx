import Link from "next/link";
import { RegisterForm } from "@/components/forms/RegisterForm";

export default function RegisterPage() {
  return (
    <main className="mx-auto flex min-h-[65vh] w-full max-w-md flex-col justify-center px-4 py-12">
      <Link href="/" className="mb-8 text-center text-2xl font-extrabold">
        Culinary<span className="text-brand">Blog</span>
      </Link>
      <section className="space-y-6 rounded-xl border border-slate-200 bg-white p-6 sm:p-8">
        <header className="space-y-2 text-center">
          <h1 className="text-2xl font-bold">Tạo tài khoản</h1>
          <p className="text-sm text-content-secondary">
            Tham gia cộng đồng chia sẻ công thức Culinary Blog.
          </p>
        </header>
        <RegisterForm />
      </section>
    </main>
  );
}
