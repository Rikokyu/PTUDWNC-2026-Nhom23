import Link from "next/link";

export function Footer() {
  return (
    <footer className="site-footer">
      <div className="container footer-grid">
        <div>
          <h3>Culinary Blog</h3>
          <p className="muted">
            Chia sẻ công thức, mẹo nấu ăn và trải nghiệm ẩm thực mỗi ngày.
          </p>
        </div>
        <div>
          <h4>Liên kết</h4>
          <ul>
            <li>
              <Link href="/">Trang chủ</Link>
            </li>
            <li>
              <Link href="/recipes">Công thức</Link>
            </li>
            <li>
              <Link href="/recipes/search">Tìm kiếm</Link>
            </li>
          </ul>
        </div>
        <div>
          <h4>Liên hệ</h4>
          <ul>
            <li>hello@culinaryblog.vn</li>
            <li>+84 28 1234 5678</li>
          </ul>
        </div>
      </div>
    <p className="container muted">© {new Date().getFullYear()} Culinary Blog. Mọi quyền được bảo lưu.</p>
    </footer>
  );
}
