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
    <div className="min-h-screen flex flex-col bg-slate-50/50">
      <Navbar />
      <div className="flex-1 max-w-[1280px] w-full mx-auto flex">
        <Sidebar />
        <main className="flex-1 p-6 lg:p-8 min-w-0">
          {children}
        </main>
      </div>
    </div>
    </RequireAuth>
  );
}
