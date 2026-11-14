import React from "react";
import { CheckCircle2, AlertTriangle, XCircle, Info, X } from "lucide-react";
import { cn } from "@/lib/utils";

export interface ToastProps {
  id?: string;
  type?: "success" | "error" | "warning" | "info";
  title: string;
  message?: string;
  onClose?: () => void;
}

export const Toast: React.FC<ToastProps> = ({
  type = "success",
  title,
  message,
  onClose,
}) => {
  const icons = {
    success: <CheckCircle2 className="w-5 h-5 text-accent" />,
    error: <XCircle className="w-5 h-5 text-rose-500" />,
    warning: <AlertTriangle className="w-5 h-5 text-accent" />,
    info: <Info className="w-5 h-5 text-sky-500" />,
  };

  const borders = {
    success: "border-accent/25 bg-surface",
    error: "border-rose-200 bg-white",
    warning: "border-amber-200 bg-white",
    info: "border-sky-200 bg-white",
  };

  return (
    <div
      className={cn(
        "flex w-full max-w-sm items-start gap-3 rounded-2xl border p-4 shadow-dropdown transition-all duration-200 animate-in slide-in-from-top-2",
        borders[type]
      )}
    >
      <div className="shrink-0 mt-0.5">{icons[type]}</div>
      <div className="flex-1">
        <h5 className="text-sm font-semibold text-content-primary">{title}</h5>
        {message && <p className="text-xs text-content-secondary mt-0.5">{message}</p>}
      </div>
      {onClose && (
        <button
          onClick={onClose}
          className="text-slate-400 hover:text-slate-600 transition-colors p-0.5 rounded-md"
        >
          <X className="w-4 h-4" />
        </button>
      )}
    </div>
  );
};
