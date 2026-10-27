"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { adminUserApi, type AdminUserFilters } from "@/lib/api/adminUsers";

export const adminUserKeys = {
  all: () => ["admin-users"] as const,
  list: (filters: AdminUserFilters) => [...adminUserKeys.all(), filters] as const,
};

export function useAdminUsers(filters: AdminUserFilters) {
  return useQuery({
    queryKey: adminUserKeys.list(filters),
    queryFn: () => adminUserApi.getAll(filters),
  });
}

export function useSetUserActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, isActive }: { userId: string; isActive: boolean }) =>
      adminUserApi.setActive(userId, isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: adminUserKeys.all() }),
  });
}

export function useSetUserRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: "Author" | "Admin" }) =>
      adminUserApi.setRole(userId, role),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: adminUserKeys.all() }),
  });
}
