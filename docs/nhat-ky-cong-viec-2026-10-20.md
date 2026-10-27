# Nhật ký công việc — 20/10/2026

Tài liệu ghi lại công việc trong phiên trao đổi ngày 20/10/2026 và các hạng mục cần tiếp tục. Đây không phải xác nhận toàn bộ dự án đã hoàn thành.

## 1. Đọc tài liệu và xác định cấu hình database

- Đọc phần tổng quan SRS, tham khảo lịch sử làm việc và đọc `docs/chucnang.md`.
- Xác nhận kết nối có tên **web** trong pgAdmin:
  - Host: `localhost`.
  - Port: `5433`.
  - Database: `culinary_blog`.
  - Username: `admin`.
- `web` là tên hiển thị của kết nối trong pgAdmin, không phải hostname cần điền vào connection string.
- PostgreSQL chạy trong container `culinary-blog-postgres`.
- Kiểm tra `/health/ready`: HTTP 200, PostgreSQL Healthy.
- Không đổi database sang server PostgreSQL khác.

## 2. Sửa lỗi không đăng ký được tài khoản

### Thay đổi

- Đồng bộ cổng chạy backend về `http://localhost:5080` trong:
  `backend/src/CulinaryBlog.API/Properties/launchSettings.json`.
- Khởi động backend và kiểm tra kết nối PostgreSQL.
- Bổ sung thông báo riêng khi trang đăng ký không kết nối được API.
- Thêm rewrite `/api/:path*` trong `frontend/next.config.ts`, chuyển yêu cầu tới backend.
  - Địa chỉ backend mặc định cho rewrite: `http://127.0.0.1:5080`.
  - Có thể cấu hình bằng biến server `API_BASE_URL`.
- Đổi địa chỉ API mặc định của Axios thành đường dẫn cùng origin.
  Điều này giúp frontend gọi API khi người dùng mở website qua địa chỉ IP thay vì `localhost`.
- Các biến `NEXT_PUBLIC_API_BASE_URL` và `NEXT_PUBLIC_API_URL` vẫn được ưu tiên nếu cấu hình; khi dùng chúng cần bảo đảm địa chỉ và CORS phù hợp.

### Kiểm tra đã thực hiện

- Đăng ký qua backend: HTTP 201, nhận session với vai trò Author.
- Đăng nhập: HTTP 200.
- Lấy thông tin người dùng qua `/api/v1/auth/me`: HTTP 200.
- Đăng ký email trùng: HTTP 409.
- CORS preflight từ `http://localhost:3000`: HTTP 204.
- Đăng ký qua frontend tại `http://172.31.64.1:3000/api/v1/auth/register`: HTTP 201.
- Lấy thông tin tài khoản qua frontend: HTTP 200.
- Trang đăng ký: HTTP 200.
- Kiểm tra TypeScript đạt.

### Tài khoản thử nghiệm

- Email đã cung cấp cho người dùng: `registration-check-20261020162834@example.com`.
- Vai trò: **Author**, không phải Admin.
- Đã kiểm tra lại đăng nhập thành công sau khi hoàn thiện phân quyền frontend.
- Có tạo thêm tài khoản kiểm tra proxy với tiền tố `proxy-check-`.
- Không ghi mật khẩu hoặc token vào tài liệu trong repository.

## 3. Rà soát yêu cầu trong chucnang.md

File yêu cầu 5 chức năng danh mục và 3 tác vụ nền.

