"use client";

import { useEffect, useState, type ReactNode } from "react";
import { CheckCircle2, FlaskConical } from "lucide-react";
import {
  useDashboardPreviewStore,
  type PreviewRecipe,
} from "@/store/dashboard-preview-store";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

export function usePreviewReady() {
  const [ready, setReady] = useState(false);
  useEffect(() => {
    let active = true;
    void Promise.resolve(useDashboardPreviewStore.persist.rehydrate()).finally(
      () => {
        if (active) setReady(true);
      },
    );
    return () => {
      active = false;
    };
  }, []);
  return ready;
}

export function DashboardHeading({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string;
  title: string;
  description: string;
  action?: ReactNode;
}) {
  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="mb-2 text-xs font-semibold uppercase tracking-[0.18em] text-orange-700">
            {eyebrow}
          </p>
          <h1 className="text-2xl font-bold tracking-tight sm:text-3xl">
            {title}
          </h1>
          <p className="mt-2 max-w-2xl text-sm text-slate-500">{description}</p>
        </div>
        {action}
      </div>
      <div className="flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-xs leading-relaxed text-amber-900">
        <FlaskConical className="mt-0.5 size-4 shrink-0" />
        <p>
          <strong>Bản xem trước giao diện.</strong> Dữ liệu minh họa và thay đổi
          chỉ được lưu trên trình duyệt này, chưa cập nhật tài khoản hoặc nội
          dung trên máy chủ.
        </p>
      </div>
    </div>
  );
}

export function Notice({ children }: { children: ReactNode }) {
  return (
    <p
      role="status"
      className="flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
    >
      <CheckCircle2 className="size-4 shrink-0" />
      {children}
    </p>
  );
}

export function Field({
  label,
  htmlFor,
  hint,
  children,
}: {
  label: string;
  htmlFor: string;
  hint?: string;
  children: ReactNode;
}) {
  return (
    <div className="space-y-2">
      <label
        htmlFor={htmlFor}
        className="block text-sm font-medium text-slate-700"
      >
        {label}
      </label>
      {children}
      {hint && <p className="text-xs text-slate-500">{hint}</p>}
    </div>
  );
}

export const formControl =
  "w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-800 outline-none transition focus:border-orange-500 focus:ring-2 focus:ring-orange-100 disabled:bg-slate-50 disabled:text-slate-500";
export const statusLabels = {
  Draft: "Bản nháp",
  Published: "Đã xuất bản",
  Archived: "Đã lưu trữ",
};
export const difficultyLabels = {
  Easy: "Dễ",
  Medium: "Vừa",
  Hard: "Khó",
  Expert: "Chuyên gia",
};

export function RecipeStatus({ status }: { status: PreviewRecipe["status"] }) {
  return (
    <Badge
      variant="outline"
      className={
        status === "Published"
          ? "border-emerald-200 bg-emerald-50 text-emerald-700"
          : status === "Draft"
            ? "border-amber-200 bg-amber-50 text-amber-700"
            : "border-slate-200 bg-slate-50 text-slate-600"
      }
    >
      {statusLabels[status]}
    </Badge>
  );
}

export function ConfirmDelete({
  title,
  description,
  open,
  onOpenChange,
  onConfirm,
}: {
  title: string;
  description: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
}) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Hủy
          </Button>
          <Button variant="destructive" onClick={onConfirm}>
            Xóa khỏi bản xem trước
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
