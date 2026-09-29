import type { Metadata } from "next";
import { Inter } from "next/font/google";
import { Providers } from "@/providers/Providers";
import "./globals.css";

const inter = Inter({
  subsets: ["latin", "vietnamese"],
  variable: "--font-inter",
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
    <html lang="vi" className={inter.variable}>
      <body className="bg-white text-content-primary antialiased selection:bg-brand-light selection:text-brand-hover min-h-screen flex flex-col font-sans">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
