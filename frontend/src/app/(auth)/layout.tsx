import { Suspense } from "react";

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return <Suspense fallback={<p>Đang tải…</p>}>{children}</Suspense>;
}
