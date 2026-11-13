"use client";

import React, { useEffect, useRef, useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { Input } from "@/components/ui/Input";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { ErrorState } from "@/components/ui/ErrorState";
import { Plus, Trash2, CheckCircle2, ImagePlus } from "lucide-react";
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
  const userId = useAuthStore((state) => state.user?.id);
  const categoriesQuery = useCategories();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submittedSuccess, setSubmittedSuccess] = useState(false);
  const [submitMode, setSubmitMode] = useState<"draft" | "publish">("draft");
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [createdRecipeId, setCreatedRecipeId] = useState<string | null>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [imageAltText, setImageAltText] = useState("");
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);
  const imageInputRef = useRef<HTMLInputElement>(null);
  const [draftReady, setDraftReady] = useState(false);
  const [draftSaved, setDraftSaved] = useState(false);
  const [draftSaveFailed, setDraftSaveFailed] = useState(false);
  const formTopRef = useRef<HTMLDivElement>(null);
  const createdRecipeRef = useRef(false);
  const draftKey = userId ? `culinary-recipe-create:${userId}` : null;

  // Form State
  const [title, setTitle] = useState("");
  const [summary, setSummary] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [difficulty, setDifficulty] = useState("Easy");
  const [prepTime, setPrepTime] = useState(15);
  const [cookTime, setCookTime] = useState(20);
  const [servings, setServings] = useState(4);
  const [instructions, setInstructions] = useState("");

  // Keep long text drafts per account. Image files cannot be restored by the browser.
  useEffect(() => {
    if (!draftKey) return;
    try {
      const saved = localStorage.getItem(draftKey);
      if (saved) {
        const draft = JSON.parse(saved);
        if (typeof draft.title === "string") setTitle(draft.title);
        if (typeof draft.summary === "string") setSummary(draft.summary);
        if (typeof draft.categoryId === "string") setCategoryId(draft.categoryId);
        if (["Easy", "Medium", "Hard"].includes(draft.difficulty)) setDifficulty(draft.difficulty);
        if (Number.isInteger(draft.prepTime)) setPrepTime(draft.prepTime);
        if (Number.isInteger(draft.cookTime)) setCookTime(draft.cookTime);
        if (Number.isInteger(draft.servings)) setServings(draft.servings);
        if (typeof draft.instructions === "string") setInstructions(draft.instructions);
        if (Array.isArray(draft.ingredients) && draft.ingredients.length) {
          const restored = draft.ingredients.filter((item: unknown) =>
            typeof item === "object" && item !== null
            && typeof (item as { name?: unknown }).name === "string"
            && typeof (item as { quantity?: unknown }).quantity === "string"
            && typeof (item as { unit?: unknown }).unit === "string");
          if (restored.length) setIngredients(restored.map((item: { id?: string; name: string; quantity: string; unit: string }) => ({
            ...item, id: typeof item.id === "string" ? item.id : crypto.randomUUID(),
          })));
        }
        if (Array.isArray(draft.steps) && draft.steps.length) {
          const restored = draft.steps.filter((item: unknown) =>
            typeof item === "object" && item !== null
            && typeof (item as { description?: unknown }).description === "string"
            && Number.isInteger((item as { durationMinutes?: unknown }).durationMinutes));
          if (restored.length) setSteps(restored.map((item: { id?: string; description: string; durationMinutes: number }) => ({
            ...item, id: typeof item.id === "string" ? item.id : crypto.randomUUID(),
          })));
        }
      }
    } catch {
      // A malformed local draft must not prevent creating a recipe.
    }
    setDraftReady(true);
  }, [draftKey]);

  useEffect(() => {
    if (!accessToken) {
      router.replace("/auth/login");
    }
  }, [accessToken, router]);

  useEffect(() => {
    if (draftReady && !categoryId && categoriesQuery.data?.length) {
      setCategoryId(categoriesQuery.data[0].id);
    }
  }, [categoryId, categoriesQuery.data, draftReady]);

  useEffect(() => {
    return () => {
      if (imagePreviewUrl) URL.revokeObjectURL(imagePreviewUrl);
    };
  }, [imagePreviewUrl]);

  const handleImageFileChange = (file: File | null) => {
    const supportedTypes = ["image/jpeg", "image/png", "image/webp", "image/avif"];
    if (file && !supportedTypes.includes(file.type)) {
      setImageError("Ảnh phải có định dạng JPEG, PNG, WebP hoặc AVIF.");
      file = null;
    } else if (file && file.size > 5 * 1024 * 1024) {
      setImageError("Kích thước ảnh không được vượt quá 5 MB.");
      file = null;
    } else {
      setImageError(null);
    }
    setImageFile(file);
    setImageAltText("");
    setImagePreviewUrl(file ? URL.createObjectURL(file) : null);
    if (!file && imageInputRef.current) imageInputRef.current.value = "";
  };

  // Dynamic Ingredients State
  const [ingredients, setIngredients] = useState([
    { id: "initial-ingredient", name: "", quantity: "", unit: "" },
  ]);

  // Dynamic Steps State
  const [steps, setSteps] = useState([
    { id: "initial-step", description: "", durationMinutes: 0 },
  ]);

  useEffect(() => {
    if (!draftReady || !draftKey || createdRecipeId) return;
    setDraftSaved(false);
    const timer = window.setTimeout(() => {
      if (createdRecipeRef.current) return;
      try {
        localStorage.setItem(draftKey, JSON.stringify({
          title, summary, categoryId, difficulty, prepTime, cookTime, servings,
          instructions, ingredients, steps,
        }));
        setDraftSaved(true);
        setDraftSaveFailed(false);
      } catch {
        setDraftSaved(false);
        setDraftSaveFailed(true);
      }
    }, 500);
    return () => window.clearTimeout(timer);
  }, [draftReady, draftKey, createdRecipeId, title, summary, categoryId, difficulty,
    prepTime, cookTime, servings, instructions, ingredients, steps]);

  const addIngredientRow = () => {
    setIngredients([...ingredients, { id: crypto.randomUUID(), name: "", quantity: "", unit: "" }]);
  };

  const removeIngredientRow = (index: number) => {
    setIngredients(ingredients.filter((_, i) => i !== index));
  };

  const addStepRow = () => {
    setSteps([...steps, { id: crypto.randomUUID(), description: "", durationMinutes: 0 }]);
  };

  const removeStepRow = (index: number) => {
    setSteps(steps.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const mode = (e.nativeEvent as SubmitEvent).submitter?.getAttribute("value") === "publish"
      ? "publish" : "draft";
    setSubmitMode(mode);
    setSubmitError(null);
    if (!accessToken) {
      router.replace("/auth/login");
      return;
    }
    const validIngredients = ingredients.filter((ingredient) =>
      ingredient.name.trim() || ingredient.quantity.trim() || ingredient.unit.trim());
    const validSteps = steps.filter((step) => step.description.trim());
    const fail = (message: string) => {
      setSubmitError(message);
      formTopRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    };
    if (!title.trim() || title.trim().length > 200) {
      fail("Tên món ăn cần có nội dung và không quá 200 ký tự.");
      return;
    }
    if (!summary.trim()) {
      fail("Hãy viết mô tả ngắn cho món ăn.");
      return;
    }
    if (!categoryId || !categoriesQuery.data?.some((category) => category.id === categoryId)) {
      fail("Hãy chọn một danh mục hợp lệ.");
      return;
    }
    if (![prepTime, cookTime, servings].every(Number.isInteger)
      || prepTime < 1 || cookTime < 0 || servings < 1) {
      fail("Thời gian chuẩn bị và khẩu phần phải lớn hơn 0; thời gian nấu không được âm.");
      return;
    }
    if (validIngredients.length === 0 || validSteps.length === 0) {
      fail("Hãy nhập ít nhất một nguyên liệu và một bước thực hiện.");
      return;
    }
    if (validIngredients.some((ingredient) => !ingredient.name.trim() || ingredient.name.trim().length > 200
      || ingredient.quantity.trim().length > 50 || ingredient.unit.trim().length > 50)) {
      fail("Mỗi nguyên liệu đã nhập cần có tên; tên tối đa 200 ký tự, lượng và đơn vị tối đa 50 ký tự.");
      return;
    }
    if (validSteps.some((step) => step.description.trim().length > 2000)) {
      fail("Mỗi bước thực hiện không được dài quá 2.000 ký tự.");
      return;
    }
    if (steps.some((step) => !step.description.trim() && step.durationMinutes > 0)) {
      fail("Bước có thời gian cần được mô tả, hoặc hãy xóa bước đó.");
      return;
    }
    if (steps.some((step) => !Number.isInteger(step.durationMinutes) || step.durationMinutes < 0)) {
      fail("Thời gian của mỗi bước phải là số phút nguyên không âm.");
      return;
    }
    if (validIngredients.some((ingredient) => !isNonNegativeIngredientQuantity(ingredient.quantity))) {
      fail("Số lượng nguyên liệu không được nhỏ hơn 0. Có thể nhập phân số hoặc “vừa đủ”.");
      return;
    }
    if (imageFile) {
      const supportedTypes = ["image/jpeg", "image/png", "image/webp", "image/avif"];
      if (!supportedTypes.includes(imageFile.type)) {
        fail("Ảnh phải có định dạng JPEG, PNG, WebP hoặc AVIF.");
        return;
      }
<<<<<<< HEAD
      if (imageFile.size > 20 * 1024 * 1024) {
        setSubmitError("Kích thước ảnh không được vượt quá 20 MB.");
=======
      if (imageFile.size > 5 * 1024 * 1024) {
        fail("Kích thước ảnh không được vượt quá 5 MB.");
>>>>>>> 315339d (Update UI)
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
        instructions: instructions.trim(),
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
      createdRecipeRef.current = true;
      setCreatedRecipeId(created.id);
      try { if (draftKey) localStorage.removeItem(draftKey); } catch { /* Storage may be unavailable. */ }
      if (imageFile) {
        try {
          await recipeApi.uploadImage(created.id, imageFile, imageAltText.trim());
        } catch (uploadError) {
          setSubmitError(
            `Bản nháp đã được lưu, nhưng ảnh chưa tải lên được: ${getApiErrorMessage(uploadError)}. Mở trang quản lý để tải ảnh rồi xuất bản.`,
          );
          await queryClient.invalidateQueries({ queryKey: recipeKeys.all() }).catch(() => undefined);
          return;
        }
      }
      if (mode === "publish") {
        try {
          const published = await recipeApi.setStatus(created.id, "publish");
          if (published.status !== 1) throw new Error("Máy chủ chưa xác nhận trạng thái đã xuất bản.");
        } catch (publishError) {
          setSubmitError(
            `Bản nháp đã được lưu nhưng chưa xuất bản: ${getApiErrorMessage(publishError)}. Mở trang quản lý để thử lại.`,
          );
          await queryClient.invalidateQueries({ queryKey: recipeKeys.all() }).catch(() => undefined);
          return;
        }
      }
      setSubmittedSuccess(true);
      await queryClient.invalidateQueries({ queryKey: recipeKeys.all() }).catch(() => undefined);
      router.push(`/dashboard/recipes/${created.id}/edit`);
    } catch (error) {
      setSubmitError(getApiErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div ref={formTopRef} className="mx-auto max-w-4xl space-y-7">
      <Breadcrumb
        items={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Công thức", href: "/dashboard/recipes" },
          { label: "Đăng công thức mới" },
        ]}
      />

      <div className="flex flex-wrap items-end justify-between gap-4 border-b border-stone-200 pb-6">
        <div>
          <p className="mb-2 text-xs font-bold uppercase tracking-[0.2em] text-brand">Không gian tác giả</p>
          <h1 className="font-display text-3xl font-bold text-content-primary">
            Tạo công thức mới
          </h1>
          <p className="mt-2 text-sm text-content-secondary">
            Ghi lại món ăn của bạn, thêm ảnh và chia sẻ với cộng đồng.
          </p>
        </div>
        <span role="status" className="rounded-full border border-stone-200 bg-white px-3 py-1.5 text-xs font-medium text-content-secondary">
          {submittedSuccess ? (submitMode === "publish" ? "Đã xuất bản" : "Đã lưu bản nháp")
            : createdRecipeId ? "Bản nháp đã lưu trên máy chủ"
              : isSubmitting ? (submitMode === "publish" ? "Đang lưu và xuất bản…" : "Đang lưu bản nháp…")
                : draftReady ? (draftSaveFailed ? "Không lưu được bản soạn trên trình duyệt"
                  : draftSaved ? "Bản soạn đã lưu trên trình duyệt" : "Đang lưu bản soạn…")
                  : "Đang mở bản soạn…"}
        </span>
      </div>

      {submittedSuccess && (
        <div className="p-4 rounded-xl bg-brand-light border border-brand-border text-brand-hover flex items-center gap-3">
          <CheckCircle2 className="w-5 h-5 shrink-0" />
          <span className="text-sm font-semibold">
            {submitMode === "publish" ? "Đã tạo và xuất bản công thức thành công! Đang mở trang quản lý..." : "Đã tạo công thức ở trạng thái bản nháp. Đang mở trang quản lý..."}
          </span>
        </div>
      )}

      {submitError && (
        <ErrorState
          title={createdRecipeId ? "Bản nháp đã lưu, cần hoàn tất" : "Chưa thể lưu công thức"}
          message={submitError}
        />
      )}
      {createdRecipeId && submitError && (
        <Link
          href={`/dashboard/recipes/${createdRecipeId}/edit`}
          className="inline-flex text-sm font-semibold text-brand hover:underline"
        >
          Mở bản nháp để tiếp tục
        </Link>
      )}
      {categoriesQuery.isError && (
        <ErrorState
          title="Không tải được danh mục"
          message="Không thể tạo công thức khi chưa lấy được danh mục từ API."
          onRetry={() => void categoriesQuery.refetch()}
        />
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <p className="text-sm text-content-secondary">Điền thông tin món ăn, nguyên liệu và các bước. Bạn có thể lưu nháp hoặc xuất bản ngay sau khi hoàn tất. Ảnh sẽ cần chọn lại nếu tải lại trang.</p>
        {/* SECTION 1: OPTIONAL RECIPE IMAGE */}
        <div className="space-y-5 rounded-2xl border border-stone-200 bg-white p-6 shadow-subtle">
          <div className="border-b border-slate-100 pb-2">
            <h2 className="text-base font-bold text-content-primary">
              1. Ảnh thumbnail <span className="font-normal text-content-tertiary">(không bắt buộc)</span>
            </h2>
            <p className="mt-1 text-sm text-content-secondary">
              Chọn ảnh đại diện cho công thức. Bạn có thể bỏ qua và thêm ảnh sau.
            </p>
          </div>

          <div className="grid gap-5 sm:grid-cols-[minmax(0,1fr)_220px] sm:items-start">
            <div className="space-y-4">
<<<<<<< HEAD
              <label className="block space-y-2">
                <span className="text-xs font-semibold uppercase tracking-wider text-content-secondary">
                  Chọn ảnh (JPEG, PNG, WebP hoặc AVIF; tối đa 20 MB)
                </span>
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp,image/avif"
                  onChange={(event) => handleImageFileChange(event.target.files?.[0] ?? null)}
                  className="block w-full rounded-lg border border-slate-300 p-2 text-sm font-normal"
                />
              </label>
=======
              <input
                ref={imageInputRef}
                type="file"
                accept="image/jpeg,image/png,image/webp,image/avif"
                aria-label="Chọn ảnh đại diện công thức"
                onChange={(event) => handleImageFileChange(event.target.files?.[0] ?? null)}
                className="hidden"
              />
              <Button
                type="button"
                variant="outline"
                leftIcon={<ImagePlus className="h-4 w-4" />}
                onClick={() => imageInputRef.current?.click()}
              >
                {imageFile ? "Đổi ảnh" : "Thêm ảnh"}
              </Button>
              <p className="text-xs text-content-tertiary">JPEG, PNG, WebP hoặc AVIF · tối đa 5 MB</p>
              {imageError && <p role="alert" className="text-sm font-medium text-rose-600">{imageError}</p>}
>>>>>>> 315339d (Update UI)
              {imageFile && (
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <p className="min-w-0 truncate text-sm text-content-secondary">
                    Đã chọn: {imageFile.name}
                  </p>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => handleImageFileChange(null)}
                    leftIcon={<Trash2 className="h-4 w-4" />}
                  >
                    Bỏ ảnh
                  </Button>
                </div>
              )}
              <Input
                label="Mô tả ảnh (không bắt buộc)"
                value={imageAltText}
                maxLength={200}
                onChange={(event) => setImageAltText(event.target.value)}
                disabled={!imageFile}
              />
            </div>

            <div className="flex min-h-36 items-center justify-center overflow-hidden rounded-lg border border-dashed border-slate-300 bg-slate-50 p-2">
              {imagePreviewUrl ? (
                <Image
                  src={imagePreviewUrl}
                  alt={imageAltText || `Ảnh xem trước ${title || "công thức"}`}
                  width={400}
                  height={200}
                  unoptimized
                  className="h-36 w-full rounded-md object-cover"
                />
              ) : (
                <p className="flex flex-col items-center gap-2 text-center text-sm text-content-tertiary">
                  <ImagePlus className="h-7 w-7" aria-hidden="true" />Chưa chọn ảnh
                </p>
              )}
            </div>
          </div>
        </div>

        {/* SECTION 2: GENERAL INFORMATION */}
        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-5">
          <h2 className="text-base font-bold text-content-primary pb-2 border-b border-slate-100">
            2. Thông tin chung
          </h2>

          <Input
            label="Tên món ăn *"
            placeholder="Ví dụ: Phở Bò Bắp Hoa Hà Nội Chuẩn Vị"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
          />

          <div className="space-y-1.5">
            <label htmlFor="recipe-summary" className="block text-xs font-semibold uppercase tracking-wider text-content-secondary">
              Mô tả ngắn món ăn *
            </label>
            <textarea
              id="recipe-summary"
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
              label="Chuẩn bị (phút) *"
              type="number"
              value={prepTime}
              onChange={(e) => setPrepTime(Number(e.target.value))}
              min={1}
            />
            <Input
              label="Nấu (phút) *"
              type="number"
              value={cookTime}
              onChange={(e) => setCookTime(Number(e.target.value))}
              min={0}
            />
            <Input
              label="Khẩu phần (người) *"
              type="number"
              value={servings}
              onChange={(e) => setServings(Number(e.target.value))}
              min={1}
            />
          </div>

        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-6">
          <label className="block space-y-2">
            <span className="text-xs font-semibold uppercase tracking-wider text-content-secondary">
              Ghi chú chung (không bắt buộc)
            </span>
            <textarea
              rows={4}
              value={instructions}
              onChange={(event) => setInstructions(event.target.value)}
              className="w-full rounded-lg border border-slate-300 bg-white p-3 text-sm text-content-primary focus:border-brand focus:outline-none"
              placeholder="Ví dụ: Mẹo chọn nguyên liệu, cách bảo quản hoặc lưu ý khi chế biến."
            />
            <span className="text-xs text-content-tertiary">Các bước chi tiết sẽ được lưu riêng bên dưới.</span>
          </label>
        </div>

        {/* SECTION 3: INGREDIENTS BUILDER */}
        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-4">
          <div className="flex items-center justify-between pb-2 border-b border-slate-100">
            <h2 className="text-base font-bold text-content-primary">3. Nguyên liệu</h2>
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
          <p className="text-sm text-content-secondary">Nhập tên nguyên liệu. Lượng và đơn vị có thể để trống nếu dùng theo khẩu vị.</p>

          <div className="space-y-3">
            {ingredients.map((ing, idx) => (
              <div
                key={ing.id}
                className="grid grid-cols-1 items-start gap-3 sm:grid-cols-[minmax(0,1fr)_7rem_10rem_auto]"
              >
                <div>
                  <Input
                    label={`Nguyên liệu ${idx + 1} *`}
                    placeholder="Tên nguyên liệu (VD: Thịt bò, Hành tây...)"
                    maxLength={200}
                    value={ing.name}
                    onChange={(e) => setIngredients((current) => current.map((item, index) =>
                      index === idx ? { ...item, name: e.target.value } : item))}
                  />
                </div>
                <div>
                  <Input
                    label="Số lượng"
                    type="text"
                    inputMode="decimal"
                    placeholder="Số lượng"
                    maxLength={50}
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
                    aria-label={`Xóa nguyên liệu ${idx + 1}`}
                    className="mt-6 rounded-lg p-2 text-slate-400 transition-colors hover:bg-rose-50 hover:text-rose-600 focus:outline-none focus:ring-2 focus:ring-rose-400"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* SECTION 4: STEPS BUILDER */}
        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-3 pb-2 border-b border-slate-100">
            <h2 className="text-base font-bold text-content-primary">
              4. Các bước thực hiện
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
              <div key={step.id} className="p-4 border border-slate-200 rounded-lg bg-slate-50/50 space-y-3">
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
                <label htmlFor={`step-${idx}`} className="sr-only">Mô tả bước {idx + 1}</label>
                <textarea
                  id={`step-${idx}`}
                  rows={2}
                  placeholder={`Mô tả chi tiết các thao tác trong bước ${idx + 1}...`}
                  maxLength={2000}
                  value={step.description}
                  onChange={(e) => {
                    setSteps((current) => current.map((item, index) =>
                      index === idx ? { ...item, description: e.target.value } : item));
                  }}
                  className="w-full rounded-lg border border-slate-300 bg-white p-3 text-sm focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/20"
                />
                <Input
                  label="Thời gian bước (phút, không bắt buộc)"
                  type="number"
                  min={0}
                  value={step.durationMinutes}
                  onChange={(event) => setSteps((current) => current.map((item, index) =>
                    index === idx ? { ...item, durationMinutes: Number(event.target.value) } : item))}
                  className="max-w-44"
                />
              </div>
            ))}
          </div>
        </div>

        {/* FORM ACTIONS */}
        <div className="flex flex-col gap-3 border-t border-slate-200 pt-5 sm:flex-row sm:items-center sm:justify-end">
          <Link href="/dashboard/recipes" className="inline-flex h-12 items-center justify-center rounded-lg px-4 text-sm font-medium text-content-secondary hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-brand/30">
            Quay lại danh sách
          </Link>
          <Button
            type="submit"
            value="draft"
            variant="outline"
            size="lg"
            isLoading={isSubmitting && submitMode === "draft"}
            disabled={Boolean(createdRecipeId) || isSubmitting || !draftReady || categoriesQuery.isLoading}
            className="w-full sm:w-auto"
          >
            Lưu bản nháp
          </Button>
          <Button
            type="submit"
            value="publish"
            variant="primary"
            size="lg"
            isLoading={isSubmitting && submitMode === "publish"}
            disabled={Boolean(createdRecipeId) || isSubmitting || !draftReady || categoriesQuery.isLoading}
            className="w-full sm:w-auto"
          >
            Lưu và Xuất bản
          </Button>
        </div>
      </form>
    </div>
  );
}
