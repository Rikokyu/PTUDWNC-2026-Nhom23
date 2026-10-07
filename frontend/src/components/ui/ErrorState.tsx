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
      className={cn(
        "p-6 rounded-xl border border-rose-200 bg-rose-50/40 text-center flex flex-col items-center justify-center max-w-md mx-auto my-8",
        className
      )}
    >
      <div className="w-10 h-10 rounded-full bg-rose-100 text-rose-600 flex items-center justify-center mb-3">
        <AlertCircle className="w-5 h-5" />
      </div>
      <h4 className="text-sm font-semibold text-rose-900 mb-1">{title}</h4>
      <p className="text-xs text-rose-700 mb-4 max-w-xs">{message}</p>
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
