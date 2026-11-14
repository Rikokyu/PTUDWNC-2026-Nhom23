"use client";

import { useEffect, useState } from "react";
import { ChefHat } from "lucide-react";

interface RecipeThumbnailProps {
  src: string | null | undefined;
  alt: string;
  className: string;
}

export function RecipeThumbnail({ src, alt, className }: RecipeThumbnailProps) {
  const [failed, setFailed] = useState(false);

  useEffect(() => setFailed(false), [src]);

  if (!src || failed) {
    return <div role="img" aria-label={`Chưa có ảnh: ${alt}`} className={`flex items-center justify-center bg-surface-subtle text-content-tertiary ${className}`}>
      <ChefHat className="h-6 w-6" aria-hidden="true" />
    </div>;
  }

  return (
    <img
      src={src}
      alt={alt}
      className={className}
      loading="lazy"
      decoding="async"
      onError={() => setFailed(true)}
    />
  );
}
