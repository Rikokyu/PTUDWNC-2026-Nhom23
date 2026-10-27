import { PasswordForm } from "@/components/forms/PasswordForm";
import { MainLayout } from "@/components/layout/main-layout";
export default function Page() { return <MainLayout><section className="section-block"><h1>Đặt lại mật khẩu</h1><PasswordForm mode="reset" /></section></MainLayout>; }
