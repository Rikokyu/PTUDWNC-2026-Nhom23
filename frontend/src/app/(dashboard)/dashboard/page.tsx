"use client";

import Link from "next/link";
import { ArrowRight, BookOpen, ChefHat, Clock, FileText, Plus } from "lucide-react";
import { useAuth } from "@/components/auth/AuthProvider";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Skeleton } from "@/components/ui/LoadingState";
import { useMyRecipes } from "@/hooks/useRecipes";
import { getApiErrorMessage } from "@/lib/api/error";
import { RecipeThumbnail } from "@/components/common/RecipeThumbnail";

const statusLabels = ["Bản nháp", "Đã xuất bản", "Đã lưu trữ"];

export default function DashboardOverviewPage() {
  const { user } = useAuth();
  const recipesQuery = useMyRecipes({ page: 1, pageSize: 50, sort: "-createdAt" });
  const recipes = recipesQuery.data?.items ?? [];
  const publishedCount = recipes.filter((recipe) => recipe.status === 1).length;
  const draftCount = recipes.filter((recipe) => recipe.status === 0).length;

  return (
    <div className="space-y-8">
      <div className="flex flex-col justify-between gap-6 overflow-hidden rounded-3xl bg-[#202b22] p-7 text-white shadow-card sm:flex-row sm:items-center sm:p-9">
        <div className="space-y-1">
          <p className="pb-2 text-xs font-bold uppercase tracking-[0.2em] text-[#e5c49d]">{user?.role === "Admin" ? "Bảng điều khiển quản trị" : "Góc làm việc của tác giả"}</p>
          <h1 className="font-display text-3xl font-bold text-white">
            Xin chào, {user?.displayName ?? "bạn"}
          </h1>
          <p className="pt-1 text-sm text-stone-300">
            {user?.role === "Admin" ? "Theo dõi bài viết của bạn và quản lý nội dung cộng đồng." : "Tiếp tục viết, chỉnh sửa và chia sẻ công thức của bạn."}
          </p>
        </div>
        <Link href="/dashboard/recipes/new" className="shrink-0">
          <Button variant="primary" size="md" leftIcon={<Plus className="h-4 w-4" />}>
            Đăng công thức mới
          </Button>
        </Link>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {recipesQuery.isLoading ? Array.from({ length: 3 }, (_, index) => (
          <Skeleton key={index} className="h-28 rounded-2xl" />
        )) : (
          <>
            <MetricCard label="Tổng công thức" value={recipesQuery.data?.totalCount ?? 0} icon={<ChefHat className="h-4 w-4 text-brand" />} />
            <MetricCard label="Đã xuất bản" value={publishedCount} icon={<BookOpen className="h-4 w-4 text-accent" />} />
            <MetricCard label="Bản nháp" value={draftCount} icon={<FileText className="h-4 w-4 text-amber-600" />} />
          </>
        )}
      </div>

      <section className="overflow-hidden rounded-2xl border border-stone-200 bg-surface shadow-subtle">
        <div className="flex items-center justify-between border-b border-slate-100 p-5">
          <h2 className="font-display text-lg font-bold text-content-primary">Công thức gần đây của bạn</h2>
          <Link href="/dashboard/recipes" className="text-xs font-semibold text-brand hover:text-brand-hover">
            Quản lý tất cả <ArrowRight className="inline h-3.5 w-3.5" />
          </Link>
        </div>

        {recipesQuery.isError ? (
          <ErrorState
            title="Không tải được công thức của bạn"
            message={getApiErrorMessage(recipesQuery.error)}
            onRetry={() => void recipesQuery.refetch()}
          />
        ) : !recipesQuery.isLoading && recipes.length === 0 ? (
          <EmptyState
            title="Bạn chưa có công thức"
            description="Hãy tạo công thức đầu tiên để bắt đầu chia sẻ."
          />
        ) : (
          <div className="divide-y divide-slate-100">
            {recipes.slice(0, 3).map((recipe) => (
              <div key={recipe.id} className="flex items-center justify-between gap-4 p-4 transition-colors hover:bg-slate-50/60 sm:p-5">
                <div className="flex min-w-0 items-center gap-4">
                  <RecipeThumbnail
                    src={recipe.thumbnailUrl}
                    alt={recipe.title}
                    className="h-14 w-14 shrink-0 rounded-lg border border-slate-200 object-cover"
                  />
                  <div className="min-w-0">
                    <h3 className="truncate text-sm font-bold text-content-primary">{recipe.title}</h3>
                    <div className="mt-1 flex items-center gap-3 text-xs text-content-tertiary">
                      <Badge variant="neutral" size="sm">{statusLabels[recipe.status]}</Badge>
                      <span className="flex items-center gap-1">
                        <Clock className="h-3 w-3" />
                        {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút
                      </span>
                    </div>
                  </div>
                </div>
                <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`}>
                  <Button variant="outline" size="sm">Xem bài</Button>
                </Link>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

function MetricCard({ label, value, icon }: { label: string; value: number; icon: React.ReactNode }) {
  return (
    <div className="space-y-3 rounded-2xl border border-stone-200 bg-surface p-5 shadow-subtle">
      <div className="flex items-center justify-between text-xs font-semibold uppercase tracking-wider text-content-tertiary">
        <span>{label}</span>{icon}
      </div>
      <p className="font-display text-3xl font-bold text-content-primary">{value}</p>
    </div>
  );
}
