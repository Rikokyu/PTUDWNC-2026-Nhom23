import React from "react";
import { AlertCircle, RefreshCw } from "lucide-react";
import { Button } from "./Button";
import { cn } from "@/lib/utils";

export interface ErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  className?: string;
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  title = "Có lỗi xảy ra",
  message = "Không thể tải dữ liệu. Vui lòng kiểm tra lại kết nối và thử lại.",
  onRetry,
  className,
}) => {
  return (
    <div
      role="alert"
      className={cn(
        "mx-auto my-8 flex w-full max-w-2xl flex-col items-center justify-center rounded-2xl border border-rose-200 bg-rose-50/70 px-6 py-10 text-center sm:px-10",
        className
      )}
    >
      <div className="mb-5 flex h-14 w-14 items-center justify-center rounded-2xl border border-rose-200 bg-white text-rose-600 shadow-subtle">
        <AlertCircle className="h-7 w-7" aria-hidden="true" />
      </div>
      <h4 className="mb-2 font-display text-xl font-bold text-content-primary">{title}</h4>
      <p className="mb-6 max-w-md text-sm leading-6 text-content-secondary">{message}</p>
      {onRetry && (
        <Button
          variant="outline"
          size="sm"
          onClick={onRetry}
          leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
          className="border-rose-300 text-rose-800 hover:bg-rose-100/50"
        >
          Thử lại
        </Button>
      )}
    </div>
  );
};
