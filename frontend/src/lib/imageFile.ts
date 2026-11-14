const MIME_BY_EXTENSION: Record<string, string> = {
  ".jpg": "image/jpeg",
  ".jpeg": "image/jpeg",
  ".png": "image/png",
  ".webp": "image/webp",
  ".avif": "image/avif",
};

const SUPPORTED_IMAGE_MIME_TYPES = new Set(Object.values(MIME_BY_EXTENSION));

export function normalizeImageFile(file: File): File | null {
  const declaredType = file.type.toLowerCase();
  const extension = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
  const imageType = SUPPORTED_IMAGE_MIME_TYPES.has(declaredType)
    ? declaredType
    : MIME_BY_EXTENSION[extension] ?? "";

  if (!imageType) return null;
  if (file.type === imageType) return file;

  return new File([file], file.name, {
    type: imageType,
    lastModified: file.lastModified,
  });
}
