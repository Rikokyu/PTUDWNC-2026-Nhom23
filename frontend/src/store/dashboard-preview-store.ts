"use client";

import { create } from "zustand";
import { persist } from "zustand/middleware";

export type PreviewCategory = {
  id: string;
  name: string;
  slug: string;
  description: string;
  emoji: string;
};
export type PreviewRecipe = {
  id: string;
  title: string;
  description: string;
  categoryId: string;
  prepTime: number;
  cookTime: number;
  servings: number;
  difficulty: "Easy" | "Medium" | "Hard" | "Expert";
  status: "Draft" | "Published" | "Archived";
  ingredients: string[];
  steps: string[];
  updatedAt: string;
};
export type PreviewProfile = {
  displayName: string;
  bio: string;
  website: string;
  location: string;
};

const categories: PreviewCategory[] = [
  {
    id: "main",
    name: "Món chính",
    slug: "mon-chinh",
    description: "Món ngon cho bữa cơm đủ đầy, ấm cúng.",
    emoji: "🍲",
  },
  {
    id: "breakfast",
    name: "Bữa sáng",
    slug: "bua-sang",
    description: "Bắt đầu ngày mới bằng một bữa ăn ngon.",
    emoji: "🥐",
  },
  {
    id: "dessert",
    name: "Tráng miệng",
    slug: "trang-mieng",
    description: "Một chút ngọt ngào sau mỗi bữa ăn.",
    emoji: "🍰",
  },
  {
    id: "vegetarian",
    name: "Món chay",
    slug: "mon-chay",
    description: "Thanh đạm, tươi ngon từ rau củ theo mùa.",
    emoji: "🥗",
  },
  {
    id: "drinks",
    name: "Đồ uống",
    slug: "do-uong",
    description: "Thức uống mát lành và dễ làm tại nhà.",
    emoji: "🍹",
  },
];
const recipes: PreviewRecipe[] = [
  {
    id: "preview-pho",
    title: "Phở bò Hà Nội",
    description:
      "Nước dùng trong, ngọt thanh với hương quế và hồi. Món ăn thân quen cho những buổi sáng cuối tuần.",
    categoryId: "main",
    prepTime: 30,
    cookTime: 90,
    servings: 4,
    difficulty: "Medium",
    status: "Published",
    ingredients: [
      "500 g thịt bò",
      "1 kg xương bò",
      "400 g bánh phở",
      "Gừng, hành tây, quế và hoa hồi",
    ],
    steps: [
      "Chần xương, rửa sạch và ninh cùng gừng, hành tây nướng.",
      "Nêm nước dùng, chần bánh phở và thái thịt bò mỏng.",
      "Xếp nguyên liệu vào tô, chan nước dùng nóng và thêm rau thơm.",
    ],
    updatedAt: "2026-10-18",
  },
  {
    id: "preview-salad",
    title: "Salad rau củ mùa thu",
    description: "Rau củ giòn ngọt, dùng cùng sốt chanh nhẹ nhàng.",
    categoryId: "vegetarian",
    prepTime: 15,
    cookTime: 0,
    servings: 2,
    difficulty: "Easy",
    status: "Published",
    ingredients: [
      "200 g xà lách",
      "1 quả dưa leo",
      "10 quả cà chua bi",
      "2 thìa dầu ô liu và nước cốt chanh",
    ],
    steps: [
      "Rửa sạch và để ráo rau củ.",
      "Trộn sốt, rưới lên rau và thưởng thức ngay.",
    ],
    updatedAt: "2026-10-17",
  },
  {
    id: "preview-banh",
    title: "Bánh chuối nướng",
    description: "Bánh thơm mềm, vị ngọt tự nhiên từ chuối chín.",
    categoryId: "dessert",
    prepTime: 20,
    cookTime: 40,
    servings: 6,
    difficulty: "Medium",
    status: "Draft",
    ingredients: ["4 quả chuối chín", "200 g bột mì", "2 quả trứng"],
    steps: [
      "Nghiền chuối và trộn với trứng, bột mì.",
      "Đổ vào khuôn và nướng đến khi bánh chín vàng.",
    ],
    updatedAt: "2026-10-16",
  },
  {
    id: "preview-toast",
    title: "Bánh mì bơ trứng",
    description: "Bữa sáng nhanh gọn, đủ năng lượng cho ngày mới.",
    categoryId: "breakfast",
    prepTime: 5,
    cookTime: 10,
    servings: 1,
    difficulty: "Easy",
    status: "Draft",
    ingredients: ["2 lát bánh mì", "1 quả trứng", "Nửa quả bơ"],
    steps: [
      "Nướng bánh mì, nghiền bơ và chiên trứng.",
      "Phết bơ lên bánh, thêm trứng và nêm tiêu.",
    ],
    updatedAt: "2026-10-15",
  },
  {
    id: "preview-tra",
    title: "Trà đào cam sả",
    description: "Hương trà nhẹ, vị đào ngọt và sả thơm dịu.",
    categoryId: "drinks",
    prepTime: 10,
    cookTime: 5,
    servings: 2,
    difficulty: "Easy",
    status: "Published",
    ingredients: [
      "2 túi trà",
      "1 quả cam",
      "2 nhánh sả",
      "Đào ngâm và đá viên",
    ],
    steps: ["Ủ trà cùng sả, để nguội.", "Thêm đào, cam và đá, khuấy đều."],
    updatedAt: "2026-10-14",
  },
  {
    id: "preview-soup",
    title: "Súp bí đỏ kem tươi",
    description: "Món súp ấm nóng cho những ngày mưa.",
    categoryId: "main",
    prepTime: 15,
    cookTime: 25,
    servings: 3,
    difficulty: "Easy",
    status: "Archived",
    ingredients: ["500 g bí đỏ", "100 ml kem tươi", "1 củ hành tây"],
    steps: [
      "Xào hành, thêm bí đỏ và nước dùng, nấu mềm.",
      "Xay nhuyễn, thêm kem và nêm vừa ăn.",
    ],
    updatedAt: "2026-10-12",
  },
];

type PreviewState = {
  categories: PreviewCategory[];
  recipes: PreviewRecipe[];
  profiles: Record<string, PreviewProfile>;
  saveCategory: (category: PreviewCategory) => void;
  deleteCategory: (id: string) => void;
  saveRecipe: (recipe: PreviewRecipe) => void;
  deleteRecipe: (id: string) => void;
  saveProfile: (userId: string, profile: PreviewProfile) => void;
};

export const useDashboardPreviewStore = create<PreviewState>()(
  persist(
    (set) => ({
      categories,
      recipes,
      profiles: {},
      saveCategory: (category) =>
        set((state) => ({
          categories: state.categories.some((item) => item.id === category.id)
            ? state.categories.map((item) =>
                item.id === category.id ? category : item,
              )
            : [...state.categories, category],
        })),
      deleteCategory: (id) =>
        set((state) => ({
          categories: state.categories.filter((item) => item.id !== id),
        })),
      saveRecipe: (recipe) =>
        set((state) => ({
          recipes: state.recipes.some((item) => item.id === recipe.id)
            ? state.recipes.map((item) =>
                item.id === recipe.id ? recipe : item,
              )
            : [recipe, ...state.recipes],
        })),
      deleteRecipe: (id) =>
        set((state) => ({
          recipes: state.recipes.filter((item) => item.id !== id),
        })),
      saveProfile: (userId, profile) =>
        set((state) => ({
          profiles: { ...state.profiles, [userId]: profile },
        })),
    }),
    { name: "culinary-dashboard-preview-v1", skipHydration: true },
  ),
);
