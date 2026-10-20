"use client";

import Link from "next/link";
import { useEffect, useState, type FormEvent, type ReactNode } from "react";
import { useRouter } from "next/navigation";
import {
  ArrowLeft,
  ChefHat,
  Clock3,
  Plus,
  Save,
  Trash2,
  Users,
} from "lucide-react";
import {
  useDashboardPreviewStore,
  type PreviewRecipe,
} from "@/store/dashboard-preview-store";
import {
  DashboardHeading,
  difficultyLabels,
  Field,
  formControl,
  Notice,
  RecipeStatus,
  statusLabels,
  usePreviewReady,
} from "./preview-ui";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { EmptyState } from "@/components/shared/empty-state";
import { Loading } from "@/components/shared/loading";
import { ErrorMessage } from "@/components/shared/error-message";

function Section({
  title,
  description,
  children,
}: {
  title: string;
  description: string;
  children: ReactNode;
}) {
  return (
    <section className="space-y-5 rounded-2xl border border-slate-200 bg-white p-5 sm:p-6">
      <div>
        <h2 className="text-base font-semibold">{title}</h2>
        <p className="mt-1 text-xs text-slate-500">{description}</p>
      </div>
      {children}
    </section>
  );
}

export function RecipeEditor({ id }: { id: string }) {
  const ready = usePreviewReady();
  const recipes = useDashboardPreviewStore((state) => state.recipes);
  const categories = useDashboardPreviewStore((state) => state.categories);
  if (!ready) return <Loading label="Đang mở công thức…" />;
  const recipe: PreviewRecipe | undefined =
    id === "preview-new"
      ? {
          id,
          title: "",
          description: "",
          categoryId: categories[0]?.id ?? "",
          prepTime: 15,
          cookTime: 20,
          servings: 2,
          difficulty: "Easy",
          status: "Draft",
          ingredients: [""],
          steps: [""],
          updatedAt: "",
        }
      : recipes.find((item) => item.id === id);
  if (!recipe)
    return (
      <EmptyState
        title="Không có công thức trong bản xem trước"
        description="Chọn một công thức minh họa trong danh sách để chỉnh sửa giao diện."
        actionHref="/dashboard/recipes"
        actionLabel="Về danh sách công thức"
      />
    );
  return <EditorForm key={id} initialRecipe={recipe} />;
}

