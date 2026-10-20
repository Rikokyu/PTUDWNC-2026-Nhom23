import React from "react";
import Image from "next/image";
import { cn } from "@/lib/utils";

export interface AuthorAvatarProps {
  name: string;
  avatarUrl?: string;
  role?: string;
  size?: "sm" | "md" | "lg";
  className?: string;
}

export const AuthorAvatar: React.FC<AuthorAvatarProps> = ({
  name,
  avatarUrl,
  role,
  size = "sm",
  className,
}) => {
  const avatarSizes = {
    sm: "w-6 h-6 text-[10px]",
    md: "w-8 h-8 text-xs",
    lg: "w-11 h-11 text-sm",
  };

  const initial = name ? name.charAt(0).toUpperCase() : "U";

  return (
    <div className={cn("inline-flex items-center gap-2", className)}>
      <div
        className={cn(
          "rounded-full bg-brand-light text-brand-hover font-bold flex items-center justify-center border border-brand-border/40 shrink-0 overflow-hidden",
          avatarSizes[size]
        )}
      >
        {avatarUrl ? (
          <img src={avatarUrl} alt={name} className="w-full h-full object-cover" />
        ) : (
          <span>{initial}</span>
        )}
      </div>
      {(size === "md" || size === "lg" || role) && (
        <div className="flex flex-col">
          <span className="text-xs font-semibold text-content-primary leading-tight">
            {name}
          </span>
          {role && (
            <span className="text-[10px] text-content-tertiary">{role}</span>
          )}
        </div>
      )}
    </div>
  );
};
