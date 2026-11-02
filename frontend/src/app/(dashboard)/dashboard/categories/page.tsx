"use client";

import { useState } from "react";
import { CategoryGrid } from "@/features/categories/CategoryGrid";
import { CategoryCreateForm } from "@/features/categories/CategoryCreateForm";
import { CategoryEditForm } from "@/features/categories/CategoryEditForm";
import { useCategories, useDeleteCategory } from "@/hooks/useCategories";
import { EmptyState } from "@/components/ui/EmptyState";
import { ErrorState } from "@/components/ui/ErrorState";
import { Skeleton } from "@/components/ui/LoadingState";
import { getApiErrorMessage } from "@/lib/api/error";
import { useAuth } from "@/components/auth/AuthProvider";
import type { CategoryApiDto } from "@/types/api";

export default function DashboardCategoriesPage() {
  const categoriesQuery = useCategories();
  const deleteCategory = useDeleteCategory();
  const { user } = useAuth();
  const [editingCategory, setEditingCategory] = useState<CategoryApiDto | null>(null);
  const [updateMessage, setUpdateMessage] = useState("");
  const [deleteMessage, setDeleteMessage] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [deletingCategoryId, setDeletingCategoryId] = useState<string | null>(null);

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
      <header>
        <h1 className="text-2xl font-extrabold text-content-primary">Quản lý danh mục</h1>
        <p className="mt-1 text-sm text-content-secondary">
          Danh sách danh mục hiện có trong PostgreSQL. Trang này chỉ dành cho Admin.
        </p>
      </header>
      <CategoryCreateForm />
      {editingCategory && (
        <CategoryEditForm
          key={editingCategory.id}
          category={editingCategory}
          onCancel={() => setEditingCategory(null)}
          onUpdated={(category) => {
            setEditingCategory(null);
            setUpdateMessage(`Đã cập nhật danh mục “${category.name}”. Slug vẫn là “${category.slug}”.`);
          }}
        />
      )}
      {updateMessage && (
        <p role="status" className="text-sm font-medium text-emerald-700">
          {updateMessage}
        </p>
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
