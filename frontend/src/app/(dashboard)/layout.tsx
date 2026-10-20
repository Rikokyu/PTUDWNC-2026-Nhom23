import React from "react";
import { Navbar } from "@/components/layout/Navbar";
import { Sidebar } from "@/components/layout/Sidebar";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { DashboardMobileNavigation } from "@/components/dashboard/mobile-navigation";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RequireAuth>
      <div className="min-h-screen flex flex-col bg-slate-50/50">
        <Navbar />
        <DashboardMobileNavigation />
        <div className="flex-1 max-w-[1280px] w-full mx-auto flex min-w-0">
          <Sidebar />
          <main className="flex-1 p-4 sm:p-6 lg:p-8 min-w-0">{children}</main>
        </div>
      </div>
    </RequireAuth>
  );
}