| Mã | Kết quả rà soát |
| --- | --- |
| FR-CAT-001 | Backend có danh sách danh mục, đếm Published, sắp xếp theo tên và sliding cache 60 phút. Trang danh mục còn dùng mock. Truy vấn đếm cần loại công thức đã xóa mềm. |
| FR-CAT-002 | Backend có chi tiết theo slug và phân trang. Frontend còn dùng mock, chưa phân trang thật. Gọi API thiếu page/pageSize đã trả 500 trong lần kiểm tra. |
| FR-CAT-003 | Backend có tạo danh mục, quyền Admin, kiểm tra tên/HTML, slug duy nhất và xóa cache. Chưa có form tạo trên frontend. |
| FR-CAT-004 | Backend có cập nhật, giữ slug và xóa cache. Chưa có form sửa trên frontend. |
| FR-CAT-005 | Backend có chặn xóa danh mục còn công thức và xóa mềm. Chưa có giao diện xóa; cách xóa mềm cần đối chiếu lại mô tả trong chucnang.md. |
| FR-JOB-001 | Có enqueue email HTML chào mừng sau đăng ký, tên người dùng, link ứng dụng, tùy chọn link kích hoạt và retry 1/5/30 phút. |
| FR-JOB-002 | Có tạo ảnh 300×300 và 800×600, cập nhật URL, retry 3 lần. Hiện dùng lưu trữ local, chưa đáp ứng lưu lên MinIO. |
| FR-JOB-003 | File SitemapGenerationJob.cs trống; chưa có tác vụ tạo sitemap theo lịch. |

Các điểm khác đã phát hiện:

- Trang chi tiết danh mục dùng danh mục đầu tiên khi slug không hợp lệ, chưa báo không tìm thấy đúng cách.
- Kiểu trả về frontend của `categoryApi.getBySlug` chưa khớp `{ category, recipes }` từ backend.
- Truy vấn chi tiết danh mục cho Author thấy cả Archived của mình, trong khi tài liệu chỉ yêu cầu thêm Draft.
- Hangfire Dashboard có kiểm tra Admin nhưng đang tắt trong cấu hình Development.
- Đã chạy nhóm test CategoryModuleTests, WelcomeEmailJobTests, ImageVariantProcessorTests và RegistrationAndImageUploadTests: **15 đạt, 0 lỗi**.
- Những test này không bao phủ toàn bộ luồng giao diện hoặc mọi yêu cầu trong tài liệu.

## 4. Hoàn thiện phân quyền frontend

Phạm vi thực hiện là phân quyền và phiên đăng nhập trên frontend, không bao gồm xây dựng toàn bộ CRUD danh mục.

### Hành vi đã triển khai

- Xác minh session qua `GET /api/v1/auth/me` trước khi hiển thị nội dung dashboard.
- Lấy role từ phản hồi backend, không tin role tự lưu trong localStorage.
- Guest truy cập dashboard được chuyển đến trang đăng nhập, giữ đường dẫn để quay lại sau đăng nhập.
- Author không thấy menu quản lý danh mục; nhập URL trực tiếp cũng không hiển thị nội dung trang đó.
- Admin được hiển thị menu và truy cập `/dashboard/categories`.
- Navbar và lời chào dashboard dùng tên tài khoản thật.
- Axios tự gửi Bearer token cho các API qua client chung.
- Nút đăng xuất ở Navbar và Sidebar xóa session local và gọi API logout.
- Xóa cache truy vấn khi session thay đổi.
- Xử lý token bị từ chối với HTTP 401 và thời điểm hết hạn đã lưu.
- Đồng bộ khi session thay đổi ở tab khác; xác minh lại khi cửa sổ được focus.
- Backend không truy cập được: không mở nội dung được bảo vệ, hiển thị lỗi và nút thử lại.
- `/login`, `/register` dùng lại các trang xác thực hoạt động.
- Các URL `/recipes/new`, `/recipes/{id}/edit` chuyển về route dashboard được bảo vệ.

### Các file chính

- `frontend/src/components/auth/AuthProvider.tsx`
- `frontend/src/components/auth/RequireAuth.tsx`
- `frontend/src/lib/auth/session.ts`
- `frontend/src/providers/Providers.tsx`
- `frontend/src/lib/api/auth.ts`
- `frontend/src/lib/api/axios.ts`
- `frontend/src/app/(dashboard)/layout.tsx`
- `frontend/src/app/(dashboard)/dashboard/page.tsx`
- `frontend/src/app/auth/login/page.tsx`
- `frontend/src/components/layout/Navbar.tsx`
- `frontend/src/components/layout/Sidebar.tsx`
- `frontend/middleware.ts`

