import React from "react";
import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";

export default function PublicLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen flex flex-col bg-surface-muted">
      <Navbar />
      <main id="main-content" className="mx-auto w-full max-w-[1280px] flex-1 px-4 py-8 sm:px-6 sm:py-12 lg:px-8">
        {children}
      </main>
      <Footer />
    </div>
  );
}
