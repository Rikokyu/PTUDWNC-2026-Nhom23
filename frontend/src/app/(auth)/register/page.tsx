import { RegisterForm } from "@/components/forms/RegisterForm";
import { googleLoginEnabled } from "@/auth";

export const dynamic = "force-dynamic";

export default function RegisterPage() {
  return (
    <main className="mx-auto max-w-md py-12">
      <h1 className="mb-6 text-center text-2xl font-bold">Tạo tài khoản Culinary Blog</h1>
      <RegisterForm googleEnabled={googleLoginEnabled} />
    </main>
  );
}
