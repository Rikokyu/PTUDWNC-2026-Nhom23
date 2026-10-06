# Kết quả kiểm tra project

**Ngày kiểm tra:** 06-10-2026  
**Phạm vi:** SRS v1.0.1, toàn bộ API backend, FR-FILE-001, FR-OBS-002,
Swagger/OpenAPI, cấu hình và các lời gọi API frontend, build và test.

## 1. Tất cả API endpoints

SRS Chương 8 đối chiếu với route source và OpenAPI. API sử dụng Minimal API,
không dùng MVC Controller; cột Controller ghi endpoint mapper thực tế.

| STT | Method | Endpoint | Chức năng | Controller / mapper | PASS/FAIL | Ghi chú |
|---:|---|---|---|---|---|---|
| 1 | POST | `/api/v1/auth/register` | Đăng ký tài khoản | `AuthEndpoints` | PASS | 201; validation 400; trùng tài khoản 409 |
| 2 | POST | `/api/v1/auth/login` | Đăng nhập | `AuthEndpoints` | PASS | 200; credential sai 401; rate limit 429 |
| 3 | POST | `/api/v1/auth/google` | Đăng nhập Google | `AuthEndpoints` | PASS | Có implementation; môi trường kiểm tra chưa cấu hình Google nên trả 503 |
| 4 | POST | `/api/v1/auth/refresh` | Làm mới token | `AuthEndpoints` | PASS | Token sai 401 |
| 5 | POST | `/api/v1/auth/logout` | Đăng xuất / thu hồi refresh token | `AuthEndpoints` | PASS | Yêu cầu xác thực; trả 204 |
| 6 | GET | `/api/v1/auth/me` | Lấy hồ sơ hiện tại | `AuthEndpoints` | PASS | Yêu cầu xác thực |
| 7 | PATCH | `/api/v1/auth/me` | Cập nhật hồ sơ | `AuthEndpoints` | PASS | Validation và lỗi xác thực được mô tả |
| 8 | GET | `/api/v1/categories` | Lấy danh sách danh mục | `CategoryEndpoints` | PASS | Truy vấn repository |
| 9 | POST | `/api/v1/categories` | Tạo danh mục | `CategoryEndpoints` | PASS | Chỉ Admin; 201/400/401/403/409 |
| 10 | GET | `/api/v1/categories/{slug}` | Lấy danh mục và nội dung theo slug | `CategoryEndpoints` | PASS | 400/404 |
| 11 | PUT | `/api/v1/categories/{id}` | Cập nhật danh mục | `CategoryEndpoints` | PASS | Chỉ Admin; 200/400/401/403/404/409 |
| 12 | DELETE | `/api/v1/categories/{id}` | Xóa danh mục | `CategoryEndpoints` | PASS | Chỉ Admin; trả 204 khi thành công |
| 13 | GET | `/api/v1/recipes` | Lấy danh sách công thức | `RecipeEndpoints` | PASS | Phân trang, danh mục, độ khó, thời gian nấu |
| 14 | POST | `/api/v1/recipes` | Tạo công thức | `RecipeEndpoints` | PASS | Author/Admin; 201 và lỗi được mô tả |
| 15 | GET | `/api/v1/recipes/search` | Tìm kiếm công thức | `RecipeEndpoints` | PASS | Có phân trang và sắp xếp |
| 16 | GET | `/api/v1/recipes/{slug}` | Lấy chi tiết công thức | `RecipeEndpoints` | PASS | 403 cho nội dung không được phép xem; 404 nếu không tồn tại |
| 17 | PUT | `/api/v1/recipes/{id}` | Cập nhật công thức | `RecipeEndpoints` | PASS | Author/Admin; OpenAPI có 400/401/403/404/409 |
| 18 | DELETE | `/api/v1/recipes/{id}` | Xóa công thức | `RecipeEndpoints` | PASS | Author/Admin; trả 204 |
| 19 | PATCH | `/api/v1/recipes/{id}/publish` | Xuất bản công thức | `RecipeEndpoints` | PASS | Author/Admin |
| 20 | PATCH | `/api/v1/recipes/{id}/unpublish` | Gỡ xuất bản | `RecipeEndpoints` | PASS | Author/Admin |
| 21 | PATCH | `/api/v1/recipes/{id}/archive` | Lưu trữ công thức | `RecipeEndpoints` | PASS | Author/Admin |
| 22 | POST | `/api/v1/recipes/{id}/ingredients` | Thêm nguyên liệu | `RecipeEndpoints` | PASS | Author/Admin; lưu qua service/repository |
| 23 | PUT | `/api/v1/recipes/{id}/ingredients/{ingredientId}` | Cập nhật nguyên liệu | `RecipeEndpoints` | PASS | Author/Admin |
| 24 | DELETE | `/api/v1/recipes/{id}/ingredients/{ingredientId}` | Xóa nguyên liệu | `RecipeEndpoints` | PASS | Author/Admin; trả 204 |
| 25 | POST | `/api/v1/recipes/{id}/steps` | Thêm bước chế biến | `RecipeEndpoints` | PASS | Author/Admin |
| 26 | PUT | `/api/v1/recipes/{id}/steps/{stepId}` | Cập nhật bước chế biến | `RecipeEndpoints` | PASS | Author/Admin |
| 27 | DELETE | `/api/v1/recipes/{id}/steps/{stepId}` | Xóa bước chế biến | `RecipeEndpoints` | PASS | Author/Admin; trả 204 |
| 28 | POST | `/api/v1/recipes/{id}/images` | Upload ảnh công thức | `RecipeEndpoints` | PASS | Author/Admin; multipart; 201/400/401/403/404/409/503 |
| 29 | PATCH | `/api/v1/recipes/{id}/images/{imageId}` | Cập nhật metadata ảnh | `RecipeEndpoints` | PASS | Author/Admin |
| 30 | PATCH | `/api/v1/recipes/{id}/images/{imageId}/primary` | Chọn ảnh chính | `RecipeEndpoints` | PASS | Author/Admin |
| 31 | DELETE | `/api/v1/recipes/{id}/images/{imageId}` | Xóa ảnh | `RecipeEndpoints` | PASS | Author/Admin; MinIO lỗi trả 503 |
| 32 | GET | `/health` | Health tổng hợp | `HealthEndpoints` | PASS | OpenAPI hiển thị |
| 33 | GET | `/health/live` | Liveness check | `HealthEndpoints` | PASS | OpenAPI hiển thị |
| 34 | GET | `/health/ready` | Readiness check | `HealthEndpoints` | PASS | Kiểm tra PostgreSQL, Redis, MinIO |

