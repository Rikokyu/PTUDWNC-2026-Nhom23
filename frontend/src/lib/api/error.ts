import axios from "axios";

interface ProblemDetails {
  title?: string;
  detail?: string;
  error?: string;
  errors?: Record<string, string[]>;
}

export function getApiErrorMessage(error: unknown): string {
  if (!axios.isAxiosError<ProblemDetails>(error)) {
    return error instanceof Error
      ? error.message
      : "Đã xảy ra lỗi không xác định.";
  }

  const data = error.response?.data;
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