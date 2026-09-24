import { z } from "zod";

export const createRecipeSchema = z.object({
  title: z.string().min(5).max(200),
  description: z.string().max(500).optional(),
  categoryId: z.string().min(1),
  prepTimeMinutes: z.number().int().min(1).max(1440),
  cookTimeMinutes: z.number().int().min(1).max(1440),
  servings: z.number().int().min(1).max(50),
  difficulty: z.enum(["Easy", "Medium", "Hard"]),
  steps: z
    .array(
      z.object({
        description: z.string().min(10),
        durationMinutes: z.number().int().min(1).optional(),
      }),
    )
    .min(1),
  ingredients: z
    .array(
      z.object({
        name: z.string().min(2),
        quantity: z.number().positive(),
        unit: z.string().min(1),
        notes: z.string().optional(),
      }),
    )
    .min(1),
});

export type CreateRecipeFormData = z.infer<typeof createRecipeSchema>;