**Tổng API theo SRS:** 34  
**Đã implement:** 34  
**Chưa implement:** 0  
**Hoàn thành API:** 34/34 = **100%**

Tất cả 34 operation xuất hiện trong OpenAPI. Đã rà soát route, handler,
business service/repository tương ứng, DTO và các TODO/stub liên quan. Không
phát hiện endpoint bắt buộc nào đang trả mock/hard-code. Các `return null`
trong JWT/current-user xử lý token hoặc claim không hợp lệ, không phải stub.

Runtime test tập trung vào các luồng an toàn: các route mutation công thức
được gọi với ID không tồn tại trả 404; đăng ký thiếu dữ liệu trả 400; đăng
nhập và refresh token sai trả 401; route Admin không token trả 401. Không cố
tình tạo/xóa dữ liệu production hoặc làm sập database để giả lập 500; vì vậy
không khẳng định đã chạy happy-path thành công riêng cho cả 34 operation.

## 2. FR-FILE-001 – Upload file lên MinIO

| STT | Yêu cầu | PASS/FAIL | Bằng chứng |
|---:|---|---|---|
| 1 | MinIO SDK và cấu hình dependency | PASS | SDK có trong Infrastructure; cấu hình lấy từ environment/configuration |
| 2 | Abstraction lưu trữ | PASS | `IFileStorageService` |
| 3 | Service lưu trữ MinIO | PASS | `MinioFileStorageService`; bucket được kiểm tra/tạo và object được upload/xóa |
| 4 | API upload `multipart/form-data` | PASS | `POST /api/v1/recipes/{id}/images`, field `file` |
| 5 | File thiếu/rỗng | PASS | Trả 400; đã kiểm thử trường hợp thiếu/rỗng |
| 6 | Giới hạn dung lượng | PASS | `FileUpload:MaxFileSizeBytes`, mặc định 5 MiB |
| 7 | Extension, MIME và magic bytes | PASS | JPEG/PNG/WebP/AVIF được đối chiếu; mismatch trả 400 |
| 8 | Không tin MIME client đơn độc | PASS | Validator kiểm tra đồng thời extension, MIME, chữ ký file và kích thước |
| 9 | Object key an toàn/path traversal | PASS | Tên file được lấy basename; key dùng GUID dưới `recipes/{id}/` |
| 10 | Upload object thực tế | PASS | JPEG smoke test trả 201; object đọc lại từ MinIO trả 200 |
| 11 | Không hard-code credential | PASS | Access/secret key lấy từ cấu hình môi trường; không ghi giá trị bí mật vào báo cáo |
| 12 | MinIO lỗi được xử lý | PASS | Lỗi storage được ánh xạ 503; health check MinIO hoạt động |
| 13 | Swagger cho phép chọn file | PASS | OpenAPI có multipart object với `file` bắt buộc, `string/binary`; Swagger UI hiện nút Choose File khi chọn Try it out |

