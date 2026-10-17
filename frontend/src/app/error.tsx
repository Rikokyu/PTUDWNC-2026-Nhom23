"use client";

import { ErrorState } from "@/components/ui/ErrorState";

export default function GlobalError({ reset }: { reset: () => void }) {
  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <ErrorState
        title="Đã xảy ra lỗi"
        message="Trang gặp sự cố ngoài dự kiến. Vui lòng thử tải lại."
        onRetry={reset}
      />
    </main>
  );
}
