import { PasswordForm } from "@/components/forms/PasswordForm";
import { MainLayout } from "@/components/layout/main-layout";
export default function Page() { return <MainLayout><section className="section-block"><h1>Quên mật khẩu</h1><PasswordForm mode="forgot" /></section></MainLayout>; }
