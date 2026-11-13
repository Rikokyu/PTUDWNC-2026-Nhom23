import type { Metadata } from "next";
import { Be_Vietnam_Pro, Manrope } from "next/font/google";
import { Providers } from "@/providers/Providers";
import "./globals.css";

const sans = Manrope({
  subsets: ["latin", "vietnamese"],
  variable: "--font-sans",
  display: "swap",
});

const display = Be_Vietnam_Pro({
  subsets: ["latin", "vietnamese"],
  weight: ["500", "600", "700", "800"],
  variable: "--font-display",
  display: "swap",
});

export const metadata: Metadata = {
  title: { default: "Culinary Blog | Nền Tảng Nấu Ăn & Chia Sẻ Công Thức", template: "%s | Culinary Blog" },
  description: "Khám phá hàng ngàn công thức nấu ăn ngon, chuẩn vị và kết nối cùng cộng đồng yêu ẩm thực.",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi" className={`${sans.variable} ${display.variable}`} suppressHydrationWarning>
      <body className="flex min-h-screen flex-col bg-surface-muted font-sans text-content-primary antialiased selection:bg-brand-light selection:text-brand-hover">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
