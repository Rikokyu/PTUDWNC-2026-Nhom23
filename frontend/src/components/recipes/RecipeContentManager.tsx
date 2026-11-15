"use client";

import { FormEvent, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowDown, ArrowUp, ImagePlus, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { ErrorState } from "@/components/ui/ErrorState";
import { SuccessNotice } from "@/components/ui/SuccessNotice";
import { Input } from "@/components/ui/Input";
import { RecipeCardSkeleton } from "@/components/ui/LoadingState";
import { recipeApi } from "@/lib/api/recipes";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import {
  MAX_RECIPE_IMAGE_BYTES,
  normalizeImageFile,
} from "@/lib/imageFile";
import { getApiErrorMessage } from "@/lib/api/error";
import { IngredientUnitField } from "@/components/recipes/IngredientUnitField";
import { isNonNegativeIngredientQuantity } from "@/lib/recipeIngredient";
import { useAuthStore } from "@/store/useAuthStore";
import { useCategories } from "@/hooks/useCategories";
import type {
  ApiDifficultyLevel,
  RecipeApiDetailDto,
  RecipeApiIngredientDto,
  RecipeApiNutritionDto,
  RecipeApiStepDto,
} from "@/types/api";

interface IngredientFields {
  name: string;
  quantity: string;
  unit: string;
  notes: string;
}

interface StepFields {
  title: string;
  description: string;
  timerMinutes: string;
  imageUrl: string;
}

interface RecipeFields {
  title: string;
  description: string;
  instructions: string;
  categoryId: string;
  prepTimeMinutes: string;
  cookTimeMinutes: string;
  servings: string;
  difficulty: ApiDifficultyLevel;
}

type NutritionFields = Record<keyof RecipeApiNutritionDto, string>;

const emptyNutrition: NutritionFields = {
  calories: "",
  protein: "",
  carbs: "",
  fat: "",
  fiber: "",
  sodium: "",
};

function nutritionFields(recipe: RecipeApiDetailDto): NutritionFields {
  return {
    calories: recipe.nutrition?.calories?.toString() ?? "",
    protein: recipe.nutrition?.protein?.toString() ?? "",
    carbs: recipe.nutrition?.carbs?.toString() ?? "",
    fat: recipe.nutrition?.fat?.toString() ?? "",
    fiber: recipe.nutrition?.fiber?.toString() ?? "",
    sodium: recipe.nutrition?.sodium?.toString() ?? "",
  };
}

function nutritionRequest(fields: NutritionFields): RecipeApiNutritionDto {
  const parse = (value: string) => value.trim() ? Number(value) : null;
  return {
    calories: parse(fields.calories),
    protein: parse(fields.protein),
    carbs: parse(fields.carbs),
    fat: parse(fields.fat),
    fiber: parse(fields.fiber),
    sodium: parse(fields.sodium),
  };
}

function ingredientFields(
  ingredient?: RecipeApiIngredientDto,
): IngredientFields {
  return {
    name: ingredient?.name ?? "",
    quantity: ingredient?.quantity ?? "",
    unit: ingredient?.unit ?? "",
    notes: ingredient?.notes ?? "",
  };
}

function stepFields(step?: RecipeApiStepDto): StepFields {
  return {
    title: step?.title ?? "",
    description: step?.description ?? "",
    timerMinutes: step?.timerMinutes?.toString() ?? "",
    imageUrl: step?.imageUrl ?? "",
  };
}

function toIngredientRequest(fields: IngredientFields) {
  return {
    name: fields.name.trim(),
    quantity: fields.quantity.trim() || null,
    unit: fields.unit.trim() || null,
    notes: fields.notes.trim() || null,
  };
}

function toStepRequest(fields: StepFields) {
  return {
    title: fields.title.trim(),
    description: fields.description.trim(),
    timerMinutes: fields.timerMinutes
      ? Number.parseInt(fields.timerMinutes, 10)
      : null,
    imageUrl: fields.imageUrl.trim() || null,
  };
}

export function RecipeContentManager({ recipeId }: { recipeId: string }) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const accessToken = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.role.toLocaleLowerCase() === "admin";
  const categoriesQuery = useCategories();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [isUploadingImage, setIsUploadingImage] = useState(false);
  const [newIngredient, setNewIngredient] = useState<IngredientFields>(
    ingredientFields(),
  );
  const [newStep, setNewStep] = useState<StepFields>(stepFields());
  const [recipeFields, setRecipeFields] = useState<RecipeFields>({
    title: "",
    description: "",
    instructions: "",
    categoryId: "",
    prepTimeMinutes: "",
    cookTimeMinutes: "",
    servings: "",
    difficulty: 1,
  });
  const [nutrition, setNutrition] = useState<NutritionFields>(emptyNutrition);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);
  const [altText, setAltText] = useState("");

  const queryKey = ["recipe-management", recipeId, isAdmin] as const;
  const recipeQuery = useQuery({
    queryKey,
    queryFn: () => recipeApi.getManagedById(recipeId, isAdmin),
    enabled: Boolean(accessToken),
  });

  useEffect(() => {
    const recipe = recipeQuery.data;
    if (!recipe) return;
    setRecipeFields({
      title: recipe.title,
      description: recipe.description,
      instructions: recipe.instructions,
      categoryId: recipe.category.id,
      prepTimeMinutes: recipe.prepTimeMinutes.toString(),
      cookTimeMinutes: recipe.cookTimeMinutes.toString(),
      servings: recipe.servings.toString(),
      difficulty: recipe.difficulty,
    });
    setNutrition(nutritionFields(recipe));
  }, [recipeQuery.data]);

  useEffect(() => {
    if (!accessToken) {
      router.replace("/auth/login");
    }
  }, [accessToken, router]);

  useEffect(() => {
    if (!imageFile) {
      setImagePreviewUrl(null);
      return;
    }

    const previewUrl = URL.createObjectURL(imageFile);
    setImagePreviewUrl(previewUrl);
    return () => URL.revokeObjectURL(previewUrl);
  }, [imageFile]);

  async function runMutation(
    operation: () => Promise<unknown>,
    successMessage: string,
  ): Promise<boolean> {
    setError(null);
    setSuccess(null);
    setBusy(true);
    try {
      await operation();
      await queryClient.invalidateQueries({ queryKey });
      await queryClient.invalidateQueries({ queryKey: ["managed-recipes"] });
      await queryClient.invalidateQueries({ queryKey: ["recipes"] });
      setSuccess(successMessage);
      return true;
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
      return false;
    } finally {
      setBusy(false);
    }
  }

  async function addIngredient(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!isNonNegativeIngredientQuantity(newIngredient.quantity)) {
      setError("Số lượng nguyên liệu không được nhỏ hơn 0.");
      setSuccess(null);
      return;
    }

    const succeeded = await runMutation(
      () => recipeApi.addIngredient(recipeId, toIngredientRequest(newIngredient)),
      "Đã thêm nguyên liệu.",
    );
    if (succeeded) setNewIngredient(ingredientFields());
  }

  async function addStep(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const succeeded = await runMutation(
      () => recipeApi.addStep(recipeId, toStepRequest(newStep)),
      "Đã thêm bước thực hiện.",
    );
    if (succeeded) setNewStep(stepFields());
  }

  async function uploadImage(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    if (!imageFile) {
      setError("Hãy chọn một ảnh trước khi tải lên.");
      return;
    }
    if (imageFile.size > MAX_RECIPE_IMAGE_BYTES) {
      setError("Kích thước ảnh không được vượt quá 5 MB.");
      return;
    }

    setIsUploadingImage(true);
    try {
      const succeeded = await runMutation(
        () => recipeApi.uploadImage(recipeId, imageFile, altText),
        "Đã tải ảnh lên.",
      );
      if (!succeeded) return;
      setImageFile(null);
      setAltText("");
      form.reset();
    } finally {
      setIsUploadingImage(false);
    }
  }

  async function changeStatus(action: "publish" | "unpublish" | "archive") {
    const confirmations = {
      publish: null,
      unpublish: "Hủy xuất bản công thức này? Công thức sẽ không còn hiển thị công khai.",
      archive: "Lưu trữ công thức này? Công thức sẽ không còn hiển thị công khai.",
    };
    const confirmation = confirmations[action];
    if (confirmation && !window.confirm(confirmation)) return;

    await runMutation(
      () => recipeApi.setStatus(recipeId, action),
      action === "publish"
        ? "Đã xuất bản công thức."
        : action === "unpublish"
          ? "Đã chuyển công thức về bản nháp."
          : "Đã lưu trữ công thức.",
    );
  }

  function updateRecipe(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const recipe = recipeQuery.data;
    if (!recipe) return;

    const prepTimeMinutes = Number(recipeFields.prepTimeMinutes);
    const cookTimeMinutes = Number(recipeFields.cookTimeMinutes);
    const servings = Number(recipeFields.servings);
    if (!recipeFields.title.trim() || recipeFields.title.trim().length > 200
      || !recipeFields.description.trim()
      || !categoriesQuery.data?.some((category) => category.id === recipeFields.categoryId)
      || !Number.isInteger(prepTimeMinutes) || prepTimeMinutes < 1
      || !Number.isInteger(cookTimeMinutes) || cookTimeMinutes < 0
      || !Number.isInteger(servings) || servings < 1) {
      setError("Hãy kiểm tra tên, mô tả, danh mục, thời gian và khẩu phần.");
      setSuccess(null);
      return;
    }

    const nutritionValues = Object.values(nutrition)
      .filter((value) => value.trim() !== "")
      .map(Number);
    if (nutritionValues.some((value) =>
      !Number.isFinite(value) || value < 0 || value > 99999999.99)) {
      setError("Giá trị dinh dưỡng phải là số không âm và không vượt quá giới hạn cho phép.");
      setSuccess(null);
      return;
    }

    void runMutation(
      () => recipeApi.update(recipeId, {
        title: recipeFields.title.trim(),
        description: recipeFields.description.trim(),
        instructions: recipeFields.instructions.trim(),
        categoryId: recipeFields.categoryId,
        prepTimeMinutes,
        cookTimeMinutes,
        servings,
        difficulty: recipeFields.difficulty,
        rowVersion: recipe.rowVersion,
        nutrition: nutritionRequest(nutrition),
      }),
      "Đã cập nhật thông tin và dinh dưỡng.",
    );
  }

  async function deleteRecipe() {
    if (!window.confirm("Xóa mềm công thức này? Công thức sẽ bị ẩn nhưng dữ liệu và ảnh vẫn được giữ lại.")) {
      return;
    }
    const succeeded = await runMutation(
      () => recipeApi.deleteRecipe(recipeId),
      "Đã xóa mềm công thức.",
    );
    if (succeeded) router.push("/dashboard/recipes");
  }

  if (!accessToken) {
    return <p className="py-12 text-center text-sm text-content-secondary">Đang chuyển đến trang đăng nhập...</p>;
  }

  if (recipeQuery.isLoading) {
    return <RecipeCardSkeleton />;
  }

  if (recipeQuery.isError) {
    return (
      <ErrorState
        title="Không tải được công thức cần quản lý"
        message={getApiErrorMessage(recipeQuery.error)}
        onRetry={() => void recipeQuery.refetch()}
      />
    );
  }

  const recipe = recipeQuery.data;
  if (!recipe) {
    return <ErrorState title="Không tìm thấy công thức" />;
  }
  const canManageRecipe = isAdmin || recipe.author?.id === user?.id;

  return (
    <div className="mx-auto max-w-5xl space-y-8">
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-stone-200 pb-6">
        <div className="space-y-1">
          <Link href="/dashboard/recipes" className="text-sm font-semibold text-brand hover:underline">
            ← Công thức của tôi
          </Link>
          <h1 className="font-display text-3xl font-bold text-content-primary">
            {recipe.title}
          </h1>
          <p className="text-sm text-content-secondary">
            Quản lý ảnh, nguyên liệu và các bước thực hiện.
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-content-secondary">
            {recipe.status === 0
              ? "Bản nháp"
              : recipe.status === 1
                ? "Đã đăng"
                : "Đã lưu trữ"}
          </span>
          {canManageRecipe && recipe.status === 0 && (
            <Button
              size="sm"
              disabled={busy}
              onClick={() => void changeStatus("publish")}
            >
              Xuất bản
            </Button>
          )}
          {canManageRecipe && recipe.status === 1 && (
            <Button
              variant="outline"
              size="sm"
              disabled={busy}
              onClick={() => void changeStatus("unpublish")}
            >
              Hủy xuất bản
            </Button>
          )}
          {canManageRecipe && recipe.status !== 2 && (
            <Button
              variant="outline"
              size="sm"
              disabled={busy}
              onClick={() => void changeStatus("archive")}
            >
              Lưu trữ
            </Button>
          )}
          <Link href={`/recipes/${encodeURIComponent(recipe.slug)}`}>
            <Button variant="outline">Xem công thức</Button>
          </Link>
          {canManageRecipe && (
            <Button variant="danger" size="sm" disabled={busy} onClick={() => void deleteRecipe()}>
              Xóa công thức
            </Button>
          )}
        </div>
      </div>

      {error && <ErrorState title="Không thể lưu thay đổi" message={error} />}
      {success && <SuccessNotice title="Đã cập nhật công thức" message={success} />}

      {canManageRecipe && (
      <form onSubmit={updateRecipe} className="space-y-5 rounded-2xl border border-stone-200 bg-surface p-5 shadow-subtle">
        <header>
          <h2 className="text-xl font-bold">Thông tin công thức</h2>
        </header>
        {categoriesQuery.isError && (
          <ErrorState
            title="Không tải được danh mục"
            message={getApiErrorMessage(categoriesQuery.error)}
            onRetry={() => void categoriesQuery.refetch()}
          />
        )}
        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Tên món ăn"
            required
            maxLength={200}
            value={recipeFields.title}
            onChange={(event) => setRecipeFields({ ...recipeFields, title: event.target.value })}
          />
          <label className="block space-y-1.5 text-xs font-semibold text-content-secondary">
            Danh mục
            <select
              required
              value={recipeFields.categoryId}
              onChange={(event) => setRecipeFields({ ...recipeFields, categoryId: event.target.value })}
              disabled={categoriesQuery.isLoading || categoriesQuery.isError}
              className="min-h-11 w-full rounded-xl border border-stone-300 bg-surface px-3 py-2.5 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
            >
              {(categoriesQuery.data ?? []).map((category) => (
                <option key={category.id} value={category.id}>{category.name}</option>
              ))}
            </select>
          </label>
          <label className="block space-y-1.5 text-xs font-semibold text-content-secondary sm:col-span-2">
            Mô tả
            <textarea
              required
              value={recipeFields.description}
              onChange={(event) => setRecipeFields({ ...recipeFields, description: event.target.value })}
              className="w-full rounded-xl border border-stone-300 bg-surface p-3 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
              rows={3}
            />
          </label>
          <Input
            label="Thời gian chuẩn bị (phút)"
            type="number"
            min={1}
            step={1}
            required
            value={recipeFields.prepTimeMinutes}
            onChange={(event) => setRecipeFields({ ...recipeFields, prepTimeMinutes: event.target.value })}
          />
          <Input
            label="Thời gian nấu (phút)"
            type="number"
            min={0}
            step={1}
            required
            value={recipeFields.cookTimeMinutes}
            onChange={(event) => setRecipeFields({ ...recipeFields, cookTimeMinutes: event.target.value })}
          />
          <Input
            label="Khẩu phần"
            type="number"
            min={1}
            step={1}
            required
            value={recipeFields.servings}
            onChange={(event) => setRecipeFields({ ...recipeFields, servings: event.target.value })}
          />
          <label className="block space-y-1.5 text-xs font-semibold text-content-secondary">
            Độ khó
            <select
              value={recipeFields.difficulty}
              onChange={(event) => setRecipeFields({
                ...recipeFields,
                difficulty: Number(event.target.value) as ApiDifficultyLevel,
              })}
              className="min-h-11 w-full rounded-xl border border-stone-300 bg-surface px-3 py-2.5 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
            >
              <option value={1}>Dễ</option>
              <option value={2}>Trung bình</option>
              <option value={3}>Khó</option>
              <option value={4}>Chuyên gia</option>
            </select>
          </label>
          <label className="block space-y-1.5 text-xs font-semibold text-content-secondary sm:col-span-2">
            Ghi chú chung
            <textarea
              value={recipeFields.instructions}
              onChange={(event) => setRecipeFields({ ...recipeFields, instructions: event.target.value })}
              className="w-full rounded-xl border border-stone-300 bg-surface p-3 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
              rows={3}
            />
          </label>
        </div>
        <div>
          <h3 className="font-semibold">Dinh dưỡng (không bắt buộc)</h3>
          <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {([
              ["calories", "Năng lượng (kcal)"],
              ["protein", "Protein (g)"],
              ["carbs", "Carbohydrate (g)"],
              ["fat", "Chất béo (g)"],
              ["fiber", "Chất xơ (g)"],
              ["sodium", "Natri (mg)"],
            ] as const).map(([field, label]) => (
              <Input
                key={field}
                label={label}
                type="number"
                min={0}
                max={99999999.99}
                step="0.01"
                value={nutrition[field]}
                onChange={(event) => setNutrition({
                  ...nutrition,
                  [field]: event.target.value,
                })}
              />
            ))}
          </div>
        </div>
        <Button
          type="submit"
          disabled={busy || categoriesQuery.isLoading || categoriesQuery.isError}
          isLoading={busy}
        >
          Lưu thông tin
        </Button>
      </form>
      )}

      {canManageRecipe && (
      <section className="space-y-5 rounded-2xl border border-stone-200 bg-surface p-5 shadow-subtle">
        <header>
          <h2 className="text-xl font-bold">Ảnh công thức</h2>
        </header>
        <form onSubmit={uploadImage} className="grid gap-3 sm:grid-cols-[auto_1fr_auto] sm:items-end">
          <div>
            <label htmlFor="recipe-content-image-upload" className={`inline-flex cursor-pointer items-center gap-2 rounded-xl border border-brand bg-brand px-4 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-brand-hover focus-within:ring-2 focus-within:ring-brand focus-within:ring-offset-2 ${busy ? "pointer-events-none opacity-50" : ""}`}>
              <ImagePlus className="h-4 w-4" aria-hidden="true" />
              {imageFile ? "Đổi ảnh" : "Thêm ảnh"}
            </label>
            <input
              id="recipe-content-image-upload"
              type="file"
              accept=".jpg,.jpeg,.png,.webp,.avif,image/jpeg,image/png,image/webp,image/avif"
              disabled={busy}
              onChange={(event) => {
                const input = event.currentTarget;
                const selectedFile = input.files?.[0] ?? null;
                input.value = "";
                setError(null);
                setSuccess(null);
                if (!selectedFile) return;
                const file = normalizeImageFile(selectedFile);
                if (!file) {
                  setImageFile(null);
                  setError("Ảnh phải có định dạng JPEG, PNG, WebP hoặc AVIF.");
                  return;
                }
                if (file && file.size > MAX_RECIPE_IMAGE_BYTES) {
                  setImageFile(null);
                  setError("Kích thước ảnh không được vượt quá 5 MB.");
                  return;
                }
                setImageFile(file);
              }}
              className="sr-only"
            />
          </div>
          <Input
            label="Mô tả ảnh"
            value={altText}
            onChange={(event) => setAltText(event.target.value)}
            maxLength={200}
          />
          <Button type="submit" disabled={busy && !isUploadingImage} isLoading={isUploadingImage} leftIcon={<ImagePlus className="h-4 w-4" />}>
            {isUploadingImage ? "Đang tải ảnh lên…" : "Tải ảnh lên"}
          </Button>
        </form>
        {imageFile && (
          <p role="status" className="truncate text-sm text-content-secondary">Đã chọn: {imageFile.name}</p>
        )}
        {imagePreviewUrl && imageFile && (
          <div className="max-w-md overflow-hidden rounded-xl border border-stone-200 bg-surface-cream">
            <img
              src={imagePreviewUrl}
              alt={altText || `Ảnh xem trước ${recipe.title}`}
              className="aspect-[4/3] w-full object-cover"
            />
          </div>
        )}
        {recipe.images.length === 0 && !imageFile && (
          <p className="text-sm text-content-secondary">Chưa có ảnh được tải lên cho công thức này.</p>
        )}
        {recipe.images.length > 0 && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {recipe.images.map((image) => (
              <article key={image.id} className="overflow-hidden rounded-xl border border-stone-200 bg-surface-cream">
                <img
                  src={resolveApiAssetUrl(image.mediumUrl ?? image.originalUrl) ?? ""}
                  alt={image.altText ?? recipe.title}
                  className="aspect-[4/3] w-full bg-slate-100 object-cover"
                />
                <div className="space-y-3 p-3">
                  <p className="truncate text-sm text-content-secondary">
                    {image.altText || "Chưa có mô tả"}
                    {image.isPrimary && (
                      <span className="ml-2 font-semibold text-brand">Ảnh chính</span>
                    )}
                  </p>
                  <div className="flex flex-wrap gap-2">
                    {!image.isPrimary && (
                      <Button
                        variant="outline"
                        size="sm"
                        disabled={busy}
                        onClick={() =>
                          void runMutation(
                            () => recipeApi.setPrimaryImage(recipe.id, image.id),
                            "Đã đặt ảnh chính.",
                          )
                        }
                      >
                        Đặt làm ảnh chính
                      </Button>
                    )}
                    <Button
                      variant="danger"
                      size="sm"
                      disabled={busy}
                      leftIcon={<Trash2 className="h-3.5 w-3.5" />}
                      onClick={() =>
                        void runMutation(
                          () => recipeApi.deleteImage(recipe.id, image.id),
                          "Đã xóa ảnh; tệp được đưa vào hàng đợi dọn dẹp.",
                        )
                      }
                    >
                      Xóa
                    </Button>
                  </div>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
      )}

      {canManageRecipe && (
      <section className="space-y-5 rounded-2xl border border-stone-200 bg-surface p-5 shadow-subtle">
        <header>
          <h2 className="text-xl font-bold">Nguyên liệu ({recipe.ingredients.length})</h2>
          <p className="mt-1 text-sm text-content-secondary">
            Số lượng là chuỗi tự do, ví dụ “1/2” hoặc “vừa đủ”.
          </p>
        </header>
        <form onSubmit={addIngredient} className="grid gap-3 rounded-xl bg-surface-cream p-4 sm:grid-cols-2">
          <Input label="Tên nguyên liệu" required maxLength={200} value={newIngredient.name} onChange={(event) => setNewIngredient({ ...newIngredient, name: event.target.value })} />
          <Input label="Số lượng" maxLength={50} value={newIngredient.quantity} onChange={(event) => setNewIngredient({ ...newIngredient, quantity: event.target.value })} />
          <IngredientUnitField
            value={newIngredient.unit}
            onChange={(unit) => setNewIngredient({ ...newIngredient, unit })}
          />
          <Input label="Ghi chú" maxLength={500} value={newIngredient.notes} onChange={(event) => setNewIngredient({ ...newIngredient, notes: event.target.value })} />
          <Button type="submit" isLoading={busy} leftIcon={<Plus className="h-4 w-4" />} className="sm:col-span-2">
            Thêm nguyên liệu
          </Button>
        </form>
        <div className="space-y-3">
          {recipe.ingredients.map((ingredient) => (
            <IngredientEditor
              key={ingredient.id}
              recipeId={recipe.id}
              ingredient={ingredient}
              busy={busy}
              runMutation={runMutation}
              onValidationError={(message) => {
                setError(message);
                setSuccess(null);
              }}
            />
          ))}
        </div>
      </section>
      )}

      {canManageRecipe && (
      <section className="space-y-5 rounded-2xl border border-stone-200 bg-surface p-5 shadow-subtle">
        <header>
          <h2 className="text-xl font-bold">Các bước ({recipe.steps.length})</h2>
          <p className="mt-1 text-sm text-content-secondary">
            Thứ tự được backend đánh lại liên tục sau khi thêm, xóa hoặc sắp xếp.
          </p>
        </header>
        <form onSubmit={addStep} className="space-y-3 rounded-xl bg-surface-cream p-4">
          <Input label="Tiêu đề bước" maxLength={200} value={newStep.title} onChange={(event) => setNewStep({ ...newStep, title: event.target.value })} />
          <label className="block space-y-1.5 text-xs font-semibold text-content-secondary">
            Mô tả (bắt buộc, tối đa 2000 ký tự)
            <textarea
              required
              maxLength={2000}
              value={newStep.description}
              onChange={(event) => setNewStep({ ...newStep, description: event.target.value })}
              className="w-full rounded-xl border border-stone-300 bg-surface p-3 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
              rows={3}
            />
          </label>
          <div className="grid gap-3 sm:grid-cols-2">
            <Input label="Thời gian (phút)" type="number" min={0} value={newStep.timerMinutes} onChange={(event) => setNewStep({ ...newStep, timerMinutes: event.target.value })} />
            <Input label="URL ảnh bước (không bắt buộc)" value={newStep.imageUrl} onChange={(event) => setNewStep({ ...newStep, imageUrl: event.target.value })} />
          </div>
          <Button type="submit" isLoading={busy} leftIcon={<Plus className="h-4 w-4" />}>
            Thêm bước
          </Button>
        </form>
        <div className="space-y-3">
          {recipe.steps.map((step, index) => (
            <StepEditor
              key={step.id}
              recipe={recipe}
              step={step}
              index={index}
              busy={busy}
              runMutation={runMutation}
            />
          ))}
        </div>
      </section>
      )}
    </div>
  );
}

function IngredientEditor({
  recipeId,
  ingredient,
  busy,
  runMutation,
  onValidationError,
}: {
  recipeId: string;
  ingredient: RecipeApiIngredientDto;
  busy: boolean;
  runMutation: (operation: () => Promise<unknown>, message: string) => Promise<boolean>;
  onValidationError: (message: string) => void;
}) {
  const [fields, setFields] = useState(() => ingredientFields(ingredient));
  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        if (!isNonNegativeIngredientQuantity(fields.quantity)) {
          onValidationError("Số lượng nguyên liệu không được nhỏ hơn 0.");
          return;
        }

        void runMutation(
          () => recipeApi.updateIngredient(recipeId, ingredient.id, toIngredientRequest(fields)),
          "Đã cập nhật nguyên liệu.",
        );
      }}
      className="grid gap-3 rounded-xl border border-stone-200 bg-surface-cream p-4 sm:grid-cols-2"
    >
      <Input label="Tên" required maxLength={200} value={fields.name} onChange={(event) => setFields({ ...fields, name: event.target.value })} />
      <Input label="Số lượng" maxLength={50} value={fields.quantity} onChange={(event) => setFields({ ...fields, quantity: event.target.value })} />
      <IngredientUnitField
        value={fields.unit}
        onChange={(unit) => setFields({ ...fields, unit })}
      />
      <Input label="Ghi chú" maxLength={500} value={fields.notes} onChange={(event) => setFields({ ...fields, notes: event.target.value })} />
      <div className="flex gap-2 sm:col-span-2">
        <Button type="submit" variant="outline" size="sm" disabled={busy}>Lưu</Button>
        <Button
          type="button"
          variant="danger"
          size="sm"
          disabled={busy}
          leftIcon={<Trash2 className="h-3.5 w-3.5" />}
          onClick={() =>
            void runMutation(
              () => recipeApi.deleteIngredient(recipeId, ingredient.id),
              "Đã xóa nguyên liệu.",
            )
          }
        >
          Xóa
        </Button>
      </div>
    </form>
  );
}

