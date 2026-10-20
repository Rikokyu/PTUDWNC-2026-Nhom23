import { Footer } from "@/components/layout/footer";
import { Header } from "@/components/layout/header";

export function MainLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="page-shell">
      <Header />
      <main className="container page-content">{children}</main>
      <Footer />
    </div>
  );
}
