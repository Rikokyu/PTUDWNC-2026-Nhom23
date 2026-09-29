"use client";

import React from "react";
import Link from "next/link";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { MOCK_RECIPES } from "@/lib/mockData";
import { ChefHat, Plus, Eye, Heart, Star, BookOpen, Clock, ArrowUpRight } from "lucide-react";

export default function DashboardOverviewPage() {
  const userRecipes = MOCK_RECIPES.slice(0, 3);

  return (
    <div className="space-y-8">
      {/* Welcome Banner */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-6 bg-white rounded-xl border border-slate-200 shadow-subtle">
        <div className="space-y-1">
          <h1 className="text-2xl font-bold text-content-primary">
            Xin chào, Minh Thư 👋
          </h1>
          <p className="text-xs text-content-secondary">
            Chào mừng bạn quay trở lại gian bếp. Bạn có 3 công thức đã xuất bản và 215 lượt yêu thích.
          </p>
        </div>
        <Link href="/dashboard/recipes/new" className="shrink-0">
          <Button variant="primary" size="md" leftIcon={<Plus className="w-4 h-4" />}>
            Đăng công thức mới
          </Button>
        </Link>
      </div>

      {/* Metrics Row - Clean text & subtle status indicators */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        <div className="p-5 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-2">
          <div className="flex items-center justify-between text-xs text-content-tertiary font-semibold uppercase tracking-wider">
            <span>Công thức đã đăng</span>
            <ChefHat className="w-4 h-4 text-brand" />
          </div>
          <p className="text-2xl font-extrabold text-content-primary">8 bài</p>
          <p className="text-[11px] text-brand-hover font-medium">3 bài được đưa lên trang nổi bật</p>
        </div>

        <div className="p-5 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-2">
          <div className="flex items-center justify-between text-xs text-content-tertiary font-semibold uppercase tracking-wider">
            <span>Lượt yêu thích</span>
            <Heart className="w-4 h-4 text-rose-500" />
          </div>
          <p className="text-2xl font-extrabold text-content-primary">481 lượt</p>
          <p className="text-[11px] text-content-tertiary">Tăng 12% so với tháng trước</p>
        </div>

        <div className="p-5 bg-white rounded-xl border border-slate-200 shadow-subtle space-y-2">
          <div className="flex items-center justify-between text-xs text-content-tertiary font-semibold uppercase tracking-wider">
            <span>Đánh giá trung bình</span>
            <Star className="w-4 h-4 text-accent fill-accent" />
          </div>
          <p className="text-2xl font-extrabold text-content-primary">4.85 / 5</p>
          <p className="text-[11px] text-content-tertiary">Dựa trên 140 đánh giá</p>
        </div>
      </div>

      {/* Recent Activity / Recipe List Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-subtle overflow-hidden space-y-0">
        <div className="p-5 border-b border-slate-100 flex items-center justify-between">
          <h2 className="text-base font-bold text-content-primary">
            Công thức nấu ăn của bạn
          </h2>
          <Link
            href="/dashboard/recipes"
            className="text-xs font-semibold text-brand hover:text-brand-hover inline-flex items-center gap-1"
          >
            Quản lý tất cả →
          </Link>
        </div>

        <div className="divide-y divide-slate-100">
          {userRecipes.map((r) => (
            <div key={r.id} className="p-4 sm:p-5 flex items-center justify-between gap-4 hover:bg-slate-50/60 transition-colors">
              <div className="flex items-center gap-4 min-w-0">
                <img
                  src={r.primaryImageUrl}
                  alt={r.title}
                  className="w-14 h-14 rounded-lg object-cover border border-slate-200 shrink-0"
                />
                <div className="min-w-0">
                  <h3 className="text-sm font-bold text-content-primary truncate">
                    {r.title}
                  </h3>
                  <div className="flex items-center gap-3 text-xs text-content-tertiary mt-1">
                    <Badge variant="neutral" size="sm">
                      {r.categoryName}
                    </Badge>
                    <span className="flex items-center gap-1">
                      <Clock className="w-3 h-3" /> {r.prepTimeMinutes + r.cookTimeMinutes} phút
                    </span>
                    <span className="hidden sm:inline-block">• {r.likeCount} thích</span>
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-2 shrink-0">
                <Link href={`/recipes/${r.slug}`}>
                  <Button variant="outline" size="sm" rightIcon={<ArrowUpRight className="w-3.5 h-3.5" />}>
                    Xem bài
                  </Button>
                </Link>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
