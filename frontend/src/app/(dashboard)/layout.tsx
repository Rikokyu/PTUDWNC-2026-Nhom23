import React from "react";
import { Navbar } from "@/components/layout/Navbar";
import { Sidebar } from "@/components/layout/Sidebar";
import { RequireAuth } from "@/components/auth/RequireAuth";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RequireAuth>
    <div className="flex min-h-screen flex-col bg-surface-muted">
      <Navbar />
      <div className="mx-auto flex w-full max-w-[1440px] flex-1 flex-col md:flex-row">
        <Sidebar />
        <main id="main-content" className="min-w-0 flex-1 p-5 sm:p-7 lg:p-10">
          {children}
        </main>
      </div>
    </div>
    </RequireAuth>
  );
}
