import React from "react";
import { Inbox } from "lucide-react";
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
      role="status"
      className={cn(
        "mx-auto my-8 flex w-full max-w-2xl flex-col items-center justify-center rounded-2xl border border-dashed border-stone-300 bg-surface-cream px-6 py-14 text-center sm:px-10",
        className
      )}
    >
      <div className="mb-5 flex h-16 w-16 items-center justify-center rounded-2xl border border-accent/20 bg-accent-light text-accent shadow-subtle">
        {icon || <Inbox className="h-7 w-7" aria-hidden="true" />}
      </div>
      <h3 className="mb-2 font-display text-xl font-bold text-content-primary">{title}</h3>
      <p className="mb-6 max-w-md text-sm leading-6 text-content-secondary">{description}</p>
      {actionLabel && onAction && (
        <Button variant="primary" size="sm" onClick={onAction}>
          {actionLabel}
        </Button>
      )}
    </div>
  );
};