### Cách bảo vệ hiện tại

- Session vẫn dùng localStorage theo cơ chế sẵn có.
- Route guard phía trình duyệt bảo vệ việc hiển thị nội dung; middleware không xác minh JWT từ localStorage.
- Backend tiếp tục chịu trách nhiệm xác thực JWT và chặn API theo quyền.
- Chưa triển khai phiên cookie HttpOnly hoặc luồng tự động refresh token.

### Kiểm thử

- TypeScript đạt.
- Production build cuối cùng thành công.
- Thêm `frontend/scripts/check-auth.cjs` để kiểm thử bằng Playwright và Edge headless.
- 5 tình huống trình duyệt với **API xác thực giả lập** đều đạt:
  1. Guest được chuyển sang đăng nhập.
  2. Author bị chặn trang danh mục, không thấy menu Admin dù localStorage ghi role Admin.
  3. Admin truy cập được và đăng xuất được.
  4. Token bị backend trả 401 được xóa và chuyển sang đăng nhập.
  5. Backend mất kết nối hiển thị nút thử lại, không hiển thị trang được bảo vệ.
- Đây không phải kiểm thử đăng nhập Admin thật với database.
- Playwright được cài cục bộ bằng `npm install --no-save --package-lock=false playwright`, chưa thêm vào devDependencies.
- Để chạy lại: cài Playwright, có Microsoft Edge và chạy `node scripts/check-auth.cjs` trong frontend.
  Biến `TEST_BASE_URL` cho phép chọn server kiểm thử; mặc định `http://localhost:3000`.

### Điều chỉnh build

- Tách thư mục sinh mã để tránh build và dev ghi đè lẫn nhau:
  - `next dev`: `.next`.
  - Production build/start: `.next-production`.
- Thêm `.next-production/` vào `.gitignore`.
- Next.js cập nhật cấu hình TypeScript để nhận types ở thư mục build mới.
- Build vẫn báo chưa có cấu hình ESLint; không coi đây là đã kiểm tra lint đầy đủ.

## 5. Công việc còn lại

### FR-CAT-001 — kế hoạch đã trao đổi, chưa thực hiện trọn vẹn

1. Loại công thức xóa mềm khỏi `recipeCount`.
2. Rà soát invalidate cache khi trạng thái hoặc danh mục của công thức thay đổi.
3. Chuẩn hóa kiểu DTO frontend, bỏ fallback mock khi API lỗi.
4. Nối trang `/categories` với `useCategories` và dữ liệu thật.
5. Bổ sung trạng thái tải, rỗng, lỗi và thử lại.
6. Kiểm thử API, cache và giao diện với database thật.

### Các chức năng khác

- Xây dựng giao diện Admin thêm/sửa/xóa danh mục; trang hiện tại vẫn chỉ có tiêu đề.
- Nối trang chi tiết danh mục với API và sửa các điểm đã rà soát.
- Tích hợp MinIO cho ảnh.
- Triển khai sitemap job.
- Các trang dashboard khác vẫn còn nội dung mẫu hoặc khung trang; phân quyền hoàn thành không đồng nghĩa các nghiệp vụ này đã hoàn thành.

## 6. Ghi chú khi tiếp tục

- Repository có nhiều thay đổi chưa commit từ trước phiên này. Không coi toàn bộ diff hiện tại là thay đổi mới của phiên này.
- Cần kiểm tra lại backend, Docker và frontend có đang chạy ở thời điểm tiếp tục; kết quả chạy dịch vụ trong tài liệu chỉ phản ánh thời điểm kiểm thử.
- Các lượt build backend còn báo cảnh báo package và giấy phép ImageSharp; chưa xử lý trong phạm vi công việc này.
