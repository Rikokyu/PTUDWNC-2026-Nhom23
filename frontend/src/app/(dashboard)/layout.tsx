import React from "react";
import { Navbar } from "@/components/layout/Navbar";
import { Sidebar } from "@/components/layout/Sidebar";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const dummyUser = {
    name: "Minh Thư",
    email: "minhthu@culinary.vn",
  };

  return (
    <div className="min-h-screen flex flex-col bg-slate-50/50">
      <Navbar user={dummyUser} />
      <div className="flex-1 max-w-[1280px] w-full mx-auto flex">
        <Sidebar />
        <main className="flex-1 p-6 lg:p-8 min-w-0">
          {children}
        </main>
      </div>
    </div>
  );
}
