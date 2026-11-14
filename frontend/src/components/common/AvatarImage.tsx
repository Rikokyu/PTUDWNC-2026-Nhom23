"use client";

import { useState } from "react";

interface AvatarImageProps {
  src?: string | null;
  alt: string;
  className?: string;
}

const fallbackAvatar = "/images/default-avatar.svg";

export function AvatarImage({ src, alt, className }: AvatarImageProps) {
  const [failedSrc, setFailedSrc] = useState<string | null>(null);
  const imageSrc = src && src !== failedSrc ? src : fallbackAvatar;

  return (
    <img
      src={imageSrc}
      alt={alt}
      className={className}
      onError={() => {
        if (src && imageSrc === src) setFailedSrc(src);
      }}
    />
  );
}
