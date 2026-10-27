# Báo cáo hoàn thành các chức năng còn thiếu

Ngày thực hiện: 21/10/2026 (Asia/Saigon).

## Phạm vi và kết quả

Đã trực tiếp sửa source code trong workspace. Giữ các luồng đăng ký, đăng nhập, đăng xuất, danh mục, chi tiết và tìm kiếm toàn văn hiện có. Không commit/push/merge, không sửa lịch sử Git, không thay database credentials, không reset database hoặc Docker volumes.

| Hạng mục | Kết luận | Bằng chứng |
|---|---|---|
| FR-SRCH-002 | PASS | PostgreSQL thật: Category + Difficulty + MaxCookTime theo AND, kết hợp full-text search; validation trước truy vấn |
| FR-SRCH-003 | PASS | Sáu kiểu sort, kết hợp filter/search, phân trang có thứ tự phụ ổn định |
| Change Password | PASS | Identity PasswordHasher; kiểm tra mật khẩu cũ, xác nhận và độ mạnh; thu hồi refresh token và từ chối JWT cũ |
| Forgot Password | PASS | Trang forgot/reset; SMTP Mailpit; token bảo vệ bằng Data Protection, hết hạn 30 phút, vô hiệu sau sử dụng; response chung, rate limiting |
| Update Profile | PASS | Lưu họ tên/bio/avatar vào PostgreSQL; upload ảnh; đồng bộ Auth Store và AuthProvider; không cho sửa Role/email |
| Homepage | PASS | Latest Recipes và Category từ API, chọn danh mục, responsive, loading/error/empty, header/search/user menu/footer |
| Integration Tests | PASS | 11 tests trên PostgreSQL thật, trong đó 3 tests gọi HTTP thật qua Kestrel |
| Dự án sẵn sàng nộp bài | **CÓ, trong phạm vi các chức năng được yêu cầu** | Các giới hạn môi trường và cảnh báo dependency được ghi riêng bên dưới |

## Giai đoạn 1 — Advanced Filtering và Result Sorting

- Giữ query parameters hiện có: `categoryId`, `difficulty`, `maxCookTime`, `sort`, `page`, `pageSize`.
- Bổ sung ba bộ lọc cho `/api/v1/recipes/search`, thực thi trong SQL trước count/phân trang.
- Category/Difficulty/CookTime kết hợp theo AND và cùng điều kiện full-text search tiếng Việt hiện có.
- Từ chối GUID sai/rỗng, enum không tồn tại, thời gian âm, sort không hỗ trợ và pagination bị tràn số.
- Hỗ trợ `createdAt`, `-createdAt`, `title`, `-title`, `cookTime`, `-cookTime` trên danh sách và tìm kiếm. Các sort tìm kiếm khác đang có được giữ lại.
- Chuẩn hóa sort không phân biệt hoa/thường; bổ sung `title DESC` cho danh sách và lựa chọn trên frontend.
- Danh sách dùng `Id` làm thứ tự phụ. Search giữ `CreatedAt DESC`, rồi `Id` làm thứ tự phụ. Các trang không lặp công thức khi giá trị sort bằng nhau.
- Trang search có đủ Category, Difficulty, CookTime và Sort. URL lưu các lựa chọn, hỗ trợ reload/back/forward.
- Kết quả kiểm tra giai đoạn 1: **5/5 tests PostgreSQL PASS**.

## Giai đoạn 2 — Đổi và đặt lại mật khẩu

- Tái sử dụng `IPasswordHasher<ApplicationUser>` hiện có; không lưu/log mật khẩu thô.
- Đổi mật khẩu yêu cầu JWT hợp lệ và mật khẩu hiện tại đúng.
- Mật khẩu mới 8–128 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt; xác nhận phải khớp.
- Reset token được ASP.NET Core Data Protection bảo vệ, gắn với user và phiên bản password hash, hết hạn sau 30 phút.
- Sau đổi/reset, hash mới làm token reset cũ không còn hợp lệ. `PasswordHash` là concurrency token để hai request không thể đồng thời ghi đè cùng phiên bản mật khẩu.
- Thu hồi refresh tokens. JWT mới chứa phiên bản mật khẩu; validation đối chiếu với database nên JWT cũ bị từ chối ngay sau đổi/reset.
- **Phiên đăng nhập đã cấp trước thay đổi này cần đăng nhập lại**, vì chưa có claim phiên bản mật khẩu.
- Forgot trả cùng HTTP status/message cho email tồn tại và không tồn tại; không trả token/link cho frontend. Lỗi SMTP không tiết lộ tài khoản qua nội dung response.
- Rate limit reset: 5 request/15 phút/IP, dùng chung cho forgot/reset. Login/register vẫn 10 request/phút/IP; các thao tác hồ sơ dùng policy API để tránh giới hạn quá thấp khi điều hướng.
- Trang `/forgot-password`, `/reset-password`, `/change-password`; login có liên kết quên mật khẩu. Có loading/error/success và xác nhận mật khẩu.
- SMTP dùng cấu hình `Smtp:*` hiện có; URL frontend lấy từ `PasswordReset:ApplicationUrl`, fallback `WelcomeEmail:ApplicationUrl`. Không thêm SMTP secret hardcode.
- Kết quả sau giai đoạn 2: **8/8 tests PostgreSQL PASS**, bao gồm kiểm thử profile chuẩn bị cho giai đoạn 3.

