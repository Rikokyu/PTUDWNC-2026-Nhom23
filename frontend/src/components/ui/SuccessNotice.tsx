import { CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface SuccessNoticeProps {
  title: string;
  message?: string;
  className?: string;
}

export function SuccessNotice({ title, message, className }: SuccessNoticeProps) {
  return (
    <div
      role="status"
      className={cn(
        "flex items-start gap-3 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-4 text-emerald-950 shadow-subtle sm:px-5",
        className,
      )}
    >
      <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-white text-emerald-600" aria-hidden="true">
        <CheckCircle2 className="h-5 w-5" />
      </span>
      <div className="min-w-0">
        <p className="font-display text-sm font-bold">{title}</p>
        {message && <p className="mt-1 text-sm leading-6 text-emerald-900">{message}</p>}
      </div>
    </div>
  );
}
