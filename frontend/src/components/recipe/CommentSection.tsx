"use client";

export function CommentSection({ recipeId }: { recipeId: string }) {
  return (
    <section aria-label="Comments">
      <h2>Binh luan</h2>
      <p>Recipe: {recipeId}</p>
    </section>
  );
}
