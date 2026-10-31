"use client";

import { useState, type FormEvent } from "react";
import { Save, X } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { getApiErrorMessage } from "@/lib/api/error";
import { useUpdateCategory } from "@/hooks/useCategories";
import type { CategoryApiDto } from "@/types/api";

interface CategoryEditFormProps {
  category: CategoryApiDto;
  onCancel: () => void;
  onUpdated: (category: CategoryApiDto) => void;
}

export function CategoryEditForm({
  category,
  onCancel,
  onUpdated,
}: CategoryEditFormProps) {
  const [name, setName] = useState(category.name);
  const [description, setDescription] = useState(category.description ?? "");
  const updateCategory = useUpdateCategory();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      const updatedCategory = await updateCategory.mutateAsync({
        id: category.id,
        request: {
          name: name.trim(),
          // Send an empty string to let the API clear an existing description.
          description: description.trim(),
        },
      });
      onUpdated(updatedCategory);
    } catch {
      // Render the API error below.
    }
  }

  return (
    <section
      aria-labelledby="edit-category-heading"
      className="rounded-xl border border-brand/30 bg-brand-light/20 p-5 shadow-subtle sm:p-6"
    >
      <div className="mb-5 flex items-start justify-between gap-4">
        <div>
          <h2
            id="edit-category-heading"
            className="text-lg font-bold text-content-primary"
          >
            Chỉnh sửa danh mục
          </h2>
          <p className="mt-1 text-sm text-content-secondary">
            Slug hiện tại <span className="font-medium">{category.slug}</span> sẽ được giữ nguyên.
          </p>
        </div>
        <button
          type="button"
          onClick={onCancel}
          disabled={updateCategory.isPending}
          aria-label="Đóng biểu mẫu chỉnh sửa"
          className="rounded-lg p-2 text-content-tertiary hover:bg-white hover:text-content-primary disabled:opacity-50"
        >
          <X className="h-5 w-5" />
        </button>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Tên danh mục"
          name="name"
          value={name}
          onChange={(event) => {
            setName(event.target.value);
            updateCategory.reset();
          }}
          minLength={2}
          maxLength={50}
          required
          autoComplete="off"
          disabled={updateCategory.isPending}
          helperText="Từ 2 đến 50 ký tự. Slug sẽ không đổi khi đổi tên."
        />

        <div className="w-full space-y-1.5">
          <label
            htmlFor="edit-category-description"
            className="block text-xs font-semibold uppercase tracking-wider text-content-secondary"
          >
            Mô tả
          </label>
          <textarea
            id="edit-category-description"
            name="description"
            value={description}
            onChange={(event) => {
              setDescription(event.target.value);
              updateCategory.reset();
            }}
            maxLength={2000}
            rows={3}
            disabled={updateCategory.isPending}
            placeholder="Mô tả ngắn về danh mục"
            className="w-full resize-y rounded-lg border border-slate-300 bg-white px-3.5 py-2.5 text-sm text-content-primary transition duration-150 placeholder:text-slate-400 focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/20 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-400"
          />
          <p className="text-xs text-content-tertiary">
            Để trống nếu muốn xóa mô tả. {description.length}/2000 ký tự
          </p>
        </div>

        {updateCategory.isError && (
          <p role="alert" className="text-sm font-medium text-rose-600">
            {getApiErrorMessage(updateCategory.error)}
          </p>
        )}

        <div className="flex flex-wrap gap-3">
          <Button
            type="submit"
            leftIcon={<Save className="h-4 w-4" />}
            isLoading={updateCategory.isPending}
            disabled={name.trim().length < 2 || name.trim().length > 50}
          >
            Lưu thay đổi
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={updateCategory.isPending}
          >
            Hủy
          </Button>
        </div>
      </form>
    </section>
  );
}