## Giai đoạn 3 — Profile và Homepage

- Cả `/profile` và `/dashboard/profile` dùng form/API thật; bỏ lưu profile bằng preview store.
- Chỉ cập nhật `DisplayName`, `Bio`, `AvatarUrl`. Email và Role chỉ hiển thị, không phải trường cập nhật.
- Giới hạn tên 100 ký tự, bio 1000 ký tự, avatar URL 500 ký tự; từ chối scheme nguy hiểm.
- Upload avatar tái sử dụng file storage và validator hiện có: giới hạn kích thước, extension/MIME/signature, tên file ngẫu nhiên, thư mục theo user đang đăng nhập, policy Upload.
- Hỗ trợ Local hoặc MinIO theo cấu hình storage hiện có. Lần chạy này kiểm chứng Local storage.
- Frontend proxy `/uploads/*` đến backend để ảnh Local hiển thị được.
- AuthProvider và Zustand được đồng bộ từ response server; giữ thông báo thành công sau khi cập nhật. Kiểm tra lại dữ liệu sau reload.
- Homepage thay toàn bộ Featured hardcode bằng API list với `sort=-createdAt` và phân trang.
- Sidebar lấy danh mục API thật, chọn danh mục và reset trang. Responsive kiểm tra ở chiều rộng 390px.
- Recipe cards có title, image/placeholder, thời gian nấu, độ khó, category và link chi tiết.
- Header giữ navigation/user menu, thêm logo và form search; footer thêm copyright. Sửa màu chữ `.muted` để metadata/footer đọc được.
- TypeScript, ESLint và production build đều PASS.

## API mới

Các route dưới đây cũng được ánh xạ tương ứng dưới prefix tương thích `/api/auth`.

| Method | Route | Xác thực | Payload chính |
|---|---|---|---|
| POST | `/api/v1/auth/change-password` | Bắt buộc | `currentPassword`, `newPassword`, `confirmPassword` |
| POST | `/api/v1/auth/forgot-password` | Không | `email` |
| POST | `/api/v1/auth/reset-password` | Không | `email`, `token`, `newPassword`, `confirmPassword` |
| PATCH | `/api/v1/auth/me` | Bắt buộc | `displayName`, `bio`, `avatarUrl` |
| POST | `/api/v1/auth/me/avatar` | Bắt buộc | multipart field `file` |

Profile/upload trả `UserDto`. Các thao tác mật khẩu trả message; validation nghiệp vụ trả 422, thiếu/sai phiên trả 401, vượt rate limit trả 429.

Ví dụ query được kiểm thử:

```text
GET /api/v1/recipes?categoryId=<guid>&difficulty=Easy&maxCookTime=30&sort=-title
GET /api/v1/recipes/search?q=pho&categoryId=<guid>&difficulty=Easy&maxCookTime=30&sort=-cookTime
```

