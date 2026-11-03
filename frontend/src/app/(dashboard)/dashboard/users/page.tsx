"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiRequest } from "@/lib/api/client";
import { useAuthStore } from "@/store/useAuthStore";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Pagination } from "@/components/shared/pagination";

type DirectoryUser = { id: string; displayName: string; avatarUrl: string | null; role: string; email: string | null };
type DirectoryPage = { items: DirectoryUser[]; totalPages: number; totalCount: number };

export default function UsersPage() {
  const user = useAuthStore(state => state.user);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [role, setRole] = useState("");
  const query = useQuery({
    queryKey: ["users", page, search, role, user?.id, user?.role],
    enabled: !!user && ["Admin", "Author"].includes(user.role),
    queryFn: () => apiRequest<DirectoryPage>({ url: "/api/v1/users", params: { page, pageSize: 12, search: search || undefined, role: role || undefined } }),
  });
  return <section className="mx-auto max-w-5xl space-y-6 p-4 sm:p-8">
    <h1 className="text-2xl font-semibold">{user?.role === "Admin" ? "Người dùng" : "Tác giả"}</h1>
    <div className="flex flex-wrap gap-3">
      <Input aria-label="Tìm theo tên" placeholder="Tìm theo tên" maxLength={100} value={search} onChange={event => { setSearch(event.target.value); setPage(1); }} />
      {user?.role === "Admin" && <select aria-label="Vai trò" value={role} onChange={event => { setRole(event.target.value); setPage(1); }} className="rounded border p-2">
        <option value="">Mọi vai trò</option><option>Author</option><option>Admin</option>
      </select>}
    </div>
    {query.isPending && <p role="status">Đang tải…</p>}
    {query.isError && <div role="alert">Không tải được danh sách. <Button onClick={() => void query.refetch()}>Thử lại</Button></div>}
    {query.data && <>
      <p>{query.data.totalCount} kết quả</p>
      {query.data.items.length === 0 && <p>Không có kết quả phù hợp.</p>}
      <div className="grid gap-4 sm:grid-cols-2">
        {query.data.items.map(item => <article key={item.id} className="rounded-xl border bg-white p-4">
          <div className="flex items-center gap-3">
            {item.avatarUrl ? <img src={item.avatarUrl} alt="" className="h-10 w-10 rounded-full object-cover" /> : <span aria-hidden className="rounded-full bg-slate-100 p-3">{item.displayName.slice(0, 1)}</span>}
            <div><h2 className="font-semibold">{item.displayName}</h2><p>{item.role}</p></div>
          </div>
          {user?.role === "Admin" && item.email && <p className="mt-2 break-all text-sm">{item.email}</p>}
        </article>)}
      </div>
      <Pagination currentPage={page} totalPages={query.data.totalPages} onPageChange={setPage} />
    </>}
  </section>;
}