## 3. FR-OBS-002 – Structured Logging

| Yêu cầu | PASS/FAIL | Bằng chứng |
|---|---|---|
| Serilog cài đặt và tích hợp ASP.NET Core | PASS | Đăng ký trong `Program.cs`, log JSON |
| Log có cấu trúc, enrichment | PASS | Structured properties và logging context |
| Correlation ID từ `X-Correlation-ID` hoặc tự sinh | PASS | `CorrelationIdMiddleware` và test middleware |
| Correlation ID trong context và response | PASS | Middleware đưa vào `LogContext`; response echo header |
| Method, path, status, elapsed time | PASS | Request logging có các trường này |
| Request thành công/thất bại và exception | PASS | HTTP request logging, exception middleware và log upload failure |
| Không serialize request nhạy cảm | PASS | `LoggingBehavior` không log nội dung DTO/password/token |
| Không log secret key hoặc nội dung file | PASS | Upload log chỉ ghi metadata; không đọc/ghi nội dung file vào log |
| Kiểm tra Correlation ID runtime | PASS | Gửi `audit-correlation-20261006`; response trả lại chính xác cùng ID |

## 4. Tích hợp upload và logging

| Kiểm tra | PASS/FAIL | Bằng chứng |
|---|---|---|
| Upload thành công có structured `FileUpload` event | PASS | Log có CorrelationId, FileName, FileSize, ContentType, ObjectKey, Status |
| Upload lỗi có event thất bại và loại/lý do lỗi | PASS | Log failure dùng structured fields; lỗi validation/storage không bị nuốt |
| Không log secret/nội dung file | PASS | Chỉ log metadata đã nêu; không log bytes hoặc credential |

## 5. Swagger/OpenAPI

- **34/34** route có operation trong OpenAPI; method/path được xác minh từ
  `/swagger/v1/swagger.json`.
- Request/response schema và các status phổ biến được khai báo.
- Đã bổ sung 400/401 cho một số operation còn thiếu metadata; hiện không còn
  operation yêu cầu xác thực nào thiếu response 401 trong OpenAPI.
- Upload schema thể hiện `multipart/form-data`, field `file` bắt buộc và
  `string/binary`; đã xác nhận nút chọn file trong Swagger UI.

## 6. Build và kiểm thử

