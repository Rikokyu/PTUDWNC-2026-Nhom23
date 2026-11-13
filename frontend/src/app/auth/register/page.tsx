import { RegisterForm } from "@/components/forms/RegisterForm";
import { AuthShell } from "@/components/layout/AuthShell";

export default function RegisterPage() {
  return (
    <AuthShell title="Tạo tài khoản" description="Gia nhập cộng đồng yêu bếp và bắt đầu viết câu chuyện ẩm thực của bạn.">
      <RegisterForm />
    </AuthShell>
  );
}
