"use client";

import { useState, type FormEvent } from "react";
import axios from "axios";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { getApiErrorMessage } from "@/lib/api/error";
import { useCreateCategory } from "@/hooks/useCategories";
import type { CategoryApiDto } from "@/types/api";

interface CategoryCreateFormProps {
  existingNames: string[];
  onCancel: () => void;
  onCreated: (category: CategoryApiDto) => void;
}

export function CategoryCreateForm({ existingNames, onCancel, onCreated }: CategoryCreateFormProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const createCategory = useCreateCategory();
  const normalizedName = name.trim().toLocaleLowerCase("vi-VN");
  const isDuplicate = normalizedName.length > 0 && existingNames.some(
    (existingName) => existingName.trim().toLocaleLowerCase("vi-VN") === normalizedName,
  );

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (isDuplicate || name.trim().length < 2 || name.trim().length > 50) return;

    try {
      const category = await createCategory.mutateAsync({
        name: name.trim(),
        description: description.trim() || null,
      });
      onCreated(category);
    } catch {
      // The mutation error is rendered below with the API's Problem Details.
    }
  }

  return (
    <section
      aria-labelledby="create-category-heading"
      className="rounded-xl border border-slate-200 bg-white p-5 shadow-subtle sm:p-6"
    >
      <div className="mb-5">
        <h2
          id="create-category-heading"
          className="text-lg font-bold text-content-primary"
        >
          Tạo danh mục mới
        </h2>
        <p className="mt-1 text-sm text-content-secondary">
          Slug sẽ được tạo tự động từ tên danh mục.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Tên danh mục"
          name="name"
          value={name}
          onChange={(event) => {
            setName(event.target.value);
            createCategory.reset();
          }}
          minLength={2}
          maxLength={50}
          required
          autoComplete="off"
          placeholder="Ví dụ: Món chay"
          helperText="Từ 2 đến 50 ký tự. Tên danh mục không được trùng."
          error={isDuplicate ? "Danh mục này đã tồn tại. Hãy nhập tên khác." : undefined}
          disabled={createCategory.isPending}
        />

        <div className="w-full space-y-1.5">
          <label
            htmlFor="category-description"
            className="block text-xs font-semibold uppercase tracking-wider text-content-secondary"
          >
            Mô tả (không bắt buộc)
          </label>
          <textarea
            id="category-description"
            name="description"
            value={description}
            onChange={(event) => {
              setDescription(event.target.value);
              createCategory.reset();
            }}
            maxLength={2000}
            rows={3}
            disabled={createCategory.isPending}
            placeholder="Mô tả ngắn về danh mục"
            className="w-full resize-y rounded-lg border border-slate-300 bg-white px-3.5 py-2.5 text-sm text-content-primary transition duration-150 placeholder:text-slate-400 focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/20 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-400"
          />
          <p className="text-xs text-content-tertiary">
            {description.length}/2000 ký tự
          </p>
        </div>

        {createCategory.isError && (
          <p role="alert" className="text-sm font-medium text-rose-600">
            {axios.isAxiosError(createCategory.error) && createCategory.error.response?.status === 409
              ? "Danh mục này đã tồn tại. Hãy nhập tên khác."
              : getApiErrorMessage(createCategory.error)}
          </p>
        )}

        <div className="flex flex-wrap gap-3">
          <Button
            type="submit"
            leftIcon={<Plus className="h-4 w-4" />}
            isLoading={createCategory.isPending}
            disabled={isDuplicate || name.trim().length < 2 || name.trim().length > 50}
          >
            Tạo danh mục
          </Button>
          <Button type="button" variant="outline" onClick={onCancel} disabled={createCategory.isPending}>
            Hủy
          </Button>
        </div>
      </form>
    </section>
  );
}
