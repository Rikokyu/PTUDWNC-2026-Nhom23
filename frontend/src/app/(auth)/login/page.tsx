import { LoginForm } from "@/components/forms/LoginForm";

export default function LoginPage() {
  return (
    <main className="mx-auto max-w-md py-12">
      <h1 className="mb-6 text-center text-2xl font-bold">Đăng nhập Culinary Blog</h1>
      <LoginForm />
    </main>
  );
}
