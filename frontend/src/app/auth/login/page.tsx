import { Suspense } from "react";
import { LoginForm } from "@/components/forms/LoginForm";
import { AuthShell } from "@/components/layout/AuthShell";

export default function LoginPage() {
  return (
    <AuthShell title="Đăng nhập" description="Tiếp tục lưu món bạn thích và chia sẻ những công thức của riêng mình.">
      <Suspense fallback={<p className="text-sm text-content-secondary">Đang tải biểu mẫu...</p>}>
        <LoginForm />
      </Suspense>
    </AuthShell>
  );
}
