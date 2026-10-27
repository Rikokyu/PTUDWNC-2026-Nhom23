# Tiếp tục Culinary Blog — 27/10/2026

## Trạng thái tiếp nhận

Không có nội dung các lượt làm việc cũ trong ngữ cảnh chat hiện tại. Đã đối chiếu source, toàn bộ diff Git và các file mới với `FINAL_MISSING_FEATURES_REPORT.md` trong workspace. Khi tiếp nhận có 32 file tracked đã chỉnh sửa cùng các file mới của Authentication, Profile, password forms, HTTP tests và Playwright. Không reset, xóa code, đổi credentials, sửa Git history, cài lại thư viện, commit hoặc push.

Các chức năng đã được triển khai trước lượt tiếp tục này:

- FR-SRCH-002: Category, Difficulty, MaxCookTime kết hợp AND với tìm kiếm PostgreSQL full-text; validation trước truy vấn.
- FR-SRCH-003: sort ngày tạo, tên, thời gian nấu, thứ tự phụ ổn định; filter/sort trước phân trang; đồng bộ URL tìm kiếm.
- Authentication: đăng ký, đăng nhập, đăng xuất, đổi mật khẩu, forgot/reset; thu hồi refresh token và vô hiệu JWT cũ sau đổi/reset.
- Profile: lưu tên/bio/avatar qua API, upload ảnh, đồng bộ AuthProvider/store; email/role không được cập nhật từ payload.
- Homepage: công thức mới nhất và danh mục từ API, phân trang, responsive và các trạng thái loading/error/empty.
- Categories: bản sửa page/pageSize và 14 trường hợp HTTP test đã có sẵn trong source, chưa cần viết lại.

## Công việc hoàn tất trong lượt này

1. Kiểm tra Git, các diff/file mới và đường đi API → handler → repository/frontend của những chức năng trên.
2. Hoàn tất Backend Build, Frontend production Build, TypeScript và ESLint bằng dependencies đã có.
3. Khởi động Docker Desktop từ vị trí cài đặt thực tế, tái sử dụng PostgreSQL/Redis/Mailpit hiện có. Không chạy Compose để tạo PostgreSQL trùng cổng 5433; không thay đổi docker-compose.yml.
4. Đọc cấu hình container vào môi trường tiến trình để kết nối PostgreSQL thật; không lưu credentials vào source/báo cáo.
5. Bổ sung tùy chọn `CULINARYBLOG_KEEP_TEST_DATABASES=1` cho cả hai fixture PostgreSQL. Đã bật tùy chọn khi chạy tests; giữ lại toàn bộ database test mới, không xóa database/volumes.
6. Chạy toàn bộ backend tests với PostgreSQL thật, không skip.
7. Tái sử dụng database runtime test `culinary_missing_runtime_65693c04ef024cfe93e3ab24ac8cef52` từ lần trước và chạy Playwright với API thật, production frontend và SMTP Mailpit. Không mock HTTP.

Không phát hiện lỗi chức năng cần sửa thêm sau kiểm chứng. Giữ nguyên code hoàn thành trước đó.

## Categories page/pageSize

`GET /api/v1/categories/{slug}` mặc định `page=1`, `pageSize=12`; có thể bỏ riêng từng tham số. Giá trị 0, âm, chữ, rỗng, lặp page, pageSize > 50 hoặc offset vượt Int32 trả HTTP 400 ProblemDetails. 14 trường hợp HTTP đã PASS trên PostgreSQL thật. `GET /api/v1/categories` vẫn trả array danh mục, không ép phân trang vào API danh sách.

## Kết quả hiện tại

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build backend/CulinaryBlog.slnx --no-restore` | PASS, 0 errors |
| `npm.cmd --prefix frontend run build` | PASS, production build |
| `npm.cmd --prefix frontend run typecheck` | PASS |
| `npm.cmd --prefix frontend run lint` | PASS, 0 errors / 14 warnings |
| Backend tests với PostgreSQL thật | PASS: 109 passed / 0 failed / 0 skipped |
| Application.Tests | 69 passed |
| API.Tests | 40 passed, gồm 14 trường hợp Categories pagination |
| Playwright với `CULINARYBLOG_E2E=1` | PASS: 1 passed / 0 failed / 0 skipped, tổng 7,2 giây |
| `git diff --check` | PASS |

Lần test đầu không có connection string cho kết quả 83 passed / 13 skipped. Kết quả chính thức ở trên là lần chạy lại có PostgreSQL: toàn bộ 109 trường hợp đều thực thi; theory Categories được mở rộng thành 14 trường hợp khi đủ môi trường.

Playwright xác nhận Homepage khớp API mới nhất; danh mục hoạt động trên màn hình 390px, không tràn ngang; Search + Category + Difficulty + CookTime + sort tên giảm dần; upload avatar; lưu profile và reload; đổi mật khẩu và từ chối JWT cũ; forgot/reset qua email SMTP thật và đăng nhập bằng mật khẩu mới.

TRX: `docs/verification/test-results/postgres-resume_*.trx` (output test cục bộ bị Git ignore). Screenshot mới: `frontend/test-results/homepage.png` (Git ignore).

## File sửa trong lượt tiếp tục

- `backend/tests/CulinaryBlog.Application.Tests/RecipeFullTextSearchTests.cs`: tùy chọn giữ database test và chú thích.
- `backend/tests/CulinaryBlog.API.Tests/MissingFeaturesHttpTests.cs`: tùy chọn giữ database test và chú thích.
- `docs/verification/RESUME_REPORT_2026-10-27.md`: báo cáo mới.
- `docs/verification/resume-file-inventory.txt`: danh sách đầy đủ file thay đổi/mới đang có so với HEAD, bao gồm công việc trước đó.

## Giới hạn còn lại

Không còn phần dang dở được phát hiện trong phạm vi yêu cầu sau các kiểm tra trên. Chưa kiểm chứng MinIO hoặc SMTP production; browser test dùng Local storage và Mailpit. Backend Debug build có cảnh báo dependency System.Security.Cryptography.Pkcs 9.0.11 và ImageSharp license; chưa xác nhận Release build vì ImageSharp yêu cầu license hợp lệ. Frontend có 14 cảnh báo ESLint hiện có, không lỗi. Không tự nâng dependency ngoài phạm vi.

API test đang chạy ở http://127.0.0.1:5080, frontend production ở http://localhost:3000, Mailpit ở http://localhost:8025. API dùng database runtime test nêu trên thông qua environment của tiến trình; cấu hình development trong source giữ nguyên. Database test được giữ lại theo yêu cầu không xóa database.