## Giai đoạn 4 — Kết quả kiểm thử cuối

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build backend/CulinaryBlog.slnx --no-restore` | PASS, 0 errors |
| `dotnet test backend/CulinaryBlog.slnx --no-build --no-restore` với PostgreSQL được cấu hình | **94 passed / 0 failed / 0 skipped** |
| Application.Tests | **69 passed** |
| API.Tests | **25 passed** |
| PostgreSQL integration trong Application.Tests | **8 passed** |
| PostgreSQL + HTTP Kestrel trong API.Tests | **3 passed** |
| `npm.cmd --prefix frontend run typecheck` | PASS |
| `npm.cmd --prefix frontend run lint` | PASS, 0 errors / 14 warnings |
| `npm.cmd --prefix frontend run build` | PASS, production build |
| Playwright Edge, frontend → API → PostgreSQL → SMTP Mailpit | **1 passed / 0 failed / 0 skipped**, 10,1 giây |

Integration tests dùng các database `culinary_fts_test_<guid>` và `culinary_http_test_<guid>` riêng biệt. Fixture tạo database, chạy migrations, test rồi chỉ xóa chính database do fixture tạo. Không migrate/seed/reset/xóa dữ liệu trong database được cung cấp qua connection string.

Kiểm thử HTTP bao gồm đăng ký/đăng nhập, chặn update không đăng nhập, bỏ qua Role giả mạo, profile và avatar upload, đổi mật khẩu, từ chối JWT/mật khẩu cũ, response forgot chung, reset một lần, rate limit và validation/sort trên hai endpoint danh sách/search.

Playwright thực thi với browser Edge headless, không intercept/mock response API. Dữ liệu runtime nằm trong database riêng `culinary_missing_runtime_65693c04ef024cfe93e3ab24ac8cef52`, được seeder hiện có tạo dữ liệu thử. Đây là dữ liệu thử được lưu và truy vấn thật qua PostgreSQL, không phải dữ liệu production và không phải mock HTTP.

## Docker và môi trường thực tế

- Docker Engine hoạt động. Sandbox ban đầu không truy cập được Docker pipe; đã kiểm tra bằng quyền thực thi được cấp.
- PostgreSQL đang chạy trong `culinary-blog-api-test-postgres`, host `127.0.0.1:5433`; Redis trong `culinary-blog-api-test-redis`, host port 6380.
- Container PostgreSQL thực tế chạy PostgreSQL 17; user/credentials khác cấu hình development/Compose trong repo. Tests đọc cấu hình container vào environment của tiến trình, không sửa credentials hoặc file cấu hình ứng dụng.
- Tái sử dụng hai container hiện có; không tạo PostgreSQL/Redis trùng.
- Khởi động đúng service `mailpit` từ Compose hiện có: SMTP 1025, giao diện/API 8025, container `culinary-blog-mailpit`.
- Không có container MinIO đang chạy trong lần kiểm tra này. Upload avatar được kiểm chứng với Local storage; không tuyên bố đã kiểm thử MinIO.
- Không xóa volumes hoặc dữ liệu hiện có. Database runtime riêng được giữ lại để đối chiếu.

## Chạy lại kiểm thử

Thiết lập `CULINARYBLOG_TEST_POSTGRES` bằng connection string thực tế từ môi trường của bạn, rồi chạy:

```powershell
dotnet build backend/CulinaryBlog.slnx
dotnet test backend/CulinaryBlog.slnx --no-build --verbosity normal
npm.cmd --prefix frontend run typecheck
npm.cmd --prefix frontend run lint
npm.cmd --prefix frontend run build
```

Nếu không có `CULINARYBLOG_TEST_POSTGRES`, 11 integration tests sẽ được đánh dấu SKIP rõ ràng, không giả PASS.

Để chạy browser test, cần API và frontend đang chạy, database thử đã migrate/seed, SMTP Mailpit 1025/API 8025. Frontend mặc định proxy API đến 5080; reset link phải trỏ đến frontend đang dùng:

```powershell
$env:CULINARYBLOG_E2E = '1'
npm.cmd --prefix frontend run test:e2e
```

Có thể đặt `E2E_BASE_URL`, `E2E_MAILPIT_URL` và `PasswordReset__ApplicationUrl` khi dùng địa chỉ khác. Browser test tạo tài khoản thử có email riêng và mật khẩu sinh ngẫu nhiên; hãy chạy trên database thử.

## Các file đã tạo/chỉnh sửa

### Backend

- `backend/src/CulinaryBlog.API/Endpoints/AuthEndpoints.cs`
- `backend/src/CulinaryBlog.API/Endpoints/RecipeEndpoints.cs`
- `backend/src/CulinaryBlog.API/Extensions/AuthenticationExtensions.cs`
- `backend/src/CulinaryBlog.API/Extensions/ServiceCollectionExtensions.cs`
- `backend/src/CulinaryBlog.Application/Common/Interfaces/IAccountService.cs` — mới.
- `backend/src/CulinaryBlog.Application/Common/Interfaces/IPasswordResetEmailSender.cs` — mới.
- `backend/src/CulinaryBlog.Application/Features/Recipes/Queries/SearchRecipes/SearchRecipesQuery.cs`
- `backend/src/CulinaryBlog.Application/Features/Recipes/Queries/SearchRecipes/SearchRecipesQueryHandler.cs`
- `backend/src/CulinaryBlog.Domain/Interfaces/IRecipeRepository.cs`
- `backend/src/CulinaryBlog.Infrastructure/DependencyInjection.cs`
- `backend/src/CulinaryBlog.Infrastructure/Email/PasswordResetEmailSender.cs` — mới.
- `backend/src/CulinaryBlog.Infrastructure/Identity/AccountService.cs` — mới.
- `backend/src/CulinaryBlog.Infrastructure/Identity/IdentityService.cs`
- `backend/src/CulinaryBlog.Infrastructure/Persistence/Configurations/ApplicationUserConfiguration.cs`
- `backend/src/CulinaryBlog.Infrastructure/Persistence/Repositories/RecipeRepository.cs`
- `backend/tests/CulinaryBlog.API.Tests/MissingFeaturesHttpTests.cs` — mới.
- `backend/tests/CulinaryBlog.Application.Tests/RecipeFullTextSearchTests.cs`
- `backend/tests/CulinaryBlog.Application.Tests/RecipeSearchValidationTests.cs`
- `backend/tests/CulinaryBlog.Application.Tests/RegistrationAndImageUploadTests.cs` — cập nhật fake repository theo optional filters.

### Frontend và báo cáo

- `frontend/package.json`, `frontend/package-lock.json` — thêm Playwright và script `test:e2e`.
- `frontend/playwright.config.ts` — mới.
- `frontend/tests/e2e/missing-features.spec.ts` — mới.
- `frontend/next.config.ts`
- `frontend/src/app/page.tsx`, `frontend/src/app/globals.css`
- `frontend/src/app/(auth)/forgot-password/page.tsx` — mới.
- `frontend/src/app/(auth)/reset-password/page.tsx` — mới.
- `frontend/src/app/(dashboard)/change-password/page.tsx` — mới.
- `frontend/src/app/(dashboard)/profile/page.tsx`
- `frontend/src/app/(dashboard)/dashboard/profile/page.tsx`
- `frontend/src/app/(public)/recipes/search/page.tsx`
- `frontend/src/components/auth/AuthProvider.tsx`
- `frontend/src/components/forms/LoginForm.tsx`
- `frontend/src/components/forms/PasswordForm.tsx` — mới.
- `frontend/src/components/forms/ProfileForm.tsx` — mới.
- `frontend/src/components/layout/header.tsx`, `frontend/src/components/layout/footer.tsx`
- `frontend/src/components/recipes/RecipeExplorer.tsx`
- `frontend/src/lib/api/search.ts`
- `frontend/src/types/api.ts`, `frontend/src/types/custom-api.ts`
- `.gitignore` — bỏ qua browser test output.
- `FINAL_MISSING_FEATURES_REPORT.md` — báo cáo này.

## Giới hạn và những phần chưa đạt

- Không còn chức năng thiếu trong phạm vi yêu cầu sau khi các kiểm tra cuối PASS.
- Production SMTP chưa được kiểm chứng với nhà cung cấp email bên ngoài; development đã kiểm chứng bằng Mailpit. Khi triển khai cần cung cấp SMTP và URL frontend thực tế bằng configuration/environment, đồng thời giữ Data Protection key ring bền vững để reset link không mất hiệu lực khi đổi instance.
- Build backend có cảnh báo dependency `System.Security.Cryptography.Pkcs` 9.0.11 và giấy phép ImageSharp cho Release; frontend có các cảnh báo ESLint hiện có. Các cảnh báo này không làm Debug backend build hoặc production frontend build thất bại; không tự nâng dependency ngoài phạm vi nhiệm vụ.
- Không khẳng định production deployment, kiểm thử MinIO hoặc tính sẵn sàng của các chức năng ngoài yêu cầu lần này.

## Xác nhận browser test cuối

**PASS — 1/1 test, 0 failed, 0 skipped.** Lần chạy cuối hoàn tất trong 10,1 giây, sau khi bổ sung avatar và sửa trạng thái thành công của profile.

Test xác nhận:

1. Đăng ký tài khoản thử bằng API thật; phiên hoạt động trong trình duyệt.
2. Homepage hiển thị công thức đầu tiên trùng kết quả API `sort=-createdAt`.
3. Chọn category gọi API lọc; sidebar hiển thị ở màn hình 390px và trang không tràn ngang.
4. Search + Category + Difficulty + CookTime + `title DESC` trả HTTP 200 và kết quả không rỗng, đáp ứng toàn bộ bộ lọc.
5. Upload avatar trả 200; ảnh được backend phục vụ qua `/uploads/avatars/...` và hiển thị trong trình duyệt.
6. Lưu họ tên/bio trả 200, hiển thị thông báo thành công và giữ dữ liệu sau reload.
7. Đổi mật khẩu qua form; chuyển về login, JWT cũ trả 401, mật khẩu mới đăng nhập thành công.
8. Forgot qua form; nhận email SMTP thật trong Mailpit, mở link từ email, reset qua form và đăng nhập bằng mật khẩu reset thành công.

Ảnh giao diện: [Homepage sau kiểm thử](docs/verification/homepage-missing-features.png).

Logs kiểm tra có trong workspace: `backend-build.log`, `backend-tests.log`, `frontend-build.log`, `frontend-typecheck.log`, `frontend-lint.log`, `frontend-e2e.log`. Đây là output kiểm thử cục bộ, được Git bỏ qua theo cấu hình sẵn có.

API test đang phục vụ ở `http://127.0.0.1:5080`, frontend ở `http://localhost:3000`, Mailpit ở `http://localhost:8025`; các tiến trình dùng database runtime riêng nêu trên. Không thay cấu hình kết nối development trong source để trỏ vào database thử.