| Hạng mục | Kết quả | Chi tiết |
|---|---|---|
| Restore dependencies | PASS | `npm ci` thành công; .NET dependencies đã restore trước đó |
| Backend solution build | PASS | `dotnet build backend/CulinaryBlog.slnx --no-restore`; 0 warning, 0 error |
| Backend tests | PASS | 15/15, 0 failed |
| Frontend type-check | PASS | `npm run typecheck` |
| Frontend lint | PASS* | `npm run lint` exit 0; cấu hình ESLint hiện rỗng nên chưa áp dụng rule hữu ích |
| Frontend production build | PASS | `npm run build`; routes Next.js build thành công |
| Runtime readiness | PASS | PostgreSQL, Redis, MinIO đều Healthy |
| Runtime upload + validation | PASS | Upload JPEG thành công; file thiếu/rỗng, quá cỡ và mismatch bị từ chối 400 |

### Ma trận điều kiện tối thiểu

| Kiểm tra | Kết quả |
|---|---|
| Build | PASS |
| Compile | PASS — 0 warning, 0 error |
| Swagger | PASS — 34 operation; upload hiện field chọn file |
| API endpoints | PASS — 34/34 |
| MinIO | PASS — health check Healthy |
| Upload | PASS — JPEG upload và đọc lại object |
| Magic Bytes | PASS |
| MIME Validation | PASS |
| Structured Logging | PASS |
| CorrelationId | PASS |

`npm ci` báo 9 advisory trong dependency tree (1 moderate, 8 high); báo cáo này
không tự động nâng cấp dependency vì cần rà tương thích riêng.

## 7. Kiểm tra và sửa frontend/API

- Sửa Axios để ưu tiên `NEXT_PUBLIC_API_BASE_URL` (đúng với `.env.example`),
  tương thích thêm `NEXT_PUBLIC_API_URL` và dùng cổng launch profile làm mặc
  định local.
- Đồng bộ TypeScript recipe DTO với response backend; thêm API helper riêng
  cho endpoint tìm kiếm.
- Sửa route collision Next.js bằng cách chuyển trang quản trị danh mục và công
  thức sang `/dashboard/categories` và `/dashboard/recipes`. Production build
  trước sửa bị fail do hai route group cùng chiếm `/categories` và `/recipes`;
  sau sửa build thành công.
- `useComments` vẫn tham chiếu `/api/v1/recipes/{recipeId}/comments`, nhưng
  endpoint Comments không thuộc phạm vi SRS v1.0.1 và hook hiện không được gọi.
  Không tính đây là API bắt buộc; nếu bật tính năng Comments sau này thì cần
  mở rộng SRS và triển khai endpoint tương ứng.
- Các trang danh sách/chi tiết hiện còn placeholder và chưa gọi các recipe
  hooks. Điều này không làm thiếu endpoint backend bắt buộc nhưng có nghĩa
  giao diện chưa phải luồng end-to-end hoàn chỉnh.
- MinIO đã được smoke test bằng container local Bitnami Legacy do giới hạn
  registry trong môi trường kiểm tra; cấu hình đó chỉ dùng local, không nên
  xem là lựa chọn image production.

## 8. Tỷ lệ hoàn thành

Tỷ lệ tính trên các yêu cầu bắt buộc được nêu trong đợt kiểm tra này:

- **FR-FILE-001: 100%**
- **FR-OBS-002: 100%**
- **ALL API ENDPOINTS: 100%** (34/34)
- **TỔNG THỂ: 100%** trong phạm vi 3 yêu cầu bắt buộc và build/test.

Các giới hạn về giao diện placeholder, Google provider chưa cấu hình, lint
config rỗng và advisory dependency được ghi riêng phía trên, không được che
giấu hoặc tính thành endpoint thiếu.

## 9. Kết luận

- [x] **HOÀN THÀNH 100%** — đối với FR-FILE-001, FR-OBS-002, toàn bộ API
  endpoints bắt buộc theo SRS và điều kiện build/test.
- [ ] HOÀN THÀNH MỘT PHẦN
- [ ] CHƯA HOÀN THÀNH

**ĐÃ ĐỦ YÊU CẦU TỐI THIỂU: CÓ.**
