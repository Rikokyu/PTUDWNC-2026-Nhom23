"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { Input } from "@/components/ui/Input";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { ErrorState } from "@/components/ui/ErrorState";
import { Plus, Trash2, CheckCircle2 } from "lucide-react";
import { useCategories } from "@/hooks/useCategories";
import { recipeApi } from "@/lib/api/recipes";
import { getApiErrorMessage } from "@/lib/api/error";
import { IngredientUnitField } from "@/components/recipes/IngredientUnitField";
import { isNonNegativeIngredientQuantity } from "@/lib/recipeIngredient";
import { recipeKeys } from "@/hooks/useRecipes";
import { useAuthStore } from "@/store/useAuthStore";

export default function NewRecipePage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const accessToken = useAuthStore((state) => state.accessToken);
  const categoriesQuery = useCategories();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submittedSuccess, setSubmittedSuccess] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [createdRecipeId, setCreatedRecipeId] = useState<string | null>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imageAltText, setImageAltText] = useState("");

  // Form State
  const [title, setTitle] = useState("");
  const [summary, setSummary] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [difficulty, setDifficulty] = useState("Easy");
  const [prepTime, setPrepTime] = useState(15);
  const [cookTime, setCookTime] = useState(20);
  const [servings, setServings] = useState(4);
  const [instructions, setInstructions] = useState("");

  useEffect(() => {
    if (!accessToken) {
      router.replace("/auth/login");
    }
  }, [accessToken, router]);

  useEffect(() => {
    if (!categoryId && categoriesQuery.data?.length) {
      setCategoryId(categoriesQuery.data[0].id);
    }
  }, [categoryId, categoriesQuery.data]);

  // Dynamic Ingredients State
  const [ingredients, setIngredients] = useState([
    { name: "", quantity: "1", unit: "g" },
    { name: "", quantity: "1", unit: "muỗng canh" },
  ]);

  // Dynamic Steps State
  const [steps, setSteps] = useState([
    { description: "", durationMinutes: 10 },
  ]);

  const addIngredientRow = () => {
    setIngredients([...ingredients, { name: "", quantity: "", unit: "g" }]);
  };

  const removeIngredientRow = (index: number) => {
    setIngredients(ingredients.filter((_, i) => i !== index));
  };

  const addStepRow = () => {
    setSteps([...steps, { description: "", durationMinutes: 5 }]);
  };

  const removeStepRow = (index: number) => {
    setSteps(steps.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);
    if (!accessToken) {
      router.replace("/auth/login");
      return;
    }
    const validIngredients = ingredients.filter((ingredient) => ingredient.name.trim());
    const validSteps = steps.filter((step) => step.description.trim());
    if (!categoryId || validIngredients.length === 0 || validSteps.length === 0) {
      setSubmitError("Hãy chọn danh mục, nhập ít nhất một nguyên liệu và một bước hướng dẫn.");
      return;
    }
    if (validIngredients.some((ingredient) => !isNonNegativeIngredientQuantity(ingredient.quantity))) {
      setSubmitError("Số lượng nguyên liệu không được nhỏ hơn 0. Có thể nhập phân số hoặc mô tả như “vừa đủ”.");
      return;
    }
    if (imageFile) {
      const supportedTypes = ["image/jpeg", "image/png", "image/webp", "image/avif"];
      if (!supportedTypes.includes(imageFile.type)) {
        setSubmitError("Ảnh phải có định dạng JPEG, PNG, WebP hoặc AVIF.");
        return;
      }
      if (imageFile.size > 5 * 1024 * 1024) {
        setSubmitError("Kích thước ảnh không được vượt quá 5 MB.");
        return;
      }
    }
    setIsSubmitting(true);
    try {
      const created = await recipeApi.create({
        title: title.trim(),
        description: summary.trim(),
        categoryId,
        prepTimeMinutes: prepTime,
        cookTimeMinutes: cookTime,
        servings,
        difficulty: difficulty === "Easy" ? 1 : difficulty === "Hard" ? 3 : 2,
        instructions:
          instructions.trim() ||
          validSteps.map((step, index) => `${index + 1}. ${step.description.trim()}`).join("\n"),
        ingredients: validIngredients.map((ingredient) => ({
          name: ingredient.name.trim(),
          quantity: ingredient.quantity.trim() || null,
          unit: ingredient.unit.trim() || null,
          notes: null,
        })),
        steps: validSteps.map((step, index) => ({
          title: `Bước ${index + 1}`,
          description: step.description.trim(),
          timerMinutes: step.durationMinutes || null,
          imageUrl: null,
        })),
      });
      if (imageFile) {
        try {
          await recipeApi.uploadImage(created.id, imageFile, imageAltText.trim());
        } catch (uploadError) {
          setCreatedRecipeId(created.id);
          setSubmitError(
            `Công thức nháp đã được tạo, nhưng ảnh chưa tải lên được: ${getApiErrorMessage(uploadError)}`,
          );
          await queryClient.invalidateQueries({ queryKey: recipeKeys.all() });
          return;
        }
      }
      setSubmittedSuccess(true);
      await queryClient.invalidateQueries({ queryKey: recipeKeys.all() });
      router.push(`/dashboard/recipes/${created.id}/edit`);
    } catch (error) {
      setSubmitError(getApiErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-8 max-w-4xl">
      <Breadcrumb
        items={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Công thức", href: "/dashboard/recipes" },
          { label: "Đăng công thức mới" },
        ]}
      />

      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-extrabold text-content-primary tracking-tight">
            Đăng Công Thức Nấu Ăn Mới
          </h1>
          <p className="text-xs text-content-secondary mt-1">
            Chia sẻ công thức ngon của bạn tới cộng đồng nấu ăn Culinary Blog.
          </p>
        </div>
      </div>

      {submittedSuccess && (
        <div className="p-4 rounded-xl bg-brand-light border border-brand-border text-brand-hover flex items-center gap-3">
          <CheckCircle2 className="w-5 h-5 shrink-0" />
          <span className="text-sm font-semibold">
            Đã tạo công thức ở trạng thái nháp. Đang mở trang quản lý...
          </span>
        </div>
      )}

      {submitError && (
        <ErrorState
          title={createdRecipeId ? "Công thức đã tạo, upload ảnh chưa thành công" : "Không thể tạo công thức"}
          message={submitError}
        />
      )}
      {createdRecipeId && submitError && (
        <Link
          href={`/dashboard/recipes/${createdRecipeId}/edit`}
          className="inline-flex text-sm font-semibold text-brand hover:underline"
        >
          Mở trang quản lý để tải ảnh lên
        </Link>
      )}
      {categoriesQuery.isError && (
        <ErrorState
          title="Không tải được danh mục"
          message="Không thể tạo công thức khi chưa lấy được danh mục từ API."
          onRetry={() => void categoriesQuery.refetch()}
        />
      )}

      <form onSubmit={handleSubmit} className="space-y-8">
        {/* SECTION 1: GENERAL INFORMATIONS */}
        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-5">
          <h2 className="text-base font-bold text-content-primary pb-2 border-b border-slate-100">
            1. Thông tin chung
          </h2>

          <Input
            label="Tên món ăn *"
            placeholder="Ví dụ: Phở Bò Bắp Hoa Hà Nội Chuẩn Vị"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
          />

          <div className="space-y-1.5">
            <label className="block text-xs font-semibold uppercase tracking-wider text-content-secondary">
              Mô tả ngắn món ăn *
            </label>
            <textarea
              rows={3}
              placeholder="Giới thiệu điểm đặc biệt, hương vị chính hoặc nguồn gốc của món ăn..."
              value={summary}
              onChange={(e) => setSummary(e.target.value)}
              className="w-full rounded-lg border border-slate-300 bg-white p-3 text-sm text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/20 placeholder:text-slate-400"
              required
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Select
              label="Danh mục ẩm thực *"
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              options={(categoriesQuery.data ?? []).map((category) => ({
                label: category.name,
                value: category.id,
              }))}
              disabled={categoriesQuery.isLoading || categoriesQuery.isError}
            />
            <Select
              label="Độ khó *"
              value={difficulty}
              onChange={(e) => setDifficulty(e.target.value)}
              options={[
                { label: "Dễ làm", value: "Easy" },
                { label: "Trung bình", value: "Medium" },
                { label: "Cầu kỳ / Khó", value: "Hard" },
              ]}
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <Input
              label="Thời gian chuẩn bị (Phút)"
              type="number"
              value={prepTime}
              onChange={(e) => setPrepTime(Number(e.target.value))}
              min={1}
            />
            <Input
              label="Thời gian nấu (Phút)"
              type="number"
              value={cookTime}
              onChange={(e) => setCookTime(Number(e.target.value))}
              min={1}
            />
            <Input
              label="Khẩu phần (Người ăn)"
              type="number"
              value={servings}
              onChange={(e) => setServings(Number(e.target.value))}
              min={1}
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <label className="block space-y-1.5 text-xs font-semibold uppercase tracking-wider text-content-secondary">
              Ảnh công thức (không bắt buộc, tối đa 5 MB)
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp,image/avif"
                onChange={(event) => setImageFile(event.target.files?.[0] ?? null)}
                className="mt-1 block w-full rounded-lg border border-slate-300 p-2 text-sm font-normal normal-case"
              />
            </label>
            <Input
              label="Mô tả ảnh"
              value={imageAltText}
              maxLength={200}
              onChange={(event) => setImageAltText(event.target.value)}
              helperText={imageFile?.name}
            />
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-6">
          <label className="block space-y-2">
            <span className="text-xs font-semibold uppercase tracking-wider text-content-secondary">
              Hướng dẫn tổng quát (không bắt buộc)
            </span>
            <textarea
              rows={4}
              value={instructions}
              onChange={(event) => setInstructions(event.target.value)}
              className="w-full rounded-lg border border-slate-300 bg-white p-3 text-sm text-content-primary focus:border-brand focus:outline-none"
            />
          </label>
        </div>

        {/* SECTION 2: INGREDIENTS BUILDER */}
        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-slate-100">
            <h2 className="text-base font-bold text-content-primary">
              2. Danh sách nguyên liệu
            </h2>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={addIngredientRow}
              leftIcon={<Plus className="w-3.5 h-3.5" />}
            >
              Thêm nguyên liệu
            </Button>
          </div>

          <div className="space-y-3">
            {ingredients.map((ing, idx) => (
              <div
                key={idx}
                className="grid grid-cols-1 items-start gap-3 sm:grid-cols-[minmax(0,1fr)_7rem_10rem_auto]"
              >
                <div>
                  <Input
                    placeholder="Tên nguyên liệu (VD: Thịt bò, Hành tây...)"
                    value={ing.name}
                    onChange={(e) => {
                      const updated = [...ingredients];
                      updated[idx].name = e.target.value;
                      setIngredients(updated);
                    }}
                  />
                </div>
                <div>
                  <Input
                    type="text"
                    inputMode="decimal"
                    placeholder="Số lượng"
                    value={ing.quantity}
                    onChange={(e) => {
                      setIngredients((current) =>
                        current.map((item, index) =>
                          index === idx ? { ...item, quantity: e.target.value } : item,
                        ),
                      );
                    }}
                  />
                </div>
                <div>
                  <IngredientUnitField
                    value={ing.unit}
                    onChange={(value) => {
                      setIngredients((current) =>
                        current.map((item, index) =>
                          index === idx ? { ...item, unit: value } : item,
                        ),
                      );
                    }}
                  />
                </div>
                {ingredients.length > 1 && (
                  <button
                    type="button"
                    onClick={() => removeIngredientRow(idx)}
                    className="p-2 text-slate-400 hover:text-rose-600 transition-colors"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* SECTION 3: STEPS BUILDER */}
        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-slate-100">
            <h2 className="text-base font-bold text-content-primary">
              3. Các bước thực hiện
            </h2>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={addStepRow}
              leftIcon={<Plus className="w-3.5 h-3.5" />}
            >
              Thêm bước thực hiện
            </Button>
          </div>

          <div className="space-y-4">
            {steps.map((step, idx) => (
              <div key={idx} className="p-4 border border-slate-200 rounded-lg bg-slate-50/50 space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold text-brand uppercase">
                    Bước {idx + 1}
                  </span>
                  {steps.length > 1 && (
                    <button
                      type="button"
                      onClick={() => removeStepRow(idx)}
                      className="text-xs text-rose-600 hover:underline flex items-center gap-1"
                    >
                      <Trash2 className="w-3.5 h-3.5" /> Xóa bước này
                    </button>
                  )}
                </div>
                <textarea
                  rows={2}
                  placeholder={`Mô tả chi tiết các thao tác trong bước ${idx + 1}...`}
                  value={step.description}
                  onChange={(e) => {
                    const updated = [...steps];
                    updated[idx].description = e.target.value;
                    setSteps(updated);
                  }}
                  className="w-full rounded-lg border border-slate-300 bg-white p-3 text-sm focus:border-brand focus:outline-none"
                />
              </div>
            ))}
          </div>
        </div>

        {/* FORM ACTIONS */}
        <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-200">
          <Link href="/dashboard/recipes">
            <Button variant="ghost" size="md">
              Hủy bỏ
            </Button>
          </Link>
          <Button
            type="submit"
            variant="primary"
            size="lg"
            isLoading={isSubmitting}
          disabled={Boolean(createdRecipeId)}
          >
            Đăng công thức bài viết
          </Button>
        </div>
      </form>
    </div>
  );
}