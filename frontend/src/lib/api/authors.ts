import apiClient from "./axios";
import type { AuthorProfileDto } from "@/types/api";

export const authorsApi = {
  getProfile: async (authorId: string, page = 1): Promise<AuthorProfileDto> => {
    const response = await apiClient.get<AuthorProfileDto>(
      `/api/v1/authors/${encodeURIComponent(authorId)}`,
      { params: { page, pageSize: 12 } },
    );
    return response.data;
  },
};
