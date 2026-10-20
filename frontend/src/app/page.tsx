import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { RecipeGrid } from "@/components/recipes/recipe-grid";

const featuredRecipes = [
  {
    id: "1",
    title: "Phở bò Hà Nội",
    slug: "pho-bo-ha-noi",
    description: "Nước dùng đậm đà, thơm mùi quế và gừng, phù hợp cho buổi sáng mát lành.",
    thumbnailUrl: null,
    cookTimeMinutes: 60,
    difficulty: "Medium" as const,
    categoryName: "Món chính",
  },
  {
    id: "2",
    title: "Bún chả Hà Nội",
    slug: "bun-cha-ha-noi",
    description: "Bún chả chuẩn vị, thịt nướng thơm, chấm với mắm và mùi tàu.",
    thumbnailUrl: null,
    cookTimeMinutes: 45,
    difficulty: "Easy" as const,
    categoryName: "Món nước",
  },
  {
    id: "3",
    title: "Canh chua cá",
    slug: "canh-chua-ca",
    description: "Canh chua thanh mát, chua nhẹ, hương thơm từ dứa và rau thơm.",
    thumbnailUrl: null,
    cookTimeMinutes: 35,
    difficulty: "Easy" as const,
    categoryName: "Món canh",
  },
];

export default function HomePage() {
  return (
    <MainLayout>
      <section className="hero-section">
        <div>
          <span className="pill">Culinary Blog</span>
          <h1>Khám phá món ăn Việt ngon, dễ làm và đầy cảm hứng.</h1>
          <p>
            Từ món ăn gia đình đến món ăn sáng tạo, mọi công thức đều được sắp xếp
            rõ ràng để bạn nấu thật dễ dàng.
          </p>
          <div className="hero-actions">
            <Link href="/recipes" className="button button-primary">
              Xem công thức
            </Link>
            <Link href="/recipes/search" className="button button-ghost">
              Tìm kiếm ngay
            </Link>
          </div>
        </div>
        <div className="hero-panel">
          <h3>Xu hướng tuần này</h3>
          <ul>
            <li>Phở bò Hà Nội</li>
            <li>Bánh xèo miền Trung</li>
            <li>Bún chả giòn thơm</li>
            <li>Gà nướng mật ong</li>
          </ul>
        </div>
      </section>

      <section className="section-block">
        <div className="section-heading">
          <h2>Featured recipes</h2>
          <Link href="/recipes">Xem tất cả</Link>
        </div>
        <RecipeGrid items={featuredRecipes} />
      </section>
    </MainLayout>
  );
}
