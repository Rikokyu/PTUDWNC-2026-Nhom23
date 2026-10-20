# Đối chiếu yêu cầu frontend

Phạm vi: tiện ích frontend, thư viện giao diện, store, giao diện tĩnh,
component dùng chung và bố cục của blog app.

| Yêu cầu | Trạng thái | Mã nguồn minh chứng |
| --- | --- | --- |
| Tiện ích gọi API và xử lý lỗi | Hoàn thành | `frontend/src/lib/api/client.ts`, `axios.ts`, `error.ts`, `frontend/src/lib/utils.ts` |
| Cài đặt shadcn/ui | Hoàn thành | `frontend/components.json`, `frontend/package.json`, `frontend/src/components/ui/` |
| Store quản lý trạng thái | Hoàn thành | `frontend/src/store/useAuthStore.ts`, `useUIStore.ts`, `dashboard-preview-store.ts` |
| Giao diện tĩnh cho blog app | Hoàn thành trong phạm vi các trang dưới đây | `frontend/src/app/`, `frontend/src/components/dashboard/` |
| Component dùng chung | Hoàn thành | Header, Footer, RecipeCard, Pagination, Loading, ErrorMessage, EmptyState, Dialog và các thành phần form |
| Layout chung | Hoàn thành | Root layout, MainLayout, layout xác thực và dashboard; điều hướng desktop và mobile |

## Các màn hình

- Trang chủ; đăng nhập và đăng ký.
- Danh sách công thức, tìm kiếm và chi tiết công thức.
- Danh sách và chi tiết danh mục.
- Dashboard tổng quan và trang tạo công thức đã có sẵn.
- Quản lý công thức: thống kê, tìm kiếm, lọc danh mục/trạng thái, phân trang,
  xem trước, lưu trữ/khôi phục và xác nhận xóa.
- Chỉnh sửa công thức: thông tin món ăn, nguyên liệu, hướng dẫn nhiều bước,
  trạng thái, kiểm tra dữ liệu, tóm tắt trực tiếp và hủy thay đổi.
- Quản lý danh mục: danh sách, số công thức, tìm kiếm, thêm/sửa/xóa qua dialog,
  kiểm tra trùng tên/slug, chặn xóa danh mục đang chứa công thức.
- Hồ sơ dashboard: tên hiển thị, giới thiệu, nơi sống, website,
  thông tin tài khoản và xem trước hồ sơ trực tiếp.

## Phạm vi dữ liệu minh họa

Bốn trang dashboard vừa hoàn thiện sử dụng dữ liệu minh họa, có thông báo
**Bản xem trước giao diện** ngay trên trang. Thay đổi được lưu bằng Zustand
trong localStorage (`culinary-dashboard-preview-v1`), không gửi lệnh ghi dữ liệu
lên máy chủ. Bản xem trước hồ sơ được phân theo ID tài khoản đăng nhập.

- Mở `/dashboard/recipes` để thử các thao tác quản lý mẫu.
- Nút **Thêm công thức minh họa** mở editor cho bản mới; các dòng công thức
  mẫu mở editor theo ID tương ứng.
- `/dashboard/categories` vẫn chỉ dành cho Admin theo kiểm soát truy cập có sẵn.
- Editor hiển thị trạng thái không tìm thấy nếu ID không thuộc dữ liệu mẫu.
- Trang tạo công thức thật `/dashboard/recipes/new` và luồng xác thực có sẵn
  vẫn dùng API; dữ liệu tạo qua API không được trộn vào danh sách minh họa.

Việc hoàn thành giao diện tĩnh không đồng nghĩa bốn màn hình này đã có luồng
CRUD thực tế trên backend. Tích hợp API cho các thao tác ghi là công việc riêng.

## Kiểm tra

Chạy tại thư mục `frontend`:

```sh
npm run typecheck
npm run lint
npm run build
```

Checklist trình diễn: tìm kiếm ra kết quả rỗng; đổi trang; thêm/chỉnh sửa công thức;
thêm/xóa dòng nguyên liệu và bước; hủy thay đổi; xác nhận hoặc hủy xóa;
kiểm tra slug trùng; kiểm tra danh mục có công thức; sửa hồ sơ và tải lại trang;
kiểm tra điều hướng trên màn hình nhỏ.

Kết quả xác minh ngày 20/10/2026:

- TypeScript và production build thành công.
- ESLint: 0 lỗi; còn 14 cảnh báo ở mã có sẵn.
- Kiểm tra trình duyệt với phiên đăng nhập giả lập: tìm kiếm, phân trang,
  form công thức, thêm/xóa dòng, lưu và tải lại, hủy/xác nhận xóa,
  chặn slug trùng, chặn xóa danh mục có công thức và lưu hồ sơ đều đạt.
- Kiểm tra ảnh desktop và mobile; không tràn chiều ngang trang ở độ rộng 390 px.
- Không ghi nhận lỗi JavaScript trong các luồng giao diện được kiểm tra.
- Author bị chặn ở trang quản lý danh mục; khách được chuyển về đăng nhập.

Các kiểm tra trình duyệt trên dùng dữ liệu mô phỏng, không xác nhận các thao tác
ghi trên backend thật.
