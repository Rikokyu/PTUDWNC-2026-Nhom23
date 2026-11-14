import React from "react";
import { cn } from "@/lib/utils";
import { AvatarImage } from "@/components/common/AvatarImage";

export interface AuthorAvatarProps {
  name: string;
  displayName?: string;
  avatarUrl?: string;
  showName?: boolean;
  role?: string;
  size?: "sm" | "md" | "lg";
  className?: string;
}

export const AuthorAvatar: React.FC<AuthorAvatarProps> = ({
  name,
  displayName,
  avatarUrl,
  showName = false,
  role,
  size = "sm",
  className,
}) => {
  const avatarSizes = {
    sm: "w-6 h-6 text-[10px]",
    md: "w-8 h-8 text-xs",
    lg: "w-11 h-11 text-sm",
  };

  return (
    <div className={cn("inline-flex min-w-0 items-center gap-2", className)}>
      <div
        className={cn(
          "rounded-full bg-brand-light text-brand-hover font-bold flex items-center justify-center border border-brand-border/40 shrink-0 overflow-hidden",
          avatarSizes[size]
        )}
      >
        <AvatarImage src={avatarUrl} alt={name} className="h-full w-full object-cover" />
      </div>
      {(showName || size === "md" || size === "lg" || role) && (
        <div className="flex min-w-0 flex-col">
          <span className="truncate text-xs font-semibold text-content-primary leading-tight">
            {displayName ?? name}
          </span>
          {role && (
            <span className="text-[10px] text-content-tertiary">{role}</span>
          )}
        </div>
      )}
    </div>
  );
};
