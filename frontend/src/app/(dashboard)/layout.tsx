export default function DashboardLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <section>
      <nav>Dashboard</nav>
      {children}
    </section>
  );
}
