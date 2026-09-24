import { create } from "zustand";
import { persist } from "zustand/middleware";

interface UIState {
  sidebarOpen: boolean;
  activeModal: string | null;
  recipesViewMode: "grid" | "list";
  toggleSidebar: () => void;
  openModal: (id: string) => void;
  closeModal: () => void;
  setRecipesViewMode: (mode: "grid" | "list") => void;
}

export const useUIStore = create<UIState>()(
  persist(
    (set) => ({
      sidebarOpen: false,
      activeModal: null,
      recipesViewMode: "grid",
      toggleSidebar: () =>
        set((state) => ({ sidebarOpen: !state.sidebarOpen })),
      openModal: (id) => set({ activeModal: id }),
      closeModal: () => set({ activeModal: null }),
      setRecipesViewMode: (mode) => set({ recipesViewMode: mode }),
    }),
    {
      name: "culinary-blog-ui",
      partialize: (state) => ({ recipesViewMode: state.recipesViewMode }),
    },
  ),
);