function StepEditor({
  recipe,
  step,
  index,
  busy,
  runMutation,
}: {
  recipe: RecipeApiDetailDto;
  step: RecipeApiStepDto;
  index: number;
  busy: boolean;
  runMutation: (operation: () => Promise<unknown>, message: string) => Promise<boolean>;
}) {
  const [fields, setFields] = useState(() => stepFields(step));

  async function move(direction: -1 | 1) {
    const reordered = [...recipe.steps];
    const nextIndex = index + direction;
    [reordered[index], reordered[nextIndex]] = [
      reordered[nextIndex],
      reordered[index],
    ];
    await runMutation(
      () => recipeApi.reorderSteps(recipe.id, reordered.map((item) => item.id)),
      "Đã cập nhật thứ tự các bước.",
    );
  }

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        void runMutation(
          () => recipeApi.updateStep(recipe.id, step.id, toStepRequest(fields)),
          "Đã cập nhật bước thực hiện.",
        );
      }}
      className="space-y-3 rounded-xl border border-stone-200 bg-surface-cream p-4"
    >
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="font-semibold">Bước {step.stepNumber}</h3>
        <div className="flex gap-2">
          <Button type="button" variant="outline" size="sm" disabled={busy || index === 0} aria-label="Chuyển bước lên" onClick={() => void move(-1)}>
            <ArrowUp className="h-4 w-4" />
          </Button>
          <Button type="button" variant="outline" size="sm" disabled={busy || index === recipe.steps.length - 1} aria-label="Chuyển bước xuống" onClick={() => void move(1)}>
            <ArrowDown className="h-4 w-4" />
          </Button>
        </div>
      </div>
      <Input label="Tiêu đề" maxLength={200} value={fields.title} onChange={(event) => setFields({ ...fields, title: event.target.value })} />
      <label className="block space-y-1.5 text-xs font-semibold text-content-secondary">
        Mô tả (tối đa 2000 ký tự)
        <textarea
          required
          maxLength={2000}
          value={fields.description}
          onChange={(event) => setFields({ ...fields, description: event.target.value })}
          className="w-full rounded-xl border border-stone-300 bg-surface p-3 text-sm font-normal text-content-primary focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/15"
          rows={3}
        />
      </label>
      <div className="grid gap-3 sm:grid-cols-2">
        <Input label="Thời gian (phút)" type="number" min={0} value={fields.timerMinutes} onChange={(event) => setFields({ ...fields, timerMinutes: event.target.value })} />
        <Input label="URL ảnh bước" value={fields.imageUrl} onChange={(event) => setFields({ ...fields, imageUrl: event.target.value })} />
      </div>
      <div className="flex gap-2">
        <Button type="submit" variant="outline" size="sm" disabled={busy}>Lưu</Button>
        <Button
          type="button"
          variant="danger"
          size="sm"
          disabled={busy}
          leftIcon={<Trash2 className="h-3.5 w-3.5" />}
          onClick={() =>
            void runMutation(
              () => recipeApi.deleteStep(recipe.id, step.id),
              "Đã xóa bước và đánh lại thứ tự.",
            )
          }
        >
          Xóa
        </Button>
      </div>
    </form>
  );
}
