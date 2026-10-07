"use client";

import React, { useState, use } from "react";
import Link from "next/link";
import { Breadcrumb } from "@/components/ui/Breadcrumb";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { RatingStars } from "@/components/common/RatingStars";
import { AuthorAvatar } from "@/components/common/AuthorAvatar";
import { RecipeCard } from "@/components/common/RecipeCard";
import { MOCK_RECIPES } from "@/lib/mockData";
import {
  Clock,
  Flame,
  Users,
  Bookmark,
  Share2,
  Printer,
  CheckCircle2,
  ChefHat,
  MessageSquare,
  Send,
  Sparkles,
} from "lucide-react";
import { notFound } from "next/navigation";

export default function RecipeDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = use(params);

  const recipe = MOCK_RECIPES.find((r) => r.slug === slug) || MOCK_RECIPES[0];

  const [servings, setServings] = useState(recipe.servings);
  const [isSaved, setIsSaved] = useState(recipe.isLiked);
  const [checkedIngredients, setCheckedIngredients] = useState<Record<string, boolean>>({});
  const [newComment, setNewComment] = useState("");
  const [commentsList, setCommentsList] = useState([
    {
      id: "c-1",
      author: "Ngọc Mai",
      avatar: "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=100&q=80",
      content: "Công thức rất dễ làm, nước dùng đậm đà thanh ngọt đúng vị truyền thống. Cảm ơn tác giả!",
      createdAt: "2 ngày trước",
      rating: 5,
    },
    {
      id: "c-2",
      author: "Văn Đức",
      content: "Ninh xương đủ thời gian nước trong vắt luôn. Lần sau mình sẽ gia giảm thêm tí tiêu sỏi.",
      createdAt: "5 ngày trước",
      rating: 5,
    },
  ]);

  const toggleIngredient = (id: string) => {
    setCheckedIngredients((prev) => ({ ...prev, [id]: !prev[id] }));
  };

  const handleAddComment = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim()) return;
    setCommentsList([
      {
        id: `c-${Date.now()}`,
        author: "Bạn (Người dùng)",
        content: newComment.trim(),
        createdAt: "Vừa xong",
        rating: 5,
      },
      ...commentsList,
    ]);
    setNewComment("");
  };

  const relatedRecipes = MOCK_RECIPES.filter((r) => r.slug !== recipe.slug).slice(0, 3);

  // Scaled quantity multiplier
  const multiplier = servings / recipe.servings;

  return (
    <div className="space-y-10 pb-12">
      {/* Breadcrumb */}
      <Breadcrumb
        items={[
          { label: "Công thức", href: "/recipes" },
          { label: recipe.categoryName, href: `/categories/${recipe.categorySlug}` },
          { label: recipe.title },
        ]}
      />

      {/* HEADER SECTION */}
      <div className="space-y-4 max-w-4xl">
        <div className="flex items-center gap-3 flex-wrap">
          <Badge variant="brand" size="md">
            {recipe.categoryName}
          </Badge>
          {recipe.isFeatured && (
            <Badge variant="pastel" size="md">
              Món Nổi Bật
            </Badge>
          )}
          <span className="text-xs text-content-tertiary">
            Đăng ngày {new Date(recipe.createdAt).toLocaleDateString("vi-VN")}
          </span>
        </div>

        <h1 className="text-3xl sm:text-4xl font-extrabold text-content-primary tracking-tight leading-tight">
          {recipe.title}
        </h1>

        <p className="text-sm sm:text-base text-content-secondary leading-relaxed">
          {recipe.summary}
        </p>

        {/* Author & Action Row */}
        <div className="pt-2 flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-t border-b border-slate-100 py-4">
          <div className="flex items-center gap-4">
            <AuthorAvatar
              name={recipe.authorName}
              avatarUrl={recipe.authorAvatar}
              role={recipe.authorRole}
              size="md"
            />
            <div className="h-4 w-px bg-slate-200" />
            <RatingStars rating={recipe.rating} totalReviews={recipe.totalReviews} size="md" />
          </div>

          <div className="flex items-center gap-2">
            <Button
              variant={isSaved ? "primary" : "outline"}
              size="sm"
              onClick={() => setIsSaved(!isSaved)}
              leftIcon={<Bookmark className="w-4 h-4" />}
            >
              {isSaved ? "Đã lưu công thức" : "Lưu công thức"}
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => window.print()}
              leftIcon={<Printer className="w-4 h-4" />}
            >
              In bài
            </Button>
          </div>
        </div>
      </div>

      {/* FEATURED IMAGE */}
      <div className="relative rounded-2xl overflow-hidden border border-slate-200 shadow-card aspect-[16/9] max-h-[500px]">
        <img
          src={recipe.primaryImageUrl}
          alt={recipe.title}
          className="w-full h-full object-cover"
        />
      </div>

      {/* METRICS SUMMARY BAR */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 p-6 bg-slate-50 rounded-xl border border-slate-200 text-center">
        <div>
          <span className="text-[11px] font-semibold uppercase tracking-wider text-content-tertiary flex items-center justify-center gap-1 mb-1">
            <Clock className="w-3.5 h-3.5 text-brand" /> Chuẩn bị
          </span>
          <p className="text-base font-bold text-content-primary">{recipe.prepTimeMinutes} phút</p>
        </div>
        <div>
          <span className="text-[11px] font-semibold uppercase tracking-wider text-content-tertiary flex items-center justify-center gap-1 mb-1">
            <Flame className="w-3.5 h-3.5 text-accent" /> Nấu nướng
          </span>
          <p className="text-base font-bold text-content-primary">{recipe.cookTimeMinutes} phút</p>
        </div>
        <div>
          <span className="text-[11px] font-semibold uppercase tracking-wider text-content-tertiary flex items-center justify-center gap-1 mb-1">
            <Users className="w-3.5 h-3.5 text-brand" /> Khẩu phần
          </span>
          <p className="text-base font-bold text-content-primary">{servings} người ăn</p>
        </div>
        <div>
          <span className="text-[11px] font-semibold uppercase tracking-wider text-content-tertiary flex items-center justify-center gap-1 mb-1">
            <ChefHat className="w-3.5 h-3.5 text-slate-600" /> Độ khó
          </span>
          <p className="text-base font-bold text-content-primary">{recipe.difficulty}</p>
        </div>
      </div>

      {/* TWO COLUMN INGREDIENTS & STEPS */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-10">
        {/* LEFT COLUMN: INGREDIENTS LIST (4 cols) */}
        <div className="lg:col-span-4 space-y-6">
          <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-subtle space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-slate-100">
              <h2 className="text-lg font-bold text-content-primary">
                Nguyên liệu chuẩn bị
              </h2>
              {/* Servings Adjuster */}
              <div className="flex items-center gap-2 border border-slate-200 rounded-lg p-1 text-xs">
                <button
                  onClick={() => setServings(Math.max(1, servings - 1))}
                  className="w-5 h-5 flex items-center justify-center font-bold hover:bg-slate-100 rounded"
                >
                  -
                </button>
                <span className="font-semibold">{servings}</span>
                <button
                  onClick={() => setServings(servings + 1)}
                  className="w-5 h-5 flex items-center justify-center font-bold hover:bg-slate-100 rounded"
                >
                  +
                </button>
              </div>
            </div>

            <ul className="space-y-3">
              {recipe.ingredients.map((ing) => {
                const isChecked = !!checkedIngredients[ing.id];
                const adjustedQty = (ing.quantity * multiplier).toFixed(
                  ing.quantity * multiplier % 1 === 0 ? 0 : 1
                );
                return (
                  <li
                    key={ing.id}
                    onClick={() => toggleIngredient(ing.id)}
                    className="flex items-start gap-3 text-xs cursor-pointer select-none group"
                  >
                    <input
                      type="checkbox"
                      checked={isChecked}
                      onChange={() => {}}
                      className="mt-0.5 rounded border-slate-300 text-brand focus:ring-brand"
                    />
                    <div className="flex-1">
                      <span
                        className={
                          isChecked
                            ? "line-through text-slate-400"
                            : "text-content-primary font-medium group-hover:text-brand transition-colors"
                        }
                      >
                        {ing.name}
                      </span>
                      {ing.notes && (
                        <span className="text-[11px] text-content-tertiary block">
                          {ing.notes}
                        </span>
                      )}
                    </div>
                    <span className="font-semibold text-content-primary shrink-0">
                      {adjustedQty} {ing.unit}
                    </span>
                  </li>
                );
              })}
            </ul>
          </div>
        </div>

        {/* RIGHT COLUMN: INSTRUCTIONS / STEPS (8 cols) */}
        <div className="lg:col-span-8 space-y-6">
          <div className="space-y-4">
            <h2 className="text-xl font-bold text-content-primary pb-2 border-b border-slate-100">
              Các bước thực hiện
            </h2>

            <div className="space-y-6">
              {recipe.steps.map((step) => (
                <div
                  key={step.id}
                  className="p-6 rounded-xl border border-slate-200 bg-white shadow-subtle flex items-start gap-4 space-y-0"
                >
                  <div className="w-8 h-8 rounded-full bg-brand text-white font-bold flex items-center justify-center shrink-0 text-sm shadow-xs">
                    {step.stepNumber}
                  </div>
                  <div className="flex-1 space-y-2">
                    <div className="flex items-center justify-between">
                      <h3 className="text-sm font-bold text-content-primary">
                        Bước {step.stepNumber}
                      </h3>
                      {step.durationMinutes && (
                        <span className="text-xs text-content-tertiary flex items-center gap-1 font-medium">
                          <Clock className="w-3.5 h-3.5 text-slate-400" />
                          ~{step.durationMinutes} phút
                        </span>
                      )}
                    </div>
                    <p className="text-xs sm:text-sm text-content-secondary leading-relaxed">
                      {step.description}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* COMMENTS & REVIEWS SECTION */}
      <section className="pt-8 border-t border-slate-200 space-y-6 max-w-4xl">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-bold text-content-primary flex items-center gap-2">
            <MessageSquare className="w-5 h-5 text-brand" />
            Bình luận & Đánh giá ({commentsList.length})
          </h2>
        </div>

        {/* Add comment form */}
        <form onSubmit={handleAddComment} className="p-4 rounded-xl border border-slate-200 bg-slate-50/50 space-y-3">
          <textarea
            rows={3}
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            placeholder="Chia sẻ trải nghiệm làm món ăn này của bạn..."
            className="w-full p-3 text-xs sm:text-sm border border-slate-300 rounded-lg bg-white focus:outline-none focus:border-brand focus:ring-2 focus:ring-brand/20 transition-all placeholder:text-slate-400"
          />
          <div className="flex justify-end">
            <Button
              type="submit"
              variant="primary"
              size="sm"
              rightIcon={<Send className="w-3.5 h-3.5" />}
            >
              Gửi bình luận
            </Button>
          </div>
        </form>

        {/* Comments list */}
        <div className="space-y-4">
          {commentsList.map((c) => (
            <div key={c.id} className="p-4 rounded-xl border border-slate-200 bg-white space-y-2">
              <div className="flex items-center justify-between">
                <AuthorAvatar name={c.author} avatarUrl={c.avatar} size="sm" />
                <span className="text-[11px] text-content-tertiary">{c.createdAt}</span>
              </div>
              <p className="text-xs sm:text-sm text-content-secondary pl-8">{c.content}</p>
            </div>
          ))}
        </div>
      </section>

      {/* RELATED RECIPES GRID */}
      <section className="pt-12 border-t border-slate-200 space-y-6">
        <h2 className="text-2xl font-bold text-content-primary tracking-tight">
          Công Thức Tương Tự
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {relatedRecipes.map((r) => (
            <RecipeCard key={r.id} {...r} />
          ))}
        </div>
      </section>
    </div>
  );
}