function EditorForm({ initialRecipe }: { initialRecipe: PreviewRecipe }) {
  const router = useRouter();
  const { categories, saveRecipe } = useDashboardPreviewStore();
  const [recipe, setRecipe] = useState<PreviewRecipe>(() =>
    structuredClone(initialRecipe),
  );
  const [baseline, setBaseline] = useState(JSON.stringify(initialRecipe));
  const [notice, setNotice] = useState("");
  const [error, setError] = useState("");
  const dirty = JSON.stringify(recipe) !== baseline;
  const isNew = initialRecipe.id === "preview-new";
  const category = categories.find((item) => item.id === recipe.categoryId);
  useEffect(() => {
    if (!dirty) return;
    const warn = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      event.returnValue = "";
    };
    window.addEventListener("beforeunload", warn);
    return () => window.removeEventListener("beforeunload", warn);
  }, [dirty]);

  function update<K extends keyof PreviewRecipe>(
    key: K,
    value: PreviewRecipe[K],
  ) {
    setRecipe((current) => ({ ...current, [key]: value }));
    setNotice("");
  }
  function updateRow(
    key: "ingredients" | "steps",
    index: number,
    value: string,
  ) {
    update(
      key,
      recipe[key].map((item, itemIndex) =>
        itemIndex === index ? value : item,
      ),
    );
  }
  function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    if (
      !recipe.title.trim() ||
      !recipe.description.trim() ||
      !categories.some((item) => item.id === recipe.categoryId)
    ) {
      setError("Điền tên món ăn, mô tả và chọn danh mục hợp lệ.");
      return;
    }
    if (
      recipe.ingredients.some((item) => !item.trim()) ||
      recipe.steps.some((item) => !item.trim())
    ) {
      setError(
        "Điền đầy đủ nguyên liệu và hướng dẫn hoặc xóa những dòng còn trống.",
      );
      return;
    }
    const saved = {
      ...recipe,
      id: isNew ? `preview-${crypto.randomUUID()}` : recipe.id,
      title: recipe.title.trim(),
      description: recipe.description.trim(),
      ingredients: recipe.ingredients.map((item) => item.trim()),
      steps: recipe.steps.map((item) => item.trim()),
      updatedAt: new Date().toISOString(),
    };
    saveRecipe(saved);
    setRecipe(saved);
    setBaseline(JSON.stringify(saved));
    setNotice("Đã lưu công thức trong bản xem trước trên trình duyệt.");
    if (isNew) router.replace(`/dashboard/recipes/${saved.id}/edit`);
  }

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <Link
        href="/dashboard/recipes"
        className="inline-flex items-center gap-2 text-sm text-slate-500 hover:text-orange-700"
      >
        <ArrowLeft className="size-4" />
        Công thức của tôi
      </Link>
      <DashboardHeading
        eyebrow="Sổ tay vào bếp"
        title={isNew ? "Viết một món ngon mới" : "Chỉnh sửa công thức"}
        description="Một công thức rõ ràng sẽ giúp người khác tự tin vào bếp cùng bạn."
      />
      {notice && <Notice>{notice}</Notice>}
      {error && <ErrorMessage message={error} />}
      <form
        onSubmit={save}
        className="grid items-start gap-6 xl:grid-cols-[minmax(0,1fr)_260px]"
      >
        <div className="min-w-0 space-y-5">
          <Section
            title="01 · Thông tin món ăn"
            description="Giới thiệu điều đặc biệt trong công thức của bạn."
          >
            <Field label="Tên món ăn *" htmlFor="recipe-title">
              <Input
                id="recipe-title"
                required
                maxLength={200}
                placeholder="Ví dụ: Phở bò Hà Nội"
                value={recipe.title}
                onChange={(event) => update("title", event.target.value)}
              />
            </Field>
            <Field
              label="Mô tả *"
              htmlFor="recipe-description"
              hint={`${recipe.description.length}/1000 ký tự`}
            >
              <textarea
                id="recipe-description"
                required
                maxLength={1000}
                rows={4}
                className={formControl}
                placeholder="Hương vị, câu chuyện hoặc mẹo nhỏ tạo nên món ăn…"
                value={recipe.description}
                onChange={(event) => update("description", event.target.value)}
              />
            </Field>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Danh mục *" htmlFor="recipe-category">
                <select
                  id="recipe-category"
                  required
                  className={formControl}
                  value={recipe.categoryId}
                  onChange={(event) => update("categoryId", event.target.value)}
                >
                  {categories.map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.name}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Độ khó" htmlFor="recipe-difficulty">
                <select
                  id="recipe-difficulty"
                  className={formControl}
                  value={recipe.difficulty}
                  onChange={(event) =>
                    update(
                      "difficulty",
                      event.target.value as PreviewRecipe["difficulty"],
                    )
                  }
                >
                  {Object.entries(difficultyLabels).map(([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </Field>
            </div>
            <div className="grid gap-4 sm:grid-cols-3">
              {(
                [
                  {
                    key: "prepTime",
                    label: "Chuẩn bị (phút)",
                    min: 1,
                    max: 1440,
                  },
                  {
                    key: "cookTime",
                    label: "Nấu ăn (phút)",
                    min: 0,
                    max: 1440,
                  },
                  { key: "servings", label: "Khẩu phần", min: 1, max: 50 },
                ] as const
              ).map(({ key, label, min, max }) => (
                <Field key={key} label={label} htmlFor={`recipe-${key}`}>
                  <Input
                    id={`recipe-${key}`}
                    type="number"
                    required
                    min={min}
                    max={max}
                    step={1}
                    value={Number.isNaN(recipe[key]) ? "" : recipe[key]}
                    onChange={(event) =>
                      update(key, event.target.valueAsNumber)
                    }
                  />
                </Field>
              ))}
            </div>
          </Section>
          <Section
            title="02 · Nguyên liệu"
            description="Ghi lượng và đơn vị để người đọc dễ chuẩn bị."
          >
            <div className="space-y-3">
              {recipe.ingredients.map((ingredient, index) => (
                <div key={index} className="flex items-center gap-2">
                  <span className="w-5 shrink-0 text-xs text-slate-400">
                    {index + 1}.
                  </span>
                  <Input
                    required
                    aria-label={`Nguyên liệu ${index + 1}`}
                    maxLength={300}
                    placeholder="Ví dụ: 200 g bột mì"
                    value={ingredient}
                    onChange={(event) =>
                      updateRow("ingredients", index, event.target.value)
                    }
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    aria-label={`Xóa nguyên liệu ${index + 1}`}
                    disabled={recipe.ingredients.length <= 1}
                    onClick={() =>
                      update(
                        "ingredients",
                        recipe.ingredients.filter(
                          (_, itemIndex) => itemIndex !== index,
                        ),
                      )
                    }
                  >
                    <Trash2 className="text-slate-400" />
                  </Button>
                </div>
              ))}
            </div>
            <Button
              type="button"
              variant="outline"
              disabled={recipe.ingredients.length >= 50}
              onClick={() => update("ingredients", [...recipe.ingredients, ""])}
            >
              <Plus />
              Thêm nguyên liệu
            </Button>
          </Section>
          <Section
            title="03 · Hướng dẫn thực hiện"
            description="Chia nhỏ thành từng bước đơn giản, dễ làm theo."
          >
            <div className="space-y-4">
              {recipe.steps.map((step, index) => (
                <div
                  key={index}
                  className="rounded-xl border border-slate-100 bg-slate-50/60 p-4"
                >
                  <div className="mb-3 flex items-center justify-between">
                    <label
                      htmlFor={`recipe-step-${index}`}
                      className="text-sm font-semibold text-orange-800"
                    >
                      Bước {index + 1}
                    </label>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      aria-label={`Xóa bước ${index + 1}`}
                      disabled={recipe.steps.length <= 1}
                      onClick={() =>
                        update(
                          "steps",
                          recipe.steps.filter(
                            (_, itemIndex) => itemIndex !== index,
                          ),
                        )
                      }
                    >
                      <Trash2 className="text-slate-400" />
                    </Button>
                  </div>
                  <textarea
                    id={`recipe-step-${index}`}
                    required
                    maxLength={2000}
                    rows={3}
                    className={formControl}
                    placeholder="Mô tả thao tác, thời gian và những điều cần lưu ý…"
                    value={step}
                    onChange={(event) =>
                      updateRow("steps", index, event.target.value)
                    }
                  />
                </div>
              ))}
            </div>
            <Button
              type="button"
              variant="outline"
              disabled={recipe.steps.length >= 30}
              onClick={() => update("steps", [...recipe.steps, ""])}
            >
              <Plus />
              Thêm bước
            </Button>
          </Section>
        </div>
        <aside className="space-y-4 xl:sticky xl:top-20">
          <section className="overflow-hidden rounded-2xl border border-orange-100 bg-white">
            <div
              aria-hidden="true"
              className="flex h-32 items-center justify-center bg-gradient-to-br from-orange-50 to-amber-100 text-6xl"
            >
              {category?.emoji ?? "🍽️"}
            </div>
            <div className="space-y-4 p-5">
              <RecipeStatus status={recipe.status} />
              <h2 className="break-words text-lg font-semibold">
                {recipe.title || "Tên món ăn của bạn"}
              </h2>
              <p className="line-clamp-3 break-words text-xs leading-relaxed text-slate-500">
                {recipe.description || "Câu chuyện món ngon bắt đầu từ đây."}
              </p>
              <div className="space-y-2 border-t border-slate-100 pt-4 text-xs text-slate-600">
                <p className="flex items-center gap-2">
                  <Clock3 className="size-4" />
                  {(recipe.prepTime || 0) + (recipe.cookTime || 0)} phút tổng
                  cộng
                </p>
                <p className="flex items-center gap-2">
                  <Users className="size-4" />
                  {recipe.servings || 0} khẩu phần
                </p>
                <p className="flex items-center gap-2">
                  <ChefHat className="size-4" />
                  {difficultyLabels[recipe.difficulty]}
                </p>
              </div>
            </div>
          </section>
          <section className="space-y-4 rounded-2xl border border-slate-200 bg-white p-5">
            <Field label="Trạng thái minh họa" htmlFor="recipe-status">
              <select
                id="recipe-status"
                className={formControl}
                value={recipe.status}
                onChange={(event) =>
                  update(
                    "status",
                    event.target.value as PreviewRecipe["status"],
                  )
                }
              >
                {Object.entries(statusLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </Field>
            <p className="text-xs text-slate-500" aria-live="polite">
              {dirty
                ? "Có thay đổi chưa được lưu."
                : "Không có thay đổi chưa lưu."}
            </p>
            <Button type="submit" className="w-full">
              <Save />
              Lưu bản xem trước
            </Button>
            <Button
              type="button"
              variant="outline"
              className="w-full"
              disabled={!dirty}
              onClick={() => {
                setRecipe(structuredClone(initialRecipe));
                setBaseline(JSON.stringify(initialRecipe));
                setError("");
                setNotice("");
              }}
            >
              Hủy thay đổi
            </Button>
          </section>
        </aside>
      </form>
    </div>
  );
}
