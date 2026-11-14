import React from "react";
import { cn } from "@/lib/utils";

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "accent" | "outline" | "ghost" | "danger";
  size?: "sm" | "md" | "lg";
  isLoading?: boolean;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant = "primary",
      size = "md",
      isLoading = false,
      leftIcon,
      rightIcon,
      children,
      disabled,
      type = "button",
      ...props
    },
    ref
  ) => {
    const baseStyles =
      "inline-flex items-center justify-center rounded-xl font-semibold transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-surface-muted disabled:cursor-not-allowed disabled:opacity-50 select-none";

    const variants = {
      primary:
        "bg-brand text-white shadow-subtle hover:bg-brand-hover active:bg-brand-active focus:ring-brand/35",
      secondary:
        "border border-stone-200 bg-surface-cream text-content-primary hover:border-stone-300 hover:bg-surface-subtle focus:ring-accent/30",
      accent:
        "bg-accent text-white shadow-subtle hover:bg-accent-hover focus:ring-accent/35",
      outline:
        "border border-stone-300 bg-surface text-content-primary hover:border-brand-border hover:bg-brand-light focus:ring-brand/30",
      ghost:
        "bg-transparent text-content-secondary hover:bg-surface-subtle hover:text-content-primary focus:ring-accent/30",
      danger:
        "bg-rose-600 text-white hover:bg-rose-700 focus:ring-rose-400 shadow-sm",
    };

    const sizes = {
      sm: "h-9 gap-1.5 px-3 text-xs",
      md: "h-11 gap-2 px-4 text-sm",
      lg: "text-base px-5 py-2.5 gap-2.5 h-12",
    };

    return (
      <button
        ref={ref}
        type={type}
        disabled={disabled || isLoading}
        className={cn(baseStyles, variants[variant], sizes[size], className)}
        {...props}
      >
        {isLoading ? (
          <svg
            className="animate-spin -ml-1 mr-2 h-4 w-4 text-current"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            ></circle>
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
        ) : leftIcon ? (
          <span className="shrink-0">{leftIcon}</span>
        ) : null}
        <span>{children}</span>
        {!isLoading && rightIcon && <span className="shrink-0">{rightIcon}</span>}
      </button>
    );
  }
);

Button.displayName = "Button";
