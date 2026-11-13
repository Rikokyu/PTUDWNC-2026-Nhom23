import axios from "axios";

interface ProblemDetails {
  title?: string;
  detail?: string;
  error?: string;
  errors?: Record<string, string[]>;
}

export function getApiFieldErrors(error: unknown): Record<string, string[]> {
  if (!axios.isAxiosError<ProblemDetails>(error)) {
    return {};
  }

  const errors = error.response?.data?.errors;
  if (!errors || typeof errors !== "object") {
    return {};
  }

  const result: Record<string, string[]> = {};
  for (const [key, value] of Object.entries(errors)) {
    if (Array.isArray(value)) {
      result[key] = value;
      const lower = key.charAt(0).toLowerCase() + key.slice(1);
      result[lower] = value;
    }
  }

  return result;
}

export function getApiErrorMessage(error: unknown): string {
  if (!axios.isAxiosError<ProblemDetails>(error)) {
    return error instanceof Error
      ? error.message
      : "Đã xảy ra lỗi không xác định.";
  }

  const data = error.response?.data;
  const status = error.response?.status;

  if (status === 423 || (data?.detail && /Account is locked until/i.test(data.detail))) {
    const match = data?.detail?.match(/Account is locked until\s+([^\s\.]+)/i);
    if (match) {
      const lockUntil = new Date(match[1]);
      if (!Number.isNaN(lockUntil.getTime())) {
        const diffMs = lockUntil.getTime() - Date.now();
        const minutes = Math.max(1, Math.ceil(diffMs / 60000));
        const timeStr = lockUntil.toLocaleTimeString("vi-VN", {
          hour: "2-digit",
          minute: "2-digit",
        });
        return `Tài khoản đang bị tạm khóa để bảo vệ an toàn do nhập sai nhiều lần. Vui lòng thử lại sau khoảng ${minutes} phút (vào lúc ${timeStr}).`;
      }
    }
    return "Tài khoản đang bị tạm khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau.";
  }

  if (data?.errors) {
    return Object.values(data.errors).flat().join(" ");
  }

  return (
    data?.detail ??
    data?.error ??
    data?.title ??
    (error.response
      ? `Yêu cầu thất bại (${error.response.status}).`
      : "Không thể kết nối máy chủ. Vui lòng kiểm tra API và thử lại.")
  );
}
