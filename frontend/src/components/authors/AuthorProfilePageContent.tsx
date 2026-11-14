"use client";

import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { ApiRecipeCard } from "@/components/common/ApiRecipeCard";
import { AvatarImage } from "@/components/common/AvatarImage";
import { ErrorState } from "@/components/ui/ErrorState";
import { EmptyState } from "@/components/ui/EmptyState";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { Pagination } from "@/components/ui/Pagination";
import { authorsApi } from "@/lib/api/authors";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import { useAuthStore } from "@/store/useAuthStore";

export function AuthorProfilePageContent({ authorId }: { authorId: string }) {
  const currentUser = useAuthStore((state) => state.user);
  const [page, setPage] = useState(1);

  useEffect(() => {
    setPage(1);
    const frame = window.requestAnimationFrame(() => {
      window.scrollTo({ top: 0, left: 0, behavior: "instant" });
    });
    return () => window.cancelAnimationFrame(frame);
  }, [authorId]);

  const authorQuery = useQuery({
    queryKey: ["author", authorId, page],
    queryFn: () => authorsApi.getProfile(authorId, page),
    enabled: Boolean(authorId),
  });

  if (authorQuery.isLoading) return <RecipeCardSkeleton />;
  if (authorQuery.isError || !authorQuery.data) {
    return <ErrorState title="Không tìm thấy tác giả" message="Trang tác giả không tồn tại hoặc API chưa khả dụng." onRetry={() => void authorQuery.refetch()} />;
  }

  const author = authorQuery.data;
  const isOwnProfile = author.id === currentUser?.id;
  const avatarUrl = resolveApiAssetUrl(author.avatarUrl);

  return (
    <div className="mx-auto max-w-6xl space-y-10 pb-12">
      <header className="flex flex-col items-center gap-5 rounded-3xl border border-stone-200 bg-surface p-6 text-center shadow-subtle sm:flex-row sm:p-8 sm:text-left">
        <div className="flex h-24 w-24 shrink-0 items-center justify-center overflow-hidden rounded-full bg-brand-light text-3xl font-bold text-brand-hover">
          <AvatarImage src={avatarUrl} alt={`Ảnh đại diện ${author.displayName}`} className="h-full w-full object-cover" />
        </div>
        <div>
          <p className="editorial-kicker">{isOwnProfile ? "Trang của tôi" : "Tác giả"}</p>
          <h1 className="mt-1 font-display text-3xl font-bold text-content-primary sm:text-4xl">{author.displayName}</h1>
          {author.bio && <p className="mt-3 max-w-2xl whitespace-pre-line text-sm leading-6 text-content-secondary">{author.bio}</p>}
          <p className="mt-3 text-sm text-content-tertiary">{author.recipes.totalCount} công thức đã xuất bản</p>
          {isOwnProfile && <Link href="/dashboard/profile" className="mt-3 inline-block text-sm font-semibold text-brand hover:underline">Chỉnh sửa hồ sơ</Link>}
        </div>
      </header>

      <section aria-label="Công thức của tác giả" className="space-y-5">
        <h2 className="font-display text-2xl font-bold text-content-primary">Công thức đã xuất bản</h2>
        {author.recipes.items.length === 0 ? (
          <EmptyState title="Chưa có công thức công khai" description="Tác giả này chưa xuất bản công thức nào." />
        ) : (
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {author.recipes.items.map((recipe) => <ApiRecipeCard key={recipe.id} recipe={recipe} />)}
          </div>
        )}
        <Pagination currentPage={page} totalPages={author.recipes.totalPages} onPageChange={(nextPage) => { setPage(nextPage); window.scrollTo({ top: 0, behavior: "smooth" }); }} />
      </section>
    </div>
  );
}
