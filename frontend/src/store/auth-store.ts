import { create } from "zustand";
import { authApi, type AuthUser } from "@/lib/api/auth";

type AuthState = {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, displayName: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
};

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  isLoading: false,
  login: async (email, password) => {
    set({ isLoading: true });
    try {
      const user = await authApi.login({ email, password });
      set({ user, isAuthenticated: true });
    } catch (error) {
      set({ user: null, isAuthenticated: false });
      throw error;
    } finally {
      set({ isLoading: false });
    }
  },
  register: async (email, displayName, password) => {
    set({ isLoading: true });
    try {
      await authApi.register({ email, displayName, password });
    } finally {
      set({ isLoading: false });
    }
  },
  logout: async () => {
    set({ isLoading: true });
    try {
      await authApi.logout();
    } finally {
      set({ user: null, isAuthenticated: false, isLoading: false });
    }
  },
}));
