"use client";

import Link from "next/link";
import { useState } from "react";
import {
  Archive,
  ChefHat,
  Eye,
  FilePenLine,
  Pencil,
  Plus,
  Search,
  Trash2,
} from "lucide-react";
import {
  useDashboardPreviewStore,
  type PreviewRecipe,
} from "@/store/dashboard-preview-store";
import {
  ConfirmDelete,
  DashboardHeading,
  difficultyLabels,
  formControl,
  Notice,
  RecipeStatus,
  statusLabels,
  usePreviewReady,
} from "@/components/dashboard/preview-ui";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { EmptyState } from "@/components/shared/empty-state";
import { Loading } from "@/components/shared/loading";
import { Pagination } from "@/components/shared/pagination";

export default function DashboardRecipesPage() {
  const ready = usePreviewReady();
  const { recipes, categories, saveRecipe, deleteRecipe } =
    useDashboardPreviewStore();
  const [query, setQuery] = useState("");
  const [status, setStatus] = useState("all");
  const [categoryId, setCategoryId] = useState("all");
  const [page, setPage] = useState(1);
  const [viewing, setViewing] = useState<PreviewRecipe | null>(null);
  const [deleting, setDeleting] = useState<PreviewRecipe | null>(null);
  const [notice, setNotice] = useState("");
  const filtered = recipes.filter(
    (recipe) =>
      recipe.title
        .toLocaleLowerCase("vi")
        .includes(query.toLocaleLowerCase("vi")) &&
      (status === "all" || recipe.status === status) &&
      (categoryId === "all" || recipe.categoryId === categoryId),
  );
  const totalPages = Math.ceil(filtered.length / 5);
  const currentPage = Math.min(page, Math.max(totalPages, 1));
  const visible = filtered.slice((currentPage - 1) * 5, currentPage * 5);
  const stats = [
    { label: "Tất cả công thức", value: recipes.length, icon: ChefHat },
    {
      label: "Đã xuất bản",
      value: recipes.filter((recipe) => recipe.status === "Published").length,
      icon: Eye,
    },
    {
      label: "Đang viết",
      value: recipes.filter((recipe) => recipe.status === "Draft").length,
      icon: FilePenLine,
    },
    {
      label: "Đã lưu trữ",
      value: recipes.filter((recipe) => recipe.status === "Archived").length,
      icon: Archive,
    },
  ];
  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <DashboardHeading
        eyebrow="Góc bếp của bạn"
        title="Công thức của tôi"
        description="Lưu lại ý tưởng, chăm chút từng món ăn và chia sẻ niềm vui vào bếp."
        action={
          <Button asChild>
            <Link href="/dashboard/recipes/preview-new/edit">
              <Plus />
              Thêm công thức minh họa
            </Link>
          </Button>
        }
      />
      {notice && <Notice>{notice}</Notice>}
      <div className="grid grid-cols-2 gap-3 xl:grid-cols-4">
        {stats.map(({ label, value, icon: Icon }) => (
          <div
            key={label}
            className="rounded-xl border border-slate-200 bg-white p-4"
          >
            <div className="flex items-center justify-between gap-2">
              <p className="text-xs text-slate-500">{label}</p>
              <Icon className="size-4 text-orange-600" />
            </div>
            <p className="mt-3 text-2xl font-semibold">{value}</p>
          </div>
        ))}
      </div>
      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
        <div className="space-y-4 border-b border-slate-100 p-4 sm:p-5">
          <div
            className="flex flex-wrap gap-2"
            aria-label="Lọc theo trạng thái"
          >
            {[["all", "Tất cả"], ...Object.entries(statusLabels)].map(
              ([key, label]) => (
                <Button
                  key={key}
                  variant={status === key ? "default" : "ghost"}
                  aria-pressed={status === key}
                  onClick={() => {
                    setStatus(key);
                    setPage(1);
                  }}
                >
                  {label}
                </Button>
              ),
            )}
          </div>
          <div className="flex flex-col gap-3 sm:flex-row">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-2 size-4 text-slate-400" />
              <Input
                aria-label="Tìm công thức"
                placeholder="Tìm theo tên món ăn…"
                className="pl-9"
                value={query}
                onChange={(event) => {
                  setQuery(event.target.value);
                  setPage(1);
                }}
              />
            </div>
            <select
              aria-label="Lọc theo danh mục"
              className={`${formControl} sm:max-w-52`}
              value={categoryId}
              onChange={(event) => {
                setCategoryId(event.target.value);
                setPage(1);
              }}
            >
              <option value="all">Tất cả danh mục</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </div>
        </div>
        {!ready ? (
          <div className="p-8">
            <Loading label="Đang tải góc bếp…" />
          </div>
        ) : !visible.length ? (
          <div className="p-5">
            <EmptyState
              title="Chưa có công thức phù hợp"
              description="Thử thay đổi bộ lọc hoặc bắt đầu viết một món ăn mới."
              actionHref="/dashboard/recipes/preview-new/edit"
              actionLabel="Viết công thức minh họa"
            />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[660px] text-left text-sm">
              <caption className="sr-only">
                Danh sách công thức minh họa và thao tác quản lý
              </caption>
              <thead className="bg-slate-50 text-xs text-slate-500">
                <tr>
                  <th scope="col" className="px-5 py-3 font-medium">
                    Công thức
                  </th>
                  <th scope="col" className="px-3 py-3 font-medium">
                    Trạng thái
                  </th>
                  <th scope="col" className="px-3 py-3 font-medium">
                    Cập nhật
                  </th>
                  <th scope="col" className="px-5 py-3 text-right font-medium">
                    Thao tác
                  </th>
                </tr>
              </thead>
              <tbody>
                {visible.map((recipe) => {
                  const category = categories.find(
                    (item) => item.id === recipe.categoryId,
                  );
                  return (
                    <tr
                      key={recipe.id}
                      className="border-t border-slate-100 transition-colors hover:bg-orange-50/40"
                    >
                      <td className="px-5 py-4">
                        <div className="flex items-center gap-3">
                          <span
                            aria-hidden="true"
                            className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-orange-50 text-2xl"
                          >
                            {category?.emoji ?? "🍽️"}
                          </span>
                          <div>
                            <Link
                              href={`/dashboard/recipes/${recipe.id}/edit`}
                              className="font-semibold text-slate-800 hover:text-orange-700"
                            >
                              {recipe.title}
                            </Link>
                            <p className="mt-1 text-xs text-slate-500">
                              {category?.name ?? "Chưa phân loại"} ·{" "}
                              {recipe.prepTime + recipe.cookTime} phút
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-3 py-4">
                        <RecipeStatus status={recipe.status} />
                      </td>
                      <td className="px-3 py-4 text-xs text-slate-500">
                        {new Date(recipe.updatedAt).toLocaleDateString("vi-VN")}
                      </td>
                      <td className="px-5 py-4">
                        <div className="flex justify-end gap-1">
                          <Button
                            variant="ghost"
                            size="icon"
                            aria-label={`Xem ${recipe.title}`}
                            onClick={() => setViewing(recipe)}
                          >
                            <Eye />
                          </Button>
                          <Button asChild variant="ghost" size="icon">
                            <Link
                              aria-label={`Chỉnh sửa ${recipe.title}`}
                              href={`/dashboard/recipes/${recipe.id}/edit`}
                            >
                              <Pencil />
                            </Link>
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            aria-label={`${recipe.status === "Archived" ? "Khôi phục" : "Lưu trữ"} ${recipe.title}`}
                            onClick={() => {
                              saveRecipe({
                                ...recipe,
                                status:
                                  recipe.status === "Archived"
                                    ? "Draft"
                                    : "Archived",
                                updatedAt: new Date().toISOString(),
                              });
                              setNotice(
                                "Đã cập nhật trạng thái trong bản xem trước.",
                              );
                            }}
                          >
                            <Archive />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            aria-label={`Xóa ${recipe.title}`}
                            onClick={() => setDeleting(recipe)}
                          >
                            <Trash2 className="text-rose-600" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
        {ready && filtered.length > 0 && (
          <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 px-5 py-4">
            <p className="text-xs text-slate-500">
              Hiển thị {(currentPage - 1) * 5 + 1}–
              {Math.min(currentPage * 5, filtered.length)} / {filtered.length}{" "}
              công thức
            </p>
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              onPageChange={setPage}
            />
          </div>
        )}
      </section>
      <Dialog
        open={viewing !== null}
        onOpenChange={(open) => {
          if (!open) setViewing(null);
        }}
      >
        <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>{viewing?.title}</DialogTitle>
            <DialogDescription>
              Xem trước nội dung công thức minh họa.
            </DialogDescription>
          </DialogHeader>
          {viewing && (
            <div className="space-y-5">
              <RecipeStatus status={viewing.status} />
              <p className="text-sm leading-relaxed text-slate-600">
                {viewing.description}
              </p>
              <p className="text-xs text-slate-500">
                {viewing.prepTime + viewing.cookTime} phút · {viewing.servings}{" "}
                khẩu phần · {difficultyLabels[viewing.difficulty]}
              </p>
              <div>
                <h3 className="mb-2 font-semibold">Nguyên liệu</h3>
                <ul className="list-inside list-disc space-y-2 text-sm text-slate-600">
                  {viewing.ingredients.map((ingredient, index) => (
                    <li key={index}>{ingredient}</li>
                  ))}
                </ul>
              </div>
              <div>
                <h3 className="mb-2 font-semibold">Cách làm</h3>
                <ol className="list-inside list-decimal space-y-3 text-sm leading-relaxed text-slate-600">
                  {viewing.steps.map((step, index) => (
                    <li key={index}>{step}</li>
                  ))}
                </ol>
              </div>
              <Button asChild>
                <Link href={`/dashboard/recipes/${viewing.id}/edit`}>
                  Chỉnh sửa công thức
                </Link>
              </Button>
            </div>
          )}
        </DialogContent>
      </Dialog>
      <ConfirmDelete
        open={deleting !== null}
        onOpenChange={(open) => {
          if (!open) setDeleting(null);
        }}
        title="Xóa công thức?"
        description={`“${deleting?.title ?? ""}” sẽ bị xóa khỏi bản xem trước. Thao tác này không ảnh hưởng dữ liệu trên máy chủ.`}
        onConfirm={() => {
          if (deleting) deleteRecipe(deleting.id);
          setDeleting(null);
          setNotice("Đã xóa công thức trong bản xem trước.");
        }}
      />
    </div>
  );
}
