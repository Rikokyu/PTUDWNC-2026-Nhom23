"use client";

import { useEffect, useRef, useState } from "react";
import { Plus } from "lucide-react";
import { CategoryGrid } from "@/features/categories/CategoryGrid";
import { CategoryCreateForm } from "@/features/categories/CategoryCreateForm";
import { CategoryEditForm } from "@/features/categories/CategoryEditForm";
import { useCategories, useDeleteCategory } from "@/hooks/useCategories";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Skeleton } from "@/components/ui/LoadingState";
import { Button } from "@/components/ui/Button";
import { getApiErrorMessage } from "@/lib/api/error";
import { useAuth } from "@/components/auth/AuthProvider";
import type { CategoryApiDto } from "@/types/api";

export default function DashboardCategoriesPage() {
  const categoriesQuery = useCategories();
  const deleteCategory = useDeleteCategory();
  const { user } = useAuth();
  const [editingCategory, setEditingCategory] = useState<CategoryApiDto | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const createButtonRef = useRef<HTMLButtonElement>(null);
  const createFormRef = useRef<HTMLDivElement>(null);
  const editFormRef = useRef<HTMLDivElement>(null);
  const [updateMessage, setUpdateMessage] = useState("");
  const [createMessage, setCreateMessage] = useState("");
  const [deleteMessage, setDeleteMessage] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [deletingCategoryId, setDeletingCategoryId] = useState<string | null>(null);

  useEffect(() => {
    if (!showCreateForm) return;
    const frame = window.requestAnimationFrame(() => {
      createFormRef.current?.scrollIntoView({ behavior: "smooth", block: "center" });
      createFormRef.current?.querySelector<HTMLElement>("input")?.focus({ preventScroll: true });
    });
    return () => window.cancelAnimationFrame(frame);
  }, [showCreateForm]);

  useEffect(() => {
    if (!editingCategory) return;
    const frame = window.requestAnimationFrame(() => {
      editFormRef.current?.scrollIntoView({ behavior: "smooth", block: "center" });
      editFormRef.current?.querySelector<HTMLElement>("input")?.focus({ preventScroll: true });
    });
    return () => window.cancelAnimationFrame(frame);
  }, [editingCategory]);

  function cancelEdit() {
    setEditingCategory(null);
    window.requestAnimationFrame(() => {
      createButtonRef.current?.focus();
    });
  }

  function closeCreateForm() {
    setShowCreateForm(false);
    window.requestAnimationFrame(() => createButtonRef.current?.focus());
  }

  async function handleDelete(category: CategoryApiDto) {
    const confirmed = window.confirm(
      `Bạn có chắc muốn xóa danh mục “${category.name}”? Danh mục còn công thức sẽ không thể xóa.`,
    );
    if (!confirmed) return;

    setDeletingCategoryId(category.id);
    setDeleteError("");
    setDeleteMessage("");

    try {
      await deleteCategory.mutateAsync(category.id);
      setDeleteMessage(`Đã xóa danh mục “${category.name}”.`);
      if (editingCategory?.id === category.id) {
        setEditingCategory(null);
      }
    } catch (error) {
      setDeleteError(getApiErrorMessage(error));
    } finally {
      setDeletingCategoryId(null);
    }
  }

  if (user?.role !== "Admin") {
    return null;
  }

  return (
    <div className="space-y-8">
      <header className="flex flex-wrap items-end justify-between gap-4 border-b border-stone-200 pb-6">
        <div>
          <p className="mb-2 text-xs font-bold uppercase tracking-[0.2em] text-brand">Quản trị nội dung</p>
          <h1 className="font-display text-3xl font-bold text-content-primary">Quản lý danh mục</h1>
          <p className="mt-1 text-sm text-content-secondary">
            Tạo và sắp xếp các chủ đề để người đọc dễ tìm món ăn.
          </p>
        </div>
        <Button
          ref={createButtonRef}
          type="button"
          leftIcon={<Plus className="h-4 w-4" />}
          aria-expanded={showCreateForm}
          aria-controls="category-create-form"
          onClick={() => {
            setEditingCategory(null);
            setCreateMessage("");
            setShowCreateForm(true);
          }}
        >
          Tạo danh mục mới
        </Button>
      </header>
      {showCreateForm && (
        <div id="category-create-form" ref={createFormRef}>
          <CategoryCreateForm
            existingNames={(categoriesQuery.data ?? []).map((category) => category.name)}
            onCancel={closeCreateForm}
            onCreated={(category) => {
              setCreateMessage(`Đã tạo danh mục “${category.name}”.`);
              closeCreateForm();
            }}
          />
        </div>
      )}
      {editingCategory && (
        <div ref={editFormRef}>
          <CategoryEditForm
            key={editingCategory.id}
            category={editingCategory}
            onCancel={cancelEdit}
            onUpdated={(category) => {
              setEditingCategory(null);
              setUpdateMessage(`Đã cập nhật danh mục “${category.name}”. Slug vẫn là “${category.slug}”.`);
            }}
          />
        </div>
      )}
      {updateMessage && (
        <p role="status" className="text-sm font-medium text-emerald-700">
          {updateMessage}
        </p>
      )}
      {createMessage && (
        <p role="status" className="text-sm font-medium text-emerald-700">{createMessage}</p>
      )}
      {deleteMessage && (
        <p role="status" className="text-sm font-medium text-emerald-700">
          {deleteMessage}
        </p>
      )}
      {deleteError && (
        <p role="alert" className="text-sm font-medium text-rose-600">
          Không thể xóa danh mục: {deleteError}
        </p>
      )}
      {categoriesQuery.isLoading ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => <Skeleton key={index} className="h-44 rounded-xl" />)}
        </div>
      ) : categoriesQuery.isError ? (
        <ErrorState
          title="Không tải được danh mục"
          message={getApiErrorMessage(categoriesQuery.error)}
          onRetry={() => void categoriesQuery.refetch()}
        />
      ) : (categoriesQuery.data?.length ?? 0) === 0 ? (
        <EmptyState title="Chưa có danh mục" description="Database chưa có dữ liệu danh mục." />
      ) : (
        <CategoryGrid
          categories={categoriesQuery.data ?? []}
          onEdit={(category) => {
            setShowCreateForm(false);
            setUpdateMessage("");
            setDeleteMessage("");
            setEditingCategory(category);
          }}
          onDelete={(category) => void handleDelete(category)}
          deletingCategoryId={deletingCategoryId}
        />
      )}
    </div>
  );
}
