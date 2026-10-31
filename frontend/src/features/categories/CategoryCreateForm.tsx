"use client";

import { useState, type FormEvent } from "react";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { getApiErrorMessage } from "@/lib/api/error";
import { useCreateCategory } from "@/hooks/useCategories";

export function CategoryCreateForm() {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const createCategory = useCreateCategory();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSuccessMessage("");

    try {
      const category = await createCategory.mutateAsync({
        name: name.trim(),
        description: description.trim() || null,
      });
      setName("");
      setDescription("");
      setSuccessMessage(`Đã tạo danh mục “${category.name}”.`);
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
            setSuccessMessage("");
            createCategory.reset();
          }}
          minLength={2}
          maxLength={50}
          required
          autoComplete="off"
          placeholder="Ví dụ: Món chay"
          helperText="Từ 2 đến 50 ký tự."
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
              setSuccessMessage("");
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
            {getApiErrorMessage(createCategory.error)}
          </p>
        )}
        {successMessage && (
          <p role="status" className="text-sm font-medium text-emerald-700">
            {successMessage}
          </p>
        )}

        <Button
          type="submit"
          leftIcon={<Plus className="h-4 w-4" />}
          isLoading={createCategory.isPending}
          disabled={name.trim().length < 2 || name.trim().length > 50}
        >
          Tạo danh mục
        </Button>
      </form>
    </section>
  );
}
