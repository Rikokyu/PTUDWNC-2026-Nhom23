import { RegisterForm } from "@/components/forms/RegisterForm";
import { AuthShell } from "@/components/layout/AuthShell";
import { googleLoginEnabled } from "@/auth";

export const dynamic = "force-dynamic";

export default function RegisterPage() {
  return (
    <AuthShell title="Tạo tài khoản" description="Gia nhập cộng đồng yêu bếp và bắt đầu viết câu chuyện ẩm thực của bạn.">
      <RegisterForm googleEnabled={googleLoginEnabled} />
    </AuthShell>
  );
}
