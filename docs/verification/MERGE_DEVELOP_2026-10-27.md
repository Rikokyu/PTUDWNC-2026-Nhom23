# Gộp develop vào fr-obs — 27/10/2026

Nguồn: `origin/develop` tại `24da8699977ea9cc7a9bd16d50f367ec0551bf10`.
Đích: `fr-obs`, HEAD trước gộp `02e506a`.

## Trạng thái

Đã xử lý 17 file xung đột giữa các commit và áp dụng lại toàn bộ thay đổi chưa commit/file mới từ stash. Bản gộp được stage để Git ghi nhận đã giải quyết xung đột. **Chưa tạo merge commit, chưa commit phần việc cá nhân, chưa push.** Git vẫn ở trạng thái merge chờ commit theo yêu cầu giữ công việc chưa commit. HEAD chưa trở thành hậu duệ của develop cho đến khi merge được commit.

Hai stash dự phòng được giữ, không drop:

- `before-resolve-develop-2026-10-27`
- `local-work-before-pull-develop-2026-10-27`

Không dùng reset/clean, không đổi credentials, không rewrite history. Không thay `.env` thật, không dừng/reset Docker hay database. API test workspace đang chạy từ lần trước đã được dừng để giải phóng DLL cho build; không tự khởi động lại API/seeder trên database runtime.

## Các quyết định gộp

- Giữ Vietnamese PostgreSQL full-text search, FR-SRCH-002/003, pagination Categories, Homepage API, Profile/avatar và luồng mật khẩu đã được kiểm thử.
- Nhận workflow công thức, quản lý nội dung/nguyên liệu/đơn vị/bước nấu, cleanup ảnh, DTO và tests mới từ develop.
- Giữ bộ UI hiện có và package/lockfile tương thích Tailwind 4; chuyển import UI viết hoa từ develop sang thư mục legacy-ui đã có để tránh xung đột tên với bộ UI hiện tại.
- Loại route Search trùng; giữ một endpoint có validation/filter/sort đầy đủ.
- Hợp nhất Authentication: giữ endpoint đã kiểm thử, thêm PUT /auth/me tương thích client develop; các command contracts từ develop gọi AccountService đã có để không nhân đôi xử lý mật khẩu/profile.
- Sửa khai báo UpdateProfileAsync trùng trong interface/fake test; thống nhất thứ tự bio/avatar giữa command và service.
- Giữ cả DI cleanup ảnh mới và DI welcome email cũ. Bổ sung xác nhận mật khẩu trong auth API client từ develop cho khớp contract đã kiểm thử.
- Giữ appsettings không chứa secrets, `.env.example` placeholder, Compose environment references và các ignore rules bảo mật.
- Build cache `frontend/tsconfig.tsbuildinfo` không được đưa lại vào Git; file cache trên máy được giữ và ignore.

## Kiểm chứng bản gộp

| Kiểm tra | Kết quả |
|---|---|
| Backend Build --no-restore | PASS, 0 errors |
| Frontend production build | PASS |
| TypeScript sau chỉnh auth client | PASS |
| Application.Tests, PostgreSQL thật | 88 PASS, 0 FAIL, 0 SKIP |
| API.Tests, PostgreSQL/HTTP thật | 40 PASS, 0 FAIL, 0 SKIP |
| Tổng backend tests | **128 PASS, 0 FAIL, 0 SKIP** |
| Conflict markers / unmerged paths | Không còn |
| Git diff check (working tree và index) | PASS |

Lần test đầu phát hiện 3 HTTP test lỗi do thiếu đăng ký welcome email và route Search trùng sau auto-merge; đã sửa và chạy lại toàn bộ backend tests với kết quả trên. Không cài lại thư viện. Database test riêng được giữ bằng CULINARYBLOG_KEEP_TEST_DATABASES=1. TRX mới nằm trong `docs/verification/test-results/merge-final_*.trx`, bị ignore.

Playwright đã chạy lại ở bước kiểm tra trước merge commit: **1 PASS, 0 FAIL, 0 SKIP**, tổng 5,4 giây (test 3,7 giây). Dùng production frontend sau merge tại http://localhost:3001, API bản sau merge tại http://127.0.0.1:5080, database runtime test riêng đã có và SMTP Mailpit. Không mock HTTP. Xác nhận Homepage khớp API, danh mục responsive 390px không tràn ngang, Search/filter/sort, upload avatar, lưu Profile sau reload, đổi mật khẩu và từ chối JWT cũ, forgot/reset qua email Mailpit và đăng nhập bằng mật khẩu mới. Không phát hiện FAIL cần sửa source. Không chạy lại toàn bộ build/backend tests ở bước này. Cảnh báo dependency Pkcs, ImageSharp license và lint UI hiện có vẫn còn.

## Kiểm tra cuối trước merge commit

- MERGE_HEAD: `24da8699977ea9cc7a9bd16d50f367ec0551bf10`; không có unmerged files.
- Staged files: không có `.env` thật, User Secrets, logs, database, test outputs hoặc build cache. Appsettings staged không chứa secret literal; các trường nhạy cảm trong `.env.example` là placeholder. Các mật khẩu fixture/demo không phải credential hạ tầng mới.
- Docker: PostgreSQL test, Redis và Mailpit đang chạy; Mailpit báo healthy. MinIO cũng đang chạy nhưng E2E này dùng Local storage, không kết luận kiểm thử MinIO.
- Hai stash dự phòng được giữ nguyên ID: `27b1d82d033b8516c6c7f909cf4d3d896dc41920` và `7ea0ecefbb56cc1cfe5bfa844428140c2890166e`; không apply/drop.
- Git diff check cho cả index và working tree: PASS.
- Về kỹ thuật có thể tạo merge commit; chưa thực hiện commit/push. Git Security vẫn CHƯA XÁC MINH do credential lịch sử chưa có bằng chứng thu hồi; chưa sẵn sàng push GitHub.

Credential PostgreSQL lịch sử vẫn **CHƯA XÁC MINH thu hồi**; báo cáo này không thay đổi kết luận Git Security CHƯA ĐẠT và không cho phép push tự động.
