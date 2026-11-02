"use client";

const fallbackImage = "/images/image 1.png";

interface RecipeThumbnailProps {
  src: string | null | undefined;
  alt: string;
  className: string;
}

export function RecipeThumbnail({ src, alt, className }: RecipeThumbnailProps) {
  return (
    <img
      src={src || fallbackImage}
      alt={alt}
      className={className}
      loading="lazy"
      onError={(event) => {
        const image = event.currentTarget;
        if (image.getAttribute("src") !== fallbackImage) {
          image.src = fallbackImage;
        }
      }}
    />
  );
}
