export function safeReturnPath(value: string | null): string {
  if (value && /^\/dashboard(?:\/|$|\?)/.test(value) && !/[\\\s]/.test(value)) {
    return value;
  }
  return "/dashboard";
}
