"use client";

import { useState, type FormEvent } from "react";
import { FolderOpen, Pencil, Plus, Search, Trash2 } from "lucide-react";
import {
  useDashboardPreviewStore,
  type PreviewCategory,
} from "@/store/dashboard-preview-store";
import {
  ConfirmDelete,
  DashboardHeading,
  Field,
  formControl,
  Notice,
  usePreviewReady,
} from "@/components/dashboard/preview-ui";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { EmptyState } from "@/components/shared/empty-state";
import { Loading } from "@/components/shared/loading";
import { ErrorMessage } from "@/components/shared/error-message";

const blank = { id: "", name: "", slug: "", description: "", emoji: "🍽️" };
function slugify(value: string) {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[đĐ]/g, "d")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "");
}

export default function DashboardCategoriesPage() {
  const ready = usePreviewReady();
  const { categories, recipes, saveCategory, deleteCategory } =
    useDashboardPreviewStore();
  const [query, setQuery] = useState("");
  const [editing, setEditing] = useState<PreviewCategory | null>(null);
  const [deleting, setDeleting] = useState<PreviewCategory | null>(null);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const filtered = categories.filter((category) =>
    `${category.name} ${category.description}`
      .toLocaleLowerCase("vi")
      .includes(query.toLocaleLowerCase("vi")),
  );
  function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!editing) return;
    const name = editing.name.trim();
    const slug = editing.slug.trim();
    if (!name || !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(slug)) {
      setError("Nhập tên và đường dẫn hợp lệ, ví dụ: mon-chinh.");
      return;
    }
    if (
      categories.some(
        (category) =>
          category.id !== editing.id &&
          (category.slug === slug ||
            category.name.toLocaleLowerCase("vi") ===
              name.toLocaleLowerCase("vi")),
      )
    ) {
      setError("Tên hoặc đường dẫn này đã tồn tại. Hãy chọn tên khác.");
      return;
    }
    saveCategory({
      ...editing,
      id: editing.id || `preview-category-${crypto.randomUUID()}`,
      name,
      slug,
      description: editing.description.trim(),
    });
    setNotice(`Đã lưu danh mục “${name}” trong bản xem trước.`);
    setEditing(null);
  }

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <DashboardHeading
        eyebrow="Không gian quản trị"
        title="Quản lý danh mục"
        description="Sắp xếp góc bếp thành những chủ đề dễ khám phá."
        action={
          <Button
            onClick={() => {
              setEditing({ ...blank });
              setError("");
            }}
          >
            <Plus />
            Thêm danh mục
          </Button>
        }
      />
      {notice && <Notice>{notice}</Notice>}
      <div className="grid gap-3 sm:grid-cols-3">
        {[
          ["Danh mục", categories.length],
          ["Công thức minh họa", recipes.length],
          [
            "Danh mục còn trống",
            categories.filter(
              (category) =>
                !recipes.some((recipe) => recipe.categoryId === category.id),
            ).length,
          ],
        ].map(([label, value]) => (
          <div
            key={label}
            className="rounded-xl border border-slate-200 bg-white p-5"
          >
            <p className="text-xs text-slate-500">{label}</p>
            <p className="mt-2 text-2xl font-semibold">{value}</p>
          </div>
        ))}
      </div>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="text-lg font-semibold">Tất cả danh mục</h2>
        <div className="relative w-full sm:w-72">
          <Search className="absolute left-3 top-2 size-4 text-slate-400" />
          <Input
            aria-label="Tìm danh mục"
            placeholder="Tìm danh mục…"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            className="bg-white pl-9"
          />
        </div>
      </div>
      {!ready ? (
        <Loading label="Đang tải danh mục…" />
      ) : filtered.length === 0 ? (
        <EmptyState
          title="Chưa tìm thấy danh mục"
          description="Thử từ khóa khác hoặc thêm một danh mục mới."
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {filtered.map((category) => {
            const count = recipes.filter(
              (recipe) => recipe.categoryId === category.id,
            ).length;
            return (
              <article
                key={category.id}
                className="flex flex-col rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"
              >
                <div className="mb-5 flex items-start justify-between">
                  <span
                    aria-hidden="true"
                    className="flex size-12 items-center justify-center rounded-xl bg-orange-50 text-2xl"
                  >
                    {category.emoji}
                  </span>
                  <Badge variant="outline">{count} công thức</Badge>
                </div>
                <h3 className="text-base font-semibold">{category.name}</h3>
                <p className="mt-1 text-xs text-slate-400">/{category.slug}</p>
                <p className="my-4 flex-1 text-sm leading-relaxed text-slate-500">
                  {category.description || "Chưa có mô tả cho danh mục này."}
                </p>
                <div className="flex items-center justify-between border-t border-slate-100 pt-4">
                  <Button
                    variant="outline"
                    onClick={() => {
                      setEditing({ ...category });
                      setError("");
                    }}
                  >
                    <Pencil />
                    Chỉnh sửa
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label={`Xóa ${category.name}`}
                    disabled={count > 0}
                    title={
                      count > 0
                        ? "Chuyển công thức sang danh mục khác trước khi xóa."
                        : "Xóa danh mục"
                    }
                    onClick={() => setDeleting(category)}
                  >
                    <Trash2 className="text-rose-600" />
                  </Button>
                </div>
              </article>
            );
          })}
        </div>
      )}
      <p className="flex items-center gap-2 text-xs text-slate-500">
        <FolderOpen className="size-4" />
        Danh mục đang chứa công thức cần được làm trống trước khi xóa.
      </p>
      <Dialog
        open={editing !== null}
        onOpenChange={(open) => {
          if (!open) setEditing(null);
        }}
      >
        <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>
              {editing?.id ? "Chỉnh sửa danh mục" : "Thêm danh mục mới"}
            </DialogTitle>
            <DialogDescription>
              Đặt tên dễ nhớ để mọi người tìm thấy món ăn yêu thích.
            </DialogDescription>
          </DialogHeader>
          {editing && (
            <form onSubmit={save} className="space-y-4">
              {error && <ErrorMessage message={error} />}
              <div className="grid grid-cols-[5rem_1fr] gap-3">
                <Field label="Biểu tượng" htmlFor="category-emoji">
                  <Input
                    id="category-emoji"
                    value={editing.emoji}
                    maxLength={8}
                    onChange={(event) =>
                      setEditing({ ...editing, emoji: event.target.value })
                    }
                  />
                </Field>
                <Field label="Tên danh mục *" htmlFor="category-name">
                  <Input
                    id="category-name"
                    required
                    maxLength={100}
                    value={editing.name}
                    onChange={(event) => {
                      const name = event.target.value;
                      setEditing({
                        ...editing,
                        name,
                        slug: !editing.id ? slugify(name) : editing.slug,
                      });
                    }}
                  />
                </Field>
              </div>
              <Field
                label="Đường dẫn *"
                htmlFor="category-slug"
                hint="Chỉ dùng chữ thường không dấu, số và dấu gạch ngang."
              >
                <Input
                  id="category-slug"
                  required
                  value={editing.slug}
                  maxLength={120}
                  onChange={(event) =>
                    setEditing({ ...editing, slug: event.target.value })
                  }
                />
              </Field>
              <Field label="Mô tả" htmlFor="category-description">
                <textarea
                  id="category-description"
                  rows={3}
                  maxLength={500}
                  className={formControl}
                  value={editing.description}
                  onChange={(event) =>
                    setEditing({ ...editing, description: event.target.value })
                  }
                />
              </Field>
              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setEditing(null)}
                >
                  Hủy
                </Button>
                <Button type="submit">Lưu danh mục</Button>
              </DialogFooter>
            </form>
          )}
        </DialogContent>
      </Dialog>
      <ConfirmDelete
        open={deleting !== null}
        onOpenChange={(open) => {
          if (!open) setDeleting(null);
        }}
        title="Xóa danh mục?"
        description={`Danh mục “${deleting?.name ?? ""}” sẽ bị xóa khỏi dữ liệu minh họa.`}
        onConfirm={() => {
          if (
            deleting &&
            !recipes.some((recipe) => recipe.categoryId === deleting.id)
          ) {
            deleteCategory(deleting.id);
            setNotice("Đã xóa danh mục trong bản xem trước.");
          }
          setDeleting(null);
        }}
      />
    </div>
  );
}
