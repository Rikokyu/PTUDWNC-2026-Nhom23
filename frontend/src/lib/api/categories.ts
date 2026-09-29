import apiClient from "./axios";
import { MockCategory, MOCK_CATEGORIES } from "@/lib/mockData";

export const categoryApi = {
  getAll: async (): Promise<MockCategory[]> => {
    try {
      const response = await apiClient.get<MockCategory[]>("/api/v1/categories");
      return response.data;
    } catch {
      // Fallback to mock categories when backend API is offline
      return MOCK_CATEGORIES;
    }
  },
  getBySlug: async (slug: string): Promise<MockCategory | undefined> => {
    try {
      const response = await apiClient.get<MockCategory>(`/api/v1/categories/${slug}`);
      return response.data;
    } catch {
      return MOCK_CATEGORIES.find((c) => c.slug === slug);
    }
  },
};
