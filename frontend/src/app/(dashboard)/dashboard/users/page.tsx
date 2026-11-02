"use client";

import { useState, type FormEvent } from "react";
import { Search, ShieldCheck, UserRoundCheck, UserRoundX } from "lucide-react";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button } from "@/components/ui/Button";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Input } from "@/components/ui/Input";
import { Pagination } from "@/components/ui/Pagination";
import { Select } from "@/components/ui/Select";
import { Skeleton } from "@/components/ui/LoadingState";
import { getApiErrorMessage } from "@/lib/api/error";
import {
  useAdminUsers,
  useSetUserActive,
  useSetUserRole,
} from "@/hooks/useAdminUsers";
import type { UserRole } from "@/types/api";

export default function AdminUsersPage() {
  const { user: currentUser } = useAuth();
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [role, setRole] = useState<"" | UserRole>("");
  const usersQuery = useAdminUsers({ page, pageSize: 20, search, role });
  const statusMutation = useSetUserActive();
  const roleMutation = useSetUserRole();
  const mutationError = statusMutation.error ?? roleMutation.error;
  const result = usersQuery.data;

  const applySearch = (event: FormEvent) => {
    event.preventDefault();
    setPage(1);
    setSearch(searchInput.trim());
  };

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-extrabold text-content-primary">Quản lý tài khoản</h1>
        <p className="mt-1 text-sm text-content-secondary">
          Admin có thể xem tài khoản, khóa/mở khóa Author và thay đổi role.
        </p>
      </header>

      <form onSubmit={applySearch} className="grid gap-3 rounded-xl border border-slate-200 bg-white p-4 md:grid-cols-[1fr_180px_auto] md:items-end">
        <Input
          label="Tìm tài khoản"
          value={searchInput}
          onChange={(event) => setSearchInput(event.target.value)}
          placeholder="Email, username hoặc họ tên"
          leftIcon={<Search className="h-4 w-4" />}
        />
        <Select
          label="Role"
          value={role}
          onChange={(event) => {
            setPage(1);
            setRole(event.target.value as "" | UserRole);
          }}
          options={[
            { label: "Tất cả", value: "" },
            { label: "Author", value: "Author" },
            { label: "Admin", value: "Admin" },
          ]}
        />
        <Button type="submit">Tìm kiếm</Button>
      </form>

      {mutationError ? (
        <div role="alert" className="rounded-lg border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
          {getApiErrorMessage(mutationError)}
        </div>
      ) : null}

      {usersQuery.isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 5 }, (_, index) => (
            <Skeleton key={index} className="h-24 rounded-xl" />
          ))}
        </div>
      ) : usersQuery.isError ? (
        <ErrorState
          title="Không tải được danh sách tài khoản"
          message={getApiErrorMessage(usersQuery.error)}
          onRetry={() => void usersQuery.refetch()}
        />
      ) : !result || result.items.length === 0 ? (
        <EmptyState title="Không có tài khoản" description="Không tìm thấy tài khoản phù hợp." />
      ) : (
        <>
          <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
            <table className="min-w-full divide-y divide-slate-200 text-sm">
              <thead className="bg-slate-50 text-left text-xs uppercase tracking-wide text-content-tertiary">
                <tr>
                  <th className="px-4 py-3">Tài khoản</th>
                  <th className="px-4 py-3">Role</th>
                  <th className="px-4 py-3">Trạng thái</th>
                  <th className="px-4 py-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {result.items.map((account) => {
                  const isCurrentUser = account.id === currentUser?.id;
                  const busy = (statusMutation.isPending && statusMutation.variables?.userId === account.id)
                    || (roleMutation.isPending && roleMutation.variables?.userId === account.id);
                  return (
                    <tr key={account.id}>
                      <td className="px-4 py-4">
                        <p className="font-semibold text-content-primary">{account.displayName}</p>
                        <p className="text-xs text-content-secondary">{account.email}</p>
                        <p className="text-xs text-content-tertiary">@{account.userName}</p>
                      </td>
                      <td className="px-4 py-4">
                        <span className="inline-flex items-center gap-1 rounded-full bg-slate-100 px-2.5 py-1 text-xs font-semibold">
                          <ShieldCheck className="h-3.5 w-3.5" /> {account.role}
                        </span>
                      </td>
                      <td className="px-4 py-4">
                        <span className={account.isActive ? "text-emerald-700" : "text-rose-700"}>
                          {account.isActive ? "Đang hoạt động" : "Đã vô hiệu hóa"}
                        </span>
                      </td>
                      <td className="px-4 py-4">
                        <div className="flex flex-wrap justify-end gap-2">
                          {account.role === "Author" ? (
                            <Button
                              size="sm"
                              variant={account.isActive ? "danger" : "outline"}
                              disabled={busy || isCurrentUser}
                              leftIcon={account.isActive ? <UserRoundX className="h-4 w-4" /> : <UserRoundCheck className="h-4 w-4" />}
                              onClick={() => {
                                const action = account.isActive ? "vô hiệu hóa" : "mở khóa";
                                if (window.confirm(`Bạn có chắc muốn ${action} tài khoản ${account.email}?`)) {
                                  statusMutation.mutate({ userId: account.id, isActive: !account.isActive });
                                }
                              }}
                            >
                              {account.isActive ? "Vô hiệu hóa" : "Mở khóa"}
                            </Button>
                          ) : null}
                          <Button
                            size="sm"
                            variant="outline"
                            disabled={busy || isCurrentUser}
                            onClick={() => {
                              const nextRole: UserRole = account.role === "Admin" ? "Author" : "Admin";
                              if (window.confirm(`Đổi role của ${account.email} thành ${nextRole}?`)) {
                                roleMutation.mutate({ userId: account.id, role: nextRole });
                              }
                            }}
                          >
                            Đổi thành {account.role === "Admin" ? "Author" : "Admin"}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          <p className="text-sm text-content-tertiary">Tổng cộng {result.totalCount} tài khoản</p>
          <Pagination currentPage={result.page} totalPages={result.totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
