import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";

export default function NotFound() {
  return (
    <MainLayout>
      <div className="panel center panel-danger">
        <h1>404</h1>
        <p>Trang bạn tìm kiếm không tồn tại.</p>
        <Link href="/" className="button button-primary">
          Về trang chủ
        </Link>
      </div>
    </MainLayout>
  );
}
