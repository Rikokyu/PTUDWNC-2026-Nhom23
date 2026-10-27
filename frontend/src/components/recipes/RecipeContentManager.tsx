"use client";

import { FormEvent, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowDown, ArrowUp, ImagePlus, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/legacy-ui/Button";
import { ErrorState } from "@/components/legacy-ui/ErrorState";
import { Input } from "@/components/legacy-ui/Input";
import { RecipeCardSkeleton } from "@/components/legacy-ui/LoadingState";
import { recipeApi } from "@/lib/api/recipes";
import { resolveApiAssetUrl } from "@/lib/api/axios";
import { getApiErrorMessage } from "@/lib/api/error";
import { IngredientUnitField } from "@/components/recipes/IngredientUnitField";
import { isNonNegativeIngredientQuantity } from "@/lib/recipeIngredient";
import { useAuthStore } from "@/store/useAuthStore";
import type {
  RecipeApiDetailDto,
  RecipeApiIngredientDto,
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
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [newIngredient, setNewIngredient] = useState<IngredientFields>(
    ingredientFields(),
  );
  const [newStep, setNewStep] = useState<StepFields>(stepFields());
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [altText, setAltText] = useState("");

  const queryKey = ["recipe-management", recipeId, isAdmin] as const;
  const recipeQuery = useQuery({
    queryKey,
    queryFn: () => recipeApi.getManagedById(recipeId, isAdmin),
    enabled: Boolean(accessToken),
  });

  useEffect(() => {
    if (!accessToken) {
      router.replace("/auth/login");
    }
  }, [accessToken, router]);

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
    if (!["image/jpeg", "image/png", "image/webp", "image/avif"].includes(imageFile.type)) {
      setError("Ảnh phải có định dạng JPEG, PNG, WebP hoặc AVIF.");
      return;
    }
    if (imageFile.size > 5 * 1024 * 1024) {
      setError("Kích thước ảnh không được vượt quá 5 MB.");
      return;
    }

    const succeeded = await runMutation(
      () => recipeApi.uploadImage(recipeId, imageFile, altText),
      "Đã tải ảnh lên.",
    );
    if (!succeeded) return;
    setImageFile(null);
    setAltText("");
    form.reset();
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

  return (
    <main className="mx-auto max-w-5xl space-y-8">
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-5">
        <div className="space-y-1">
          <Link href="/dashboard/recipes" className="text-sm font-semibold text-brand hover:underline">
            ← Công thức của tôi
          </Link>
          <h1 className="text-2xl font-extrabold text-content-primary">
            Quản lý nội dung: {recipe.title}
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
          {recipe.status === 0 && (
            <Button
              size="sm"
              disabled={busy}
              onClick={() => void changeStatus("publish")}
            >
              Xuất bản
            </Button>
          )}
          {recipe.status === 1 && (
            <Button
              variant="outline"
              size="sm"
              disabled={busy}
              onClick={() => void changeStatus("unpublish")}
            >
              Hủy xuất bản
            </Button>
          )}
          {recipe.status !== 2 && (
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
        </div>
      </div>

      {error && <ErrorState title="Không thể lưu thay đổi" message={error} />}
      {success && (
        <p role="status" className="rounded-lg border border-emerald-300 bg-emerald-50 px-4 py-3 text-sm text-emerald-900">
          {success}
        </p>
      )}

      <section className="space-y-5 rounded-xl border border-slate-200 bg-white p-5">
        <header>
          <h2 className="text-xl font-bold">Ảnh công thức</h2>
          <p className="mt-1 text-sm text-content-secondary">
            Ảnh được lưu trên local file storage hiện cấu hình cho backend.
          </p>
        </header>
        <form onSubmit={uploadImage} className="grid gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
          <label className="block space-y-1.5 text-xs font-semibold text-content-secondary">
            Chọn ảnh (JPEG, PNG, WebP, AVIF; tối đa 5 MB)
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp,image/avif"
              onChange={(event) => setImageFile(event.target.files?.[0] ?? null)}
              className="block w-full rounded-lg border border-slate-300 p-2 text-sm font-normal"
            />
          </label>
          <Input
            label="Mô tả ảnh"
            value={altText}
            onChange={(event) => setAltText(event.target.value)}
            maxLength={200}
          />
          <Button type="submit" isLoading={busy} leftIcon={<ImagePlus className="h-4 w-4" />}>
            Tải ảnh lên
          </Button>
        </form>
        {recipe.images.length === 0 ? (
          <p className="text-sm text-content-secondary">Chưa có ảnh cho công thức này.</p>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {recipe.images.map((image) => (
              <article key={image.id} className="overflow-hidden rounded-lg border border-slate-200">
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

      <section className="space-y-5 rounded-xl border border-slate-200 bg-white p-5">
        <header>
          <h2 className="text-xl font-bold">Nguyên liệu ({recipe.ingredients.length})</h2>
          <p className="mt-1 text-sm text-content-secondary">
            Số lượng là chuỗi tự do, ví dụ “1/2” hoặc “vừa đủ”.
          </p>
        </header>
        <form onSubmit={addIngredient} className="grid gap-3 rounded-lg bg-slate-50 p-4 sm:grid-cols-2">
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

      <section className="space-y-5 rounded-xl border border-slate-200 bg-white p-5">
        <header>
          <h2 className="text-xl font-bold">Các bước ({recipe.steps.length})</h2>
          <p className="mt-1 text-sm text-content-secondary">
            Thứ tự được backend đánh lại liên tục sau khi thêm, xóa hoặc sắp xếp.
          </p>
        </header>
        <form onSubmit={addStep} className="space-y-3 rounded-lg bg-slate-50 p-4">
          <Input label="Tiêu đề bước" maxLength={200} value={newStep.title} onChange={(event) => setNewStep({ ...newStep, title: event.target.value })} />
          <label className="block space-y-1.5 text-xs font-semibold text-content-secondary">
            Mô tả (bắt buộc, tối đa 2000 ký tự)
            <textarea
              required
              maxLength={2000}
              value={newStep.description}
              onChange={(event) => setNewStep({ ...newStep, description: event.target.value })}
              className="w-full rounded-lg border border-slate-300 bg-white p-3 text-sm font-normal text-content-primary"
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
    </main>
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
      className="grid gap-3 rounded-lg border border-slate-200 p-4 sm:grid-cols-2"
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
      className="space-y-3 rounded-lg border border-slate-200 p-4"
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
          className="w-full rounded-lg border border-slate-300 bg-white p-3 text-sm font-normal text-content-primary"
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
