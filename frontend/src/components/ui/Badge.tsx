import React from "react";
import { cn } from "@/lib/utils";

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: "brand" | "accent" | "pastel" | "neutral" | "outline";
  size?: "sm" | "md";
}

export const Badge: React.FC<BadgeProps> = ({
  className,
  variant = "brand",
  size = "md",
  children,
  ...props
}) => {
  const baseStyles =
    "inline-flex items-center font-medium rounded-full select-none";

  const variants = {
    brand: "bg-brand-light text-brand-hover border border-brand-border/40",
    accent: "bg-accent-light text-accent border border-accent/20",
    pastel: "border border-accent/20 bg-accent-light text-accent-hover",
    neutral: "border border-stone-200 bg-surface-muted text-content-secondary",
    outline: "border border-stone-300 bg-transparent text-content-secondary",
  };

  const sizes = {
    sm: "text-[11px] px-2 py-0.5 gap-1",
    md: "text-xs px-2.5 py-1 gap-1.5",
  };

  return (
    <span
      className={cn(baseStyles, variants[variant], sizes[size], className)}
      {...props}
    >
      {children}
    </span>
  );
};
