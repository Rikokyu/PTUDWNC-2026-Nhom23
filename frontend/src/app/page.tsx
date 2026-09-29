"use client";

import React, { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { RecipeCard } from "@/components/common/RecipeCard";
import { CategoryPill } from "@/components/common/CategoryPill";
import { SearchBar } from "@/components/common/SearchBar";
import { MOCK_RECIPES, MOCK_CATEGORIES } from "@/lib/mockData";
import { ChefHat, ArrowRight, Sparkles } from "lucide-react";

export default function HomePage() {
  const [selectedCategory, setSelectedCategory] = useState<string>("all");

  const featuredRecipes = MOCK_RECIPES.filter((r) => r.isFeatured);
  const latestRecipes =
    selectedCategory === "all"
      ? MOCK_RECIPES
      : MOCK_RECIPES.filter((r) => r.categorySlug === selectedCategory);

  return (
    <div className="min-h-screen flex flex-col bg-white">
      <Navbar />

      {/* HERO SECTION */}
      <section className="relative pt-12 pb-16 lg:pt-20 lg:pb-24 overflow-hidden border-b border-slate-100">
        <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
            {/* Left Content */}
            <div className="lg:col-span-7 space-y-6 text-left">
              <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-brand-light text-brand-hover text-xs font-semibold border border-brand-border/40">
                {/* Pastel Purple micro detail (<2% color budget) */}
                <span className="w-2 h-2 rounded-full bg-pastel-purple animate-pulse" />
                <span>Nền Tảng Nấu Ăn & Chia Sẻ Công Thức #1</span>
              </div>

              <h1 className="text-3xl sm:text-4xl lg:text-5xl font-extrabold text-content-primary tracking-tight leading-[1.15]">
                Khám phá hương vị <br className="hidden sm:inline" />
                <span className="text-brand">bữa ăn gia đình</span> mỗi ngày
              </h1>

              <p className="text-base sm:text-lg text-content-secondary max-w-xl leading-relaxed">
                Hàng ngàn công thức nấu ăn chuẩn vị, hướng dẫn chi tiết từng bước từ các đầu bếp gia đình và chuyên gia ẩm thực Việt.
              </p>

              {/* Quick Search Input */}
              <div className="pt-2 max-w-xl">
                <SearchBar
                  onSearch={(query) => {
                    if (query) {
                      window.location.href = `/recipes/search?q=${encodeURIComponent(query)}`;
                    }
                  }}
                />
              </div>

              {/* Hero Stats */}
              <div className="pt-4 grid grid-cols-3 gap-6 max-w-lg border-t border-slate-100">
                <div>
                  <p className="text-xl font-extrabold text-content-primary">1,200+</p>
                  <p className="text-xs text-content-tertiary">Công thức đã kiểm duyệt</p>
                </div>
                <div>
                  <p className="text-xl font-extrabold text-content-primary">45,000+</p>
                  <p className="text-xs text-content-tertiary">Thành viên bếp nấu</p>
                </div>
                <div>
                  <p className="text-xl font-extrabold text-content-primary">4.9★</p>
                  <p className="text-xs text-content-tertiary">Đánh giá cộng đồng</p>
                </div>
              </div>
            </div>

            {/* Right Hero Presentation Card using public/images/image 1.png */}
            <div className="lg:col-span-5 relative">
              <div className="relative rounded-2xl overflow-hidden shadow-card border border-slate-200 group bg-slate-100">
                <img
                  src="/images/image 1.png"
                  alt="Ẩm thực Việt Nam tươi ngon"
                  className="w-full aspect-[4/3] object-cover transition-transform duration-500 group-hover:scale-102"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-slate-950/70 via-slate-950/10 to-transparent" />
                
                <div className="absolute bottom-5 left-5 right-5 text-white p-2">
                  <Badge variant="accent" size="sm" className="mb-2">
                    Món Nổi Bật
                  </Badge>
                  <h3 className="text-lg font-bold leading-snug">
                    Bữa Ăn Chuẩn Vị - Gắn Kết Yêu Thương
                  </h3>
                  <p className="text-xs text-slate-200 mt-1 line-clamp-1">
                    Công thức chi tiết, nguyên liệu dễ tìm cho gian bếp Việt.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CATEGORY SELECTION STRIP */}
      <section className="py-8 bg-slate-50/60 border-b border-slate-200/60">
        <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between gap-4 mb-4">
            <h3 className="text-xs font-bold uppercase tracking-wider text-content-tertiary">
              Danh mục ẩm thực phổ biến
            </h3>
            <Link
              href="/categories"
              className="text-xs font-semibold text-brand hover:text-brand-hover inline-flex items-center gap-1"
            >
              Xem tất cả danh mục →
            </Link>
          </div>

          <div className="flex items-center gap-2 overflow-x-auto pb-2 no-scrollbar">
            <CategoryPill
              name="Tất cả"
              slug="all"
              isActive={selectedCategory === "all"}
              onClick={() => setSelectedCategory("all")}
            />
            {MOCK_CATEGORIES.map((cat) => (
              <CategoryPill
                key={cat.id}
                name={cat.name}
                slug={cat.slug}
                count={cat.recipeCount}
                isActive={selectedCategory === cat.slug}
                onClick={() => setSelectedCategory(cat.slug)}
              />
            ))}
          </div>
        </div>
      </section>

      {/* FEATURED RECIPES SECTION */}
      <section className="py-16 bg-white">
        <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col md:flex-row md:items-end justify-between mb-10 gap-4">
            <div>
              <div className="inline-flex items-center gap-1.5 text-xs font-bold text-accent uppercase tracking-wider mb-2">
                <Sparkles className="w-4 h-4" />
                <span>Gợi ý hôm nay</span>
              </div>
              <h2 className="text-2xl sm:text-3xl font-bold text-content-primary tracking-tight">
                Công Thức Nổi Bật Tuần Này
              </h2>
            </div>
            <Link href="/recipes">
              <Button variant="outline" size="sm" rightIcon={<ArrowRight className="w-4 h-4" />}>
                Khám phá tất cả công thức
              </Button>
            </Link>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {featuredRecipes.map((recipe) => (
              <RecipeCard key={recipe.id} {...recipe} />
            ))}
          </div>
        </div>
      </section>

      {/* CATEGORIES GRID SECTION */}
      <section className="py-16 bg-slate-50/50 border-y border-slate-200/60">
        <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center max-w-xl mx-auto mb-12 space-y-2">
            <h2 className="text-2xl sm:text-3xl font-bold text-content-primary tracking-tight">
              Khám Phá Theo Chủ Đề Nấu Ăn
            </h2>
            <p className="text-xs sm:text-sm text-content-secondary">
              Tìm kiếm nhanh chóng món ăn phù hợp cho từng dịp và khẩu vị gia đình.
            </p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {MOCK_CATEGORIES.map((category) => (
              <Link
                key={category.id}
                href={`/categories/${category.slug}`}
                className="group relative rounded-xl overflow-hidden border border-slate-200 bg-white p-6 shadow-subtle hover:shadow-card hover:border-slate-300 transition-all duration-200 flex flex-col justify-between"
              >
                <div className="flex items-start justify-between gap-4 mb-4">
                  <div>
                    <h3 className="text-lg font-bold text-content-primary group-hover:text-brand transition-colors">
                      {category.name}
                    </h3>
                    <p className="text-xs text-content-tertiary mt-1">
                      {category.recipeCount} công thức ngon
                    </p>
                  </div>
                  <div className="w-10 h-10 rounded-lg bg-brand-light text-brand-hover flex items-center justify-center shrink-0 border border-brand-border/30">
                    <ChefHat className="w-5 h-5" />
                  </div>
                </div>

                <p className="text-xs text-content-secondary leading-relaxed mb-4">
                  {category.description}
                </p>

                <div className="text-xs font-semibold text-brand inline-flex items-center gap-1 group-hover:translate-x-1 transition-transform">
                  Xem công thức →
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* LATEST RECIPES GRID */}
      <section className="py-16 bg-white">
        <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between mb-8 gap-4">
            <div>
              <h2 className="text-2xl font-bold text-content-primary tracking-tight">
                Món Ăn Mới Nhất
              </h2>
              <p className="text-xs text-content-tertiary mt-1">
                Các công thức mới được chia sẻ bởi cộng đồng bếp Culinary Blog
              </p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {latestRecipes.map((recipe) => (
              <RecipeCard key={recipe.id} {...recipe} />
            ))}
          </div>
        </div>
      </section>

      {/* COMMUNITY CTA BANNER SECTION */}
      <section className="py-16 bg-slate-900 text-white">
        <div className="max-w-[1280px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-center">
            <div className="lg:col-span-8 space-y-4">
              <Badge variant="brand" className="bg-brand text-white border-none">
                Cùng chia sẻ công thức
              </Badge>
              <h2 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
                Bạn Có Công Thức Nấu Ăn Độc Đáo?
              </h2>
              <p className="text-sm text-slate-300 max-w-xl leading-relaxed">
                Đăng bài chia sẻ món ngon gia truyền của bạn tới hàng chục nghìn người yêu ẩm thực trên khắp cả nước.
              </p>
            </div>

            <div className="lg:col-span-4 flex flex-col sm:flex-row lg:flex-col gap-3 justify-end">
              <Link href="/dashboard/recipes/new">
                <Button variant="primary" size="lg" className="w-full justify-center">
                  Đăng công thức mới ngay
                </Button>
              </Link>
              <Link href="/about">
                <Button variant="outline" size="lg" className="w-full justify-center text-white border-slate-700 hover:bg-slate-800">
                  Tìm hiểu thêm
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </section>

      <Footer />
    </div>
  );
}
