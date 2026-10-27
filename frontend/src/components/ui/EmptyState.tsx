import React from "react";
import { FolderOpen } from "lucide-react";
import { Button } from "./Button";
import { cn } from "@/lib/utils";

export interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description: string;
  actionLabel?: string;
  onAction?: () => void;
  className?: string;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  actionLabel,
  onAction,
  className,
}) => {
  return (
    <div
      className={cn(
        "py-16 px-6 text-center border border-dashed border-slate-200 rounded-xl bg-slate-50/50 flex flex-col items-center justify-center max-w-md mx-auto my-8",
        className
      )}
    >
      <div className="w-12 h-12 rounded-full bg-pastel-purple-light text-slate-700 flex items-center justify-center mb-4 border border-pastel-purple/40">
        {icon || <FolderOpen className="w-6 h-6 text-slate-600" />}
      </div>
      <h3 className="text-base font-semibold text-content-primary mb-1">{title}</h3>
      <p className="text-xs text-content-tertiary mb-6 max-w-xs">{description}</p>
      {actionLabel && onAction && (
        <Button variant="primary" size="sm" onClick={onAction}>
          {actionLabel}
        </Button>
      )}
    </div>
  );
};
