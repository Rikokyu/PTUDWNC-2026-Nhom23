# Nghiệm thu Tuần 6, Tuần 7 và Lab 8 — cập nhật kiểm chứng runtime 28/10/2026

## Cập nhật bảo vệ commit local — 03/11/2026

- Đã loại bỏ mật khẩu Admin cố định khỏi `frontend/tests/e2e/lab8-runtime.spec.ts`; test dùng `process.env.E2E_ADMIN_PASSWORD` và báo lỗi rõ ràng nếu thiếu biến.
- Playwright dùng `@next/env` sẵn có để đọc cấu hình local. `frontend/.env.example` chỉ có placeholder; giá trị test cũ được chuyển nguyên vẹn vào `frontend/.env.local` đã ignore, không đổi mật khẩu Admin hoặc đưa secret vào Git.
- Kiểm chứng mới: TypeScript PASS; Playwright **3 PASS, 0 FAIL, 0 SKIP**, gồm hai test Lab8 thật (Admin login/user listing, recipe CRUD/status/MinIO, browser create/edit) và một UI contract pagination. Bằng chứng local: `week678-localcommit-typecheck.log`, `week678-localcommit-playwright.log`.
- API dùng database/bucket test mới tách riêng. Không sửa credential, không reset/xóa database hoặc volumes, không merge/push/rebase; hai stash dự phòng giữ nguyên.
- Bản backup 42 file gốc vẫn có đủ SHA256 trước sửa. Các thay đổi lần này được chuẩn bị lưu bằng commit local `feat: complete week6 week7 and lab8` sau kiểm tra staged content.
- Loại khỏi commit: `.env` thật, `.env.local`, logs/build/test outputs, `week678-runtime-state.json`, `docs/verification/WEEK678_READINESS.json`, `docs/verification/WEEK678_RUNTIME_EVIDENCE.json`. Các báo cáo giữ tham chiếu đến bằng chứng local, không đưa dữ liệu runtime vào Git.
- Git Security lịch sử vẫn CHƯA ĐẠT; các kết luận dừng commit trong báo cáo cũ mô tả các lần kiểm tra trước. Credential trên `origin/develop` không được tích hợp trong phiên commit local này.

---

**Phần cập nhật này thay thế các kết luận môi trường chưa khả dụng trong bản báo cáo cũ được giữ bên dưới để đối chiếu.**

## Kết quả mới nhất

| Hạng mục | Kết quả thực tế | Bằng chứng |
|---|---|---|
| PostgreSQL integration | **PASS: 26/26, 0 FAIL, 0 SKIP** | `week678-postgres-tests.log`: 8 Application + 18 API; TRX `week678-postgres_*` |
| MinIO upload | **PASS** | Lab8 E2E upload thật, GET object so sánh từng byte, thay ảnh chính, ảnh tải được trong Recipe Detail |
| Redis | **PASS** | `docker exec culinary-blog-api-test-redis redis-cli PING` → PONG; API readiness xác nhận kết nối backend |
| PostgreSQL/Redis/MinIO readiness | **PASS: HTTP 200, tất cả Healthy** | `docs/verification/WEEK678_READINESS.json` |
| OTLP collector | **PASS, có dữ liệu thực** | `week678-collector.log`, `docs/verification/WEEK678_RUNTIME_EVIDENCE.json` |
| E2E thật: homepage/search/filter/sort/profile/password/SMTP reset | **1 PASS, 0 SKIP** | `week678-real-e2e.log` — test `missing-features.spec.ts` |
| E2E thật: Lab8 API + trình duyệt | **2 PASS, 0 FAIL, 0 SKIP** | `week678-lab8-e2e.log`; runner exit 0 trong lần chạy cuối |
| UI contract pagination (API interception) | **1 PASS** | `week678-real-e2e.log`; phân biệt với 3 E2E thật ở trên |
| Frontend production build sau sửa thiếu sót trang edit | **PASS** | `week678-runtime-frontend-build.log` |
| TypeScript sau thay đổi cuối | **PASS** | `week678-runtime-typecheck.log` |
| ESLint | **PASS: 0 errors, 18 warnings** | `week678-runtime-lint.log` |
| Backend build | **PASS từ lần trước; backend không đổi trong phiên runtime** | `week678-backend-build.log`; không build lại không cần thiết |
| Git Security lịch sử | **CHƯA ĐẠT** | Chưa có xác nhận thu hồi credential từng xuất hiện trong history |

117 tests PASS của lần trước được giữ nguyên bằng chứng, không tuyên bố đã chạy lại toàn bộ trong phiên này. 13 test attributes bị SKIP trước đó mở rộng thành 26 ca khi bật PostgreSQL (bao gồm theory cases); lần chạy mới đã chạy đủ 26 ca, không cộng số SKIP như PASS. Các nhóm E2E được chạy tách để kiểm tra đúng phần thay đổi và tôn trọng rate limit.

## 1. Docker và nguyên nhân kết nối

Docker Desktop/Engine hiện `running`, Docker context `desktop-linux`. Sandbox không truy cập được Docker pipe/config, nên kiểm tra Engine dùng lệnh được cấp quyền ngoài sandbox. Không thay cấu hình hệ thống, không cần khởi động lại Desktop.

| Container tái sử dụng | Mapping thực tế | Xác minh |
|---|---|---|
| `culinary-blog-api-test-postgres` | `127.0.0.1:5433 → 5432` | `pg_isready`: accepting connections, 26 integration tests PASS |
| `culinary-blog-api-test-redis` | `127.0.0.1:6380 → 6379` | PONG + backend Healthy |
| `culinary-blog-minio` | `9000/9001 → 9000/9001` | readiness + upload thật |
| `culinary-blog-mailpit` | `1025/8025` | SMTP password reset E2E PASS |
| `ptudwnc-2026-nhom23-otel-collector-1` | localhost `4317/4318` | Nhận OTLP traces/metrics thật |

PostgreSQL trong `docker-compose.yml` cũng muốn bind 5433. Vì container test đang sử dụng cổng này, **không chạy thêm service postgres đó**. Không tạo container trùng, không đổi port/credentials, không dùng PostgreSQL native ở 5432 hoặc container FTS ở 55433 thay thế connection hiện có.

Collector được tạo đúng một lần từ Compose bổ sung đã có bằng `docker compose -f docker-compose.yml -f docker-compose.observability.yml up -d --no-deps otel-collector`. PostgreSQL, Redis, MinIO và Mailpit đã chạy nên được tái sử dụng.

26 lỗi trước đây xảy ra ở bước mở connection `127.0.0.1:5433` khi Docker/service chưa khả dụng, không phải assertion nghiệp vụ. Lần đầu chạy lại trong sandbox đạt 25/26 và còn HTTP 500 ở password reset; chạy ngoài sandbox đạt 26/26 mà không sửa backend. Giới hạn quyền Data Protection/key store là một khả năng, nhưng fixture không lưu exception của lần 500 nên chưa đủ bằng chứng để chốt nguyên nhân của lỗi trung gian này. Không tuyên bố đã sửa một lỗi backend tương ứng.

## 2. Cách ly dữ liệu

Integration fixtures tạo database ngẫu nhiên có tiền tố `culinary_fts_test_` và `culinary_http_test_`. KEEP_TEST_DATABASES=1 giữ nguyên chúng, không drop/reset bất kỳ database nào.

API runtime chạy ở `http://127.0.0.1:5080` với database test **`culinary_lab8_verify_306b24934a76`** và bucket test **`lab8-verify-18f32e2a02a9`**. Frontend production chạy `http://localhost:3100`, proxy đến API này. Seeder và toàn bộ thao tác mutation chỉ chạy trên database mới đó. Tài khoản Admin được seed trong database test; các Author dùng email test riêng. Không ghi vào database/bucket của nhóm.

Credentials được đọc từ User Secrets hiện có và truyền trong process environment, không in ra, không sửa hoặc ghi vào repository. Redis dùng kết nối 6380 hiện có; chỉ PING/readiness, không flush. Bucket test dùng chính cấu hình `PublicRead=true` hiện có của Development để kiểm chứng URL ảnh; không sửa policy bucket cũ. Database, bucket và services test được giữ để người dùng kiểm tra tiếp; không xóa dữ liệu hay volumes.

## 3. Lab 8 — kiểm chứng và sửa đúng phạm vi

`frontend/tests/e2e/lab8-runtime.spec.ts` có hai test dùng backend/PostgreSQL/MinIO thật:

1. Register Author → Create Draft → Guest bị từ chối Draft → upload không đăng nhập 401 → file giả bị từ chối → upload PNG thật → GET MinIO object khớp bytes → Update với RowVersion → thay ảnh chính → Publish → Recipe Detail hiển thị ảnh đã tải → Author directory không email → Admin directory có email và UI lọc Admin → Author khác archive bị 403 → Archive → Guest bị từ chối → publish lại Archived bị **422 theo validation contract hiện có** → soft delete → GET 404.
2. Trình duyệt mở form tạo → nhập title/description/ingredient/step → chọn file và thấy preview → gửi create/upload thật → vào trang edit có ảnh MinIO → sửa title bằng form update thật → reload vẫn giữ title đã sửa.

Test phát hiện trang edit thực tế chỉ có ảnh/nguyên liệu/bước nấu, chưa có phần sửa thông tin cơ bản. Sửa tối thiểu:

- `frontend/src/lib/api/recipes.ts`: thêm client gọi PUT API đã có.
- `frontend/src/components/recipes/RecipeContentManager.tsx`: thêm form title/description/category/difficulty/prep/cook/servings, dùng RowVersion hiện có. Giữ inputs khi request lỗi; không viết lại trang hoặc backend.

Lần đầu viết test có hai sai khác test contract (detail GET trả object trực tiếp; invalid state trả 422) và selector ingredient trùng; đã sửa test cho đúng API/DOM. Các lần chạy lặp cũng nhận 429 theo rate limit upload 5/phút. Không nới/tắt rate limiter; chờ cửa sổ mới rồi chạy lại, kết quả cuối 2/2 PASS. Không tính các lần lỗi trung gian là PASS.

## 4. OpenTelemetry và logging

API bật SDK + exporter tại runtime bằng process environment, OTLP gRPC endpoint `http://localhost:4317`; collector nhận gRPC 4317 và HTTP 4318, debug exporter xuất bằng chứng thật. Metric export interval 5 giây chỉ dùng trong process test.

Collector có ASP.NET/application spans, Npgsql database spans, MinIO upload spans, trace ID 32 hex, span ID 16 hex, parent ID. Gửi traceparent W3C cố định `0123456789abcdef0123456789abcdef` và CorrelationId `week678-runtime-verification` để đối chiếu log. API trả lại CorrelationId; structured log có TraceId/SpanId và application operation outcomes.

Ảnh chụp dữ liệu log cuối: 245 span entries với ID hợp lệ, 43 trace ID chung giữa collector và log API. Trace W3C cố định được thấy ở API log nhưng **không thấy trong cửa sổ log collector đã thu** (`propagatedTraceFound=false`); chưa xác định nguyên nhân và không coi trace cố định này là bằng chứng export. Bằng chứng export/correlation dùng 43 trace ID chung thực tế; propagation còn được kiểm tra riêng bằng HTTP test và API log. Script verification exit 0 dựa trên các bằng chứng phân biệt này.

Business metrics quan sát được: `culinary.recipe.searches`, `culinary.recipe.searches.empty`, `culinary.recipe.changes`, `culinary.image.uploads`, `culinary.operation.duration`. Có giá trị dương sau search/create/publish/archive/upload thật. Empty search đã gọi riêng và trả TotalCount=0 để xác minh counter không kết quả. File evidence ghi counts và giá trị mẫu, không suy ra PASS từ cấu hình/build.

Script quét collector bằng các JWT secret/MinIO credentials đang cấu hình: không tìm thấy; không có chuỗi JWT dạng access token. Đây là kiểm tra các giá trị đã biết và cấu trúc JWT, không phải chứng nhận toàn bộ dữ liệu lịch sử đã sạch. Custom labels vẫn chỉ operation/outcome hữu hạn, không userId/email/keyword.

Các test cũ `TelemetryExportTests` đã kiểm tra API vẫn hoạt động khi exporter không khả dụng. Phiên này không dừng MinIO/Redis/PostgreSQL của nhóm để mô phỏng outage; không tuyên bố có runtime outage test cho các dịch vụ đó.

## 5. Kết luận cuối cùng hiện hành

| Yêu cầu | Kết luận |
|---|---|
| FR-SRCH-004 | **ĐẠT** — PostgreSQL pagination/filter/sort integration + UI contract đã kiểm chứng |
| FR-OBS-003 | **ĐẠT** — collector nhận traces/metrics thực, có DB/MinIO spans và propagation |
| Lab 8 | **ĐẠT trong phạm vi chức năng đã yêu cầu và các luồng kiểm thử nêu trên** |
| PostgreSQL Integration | **PASS: 26/26** |
| MinIO Upload | **PASS** |
| Redis | **PASS** |
| OTLP Export | **PASS** |
| Playwright E2E thật | **PASS: 3 kịch bản**, thêm 1 UI contract PASS; không có SKIP trong các lần chạy này |
| Git Security | **CHƯA ĐẠT** — chủ credential cần xác nhận thu hồi credential lịch sử |
| Sẵn sàng nộp bài | **CHƯA** — còn điều kiện xác nhận Git Security |

Nhánh vẫn `fr-obs`; không pull/merge/rebase/commit/push, không sửa/xóa/áp dụng stash. Không thay credentials hoặc volumes. Production Release/backend deployment chưa được chứng nhận; giữ nguyên cảnh báo dependency Pkcs/ImageSharp và 18 lint warnings đã ghi nhận.

Scripts bổ sung chỉ phục vụ kiểm chứng: `scripts/start-week678-verification.py`, `scripts/verify-week678-telemetry.py`; script PostgreSQL hiện có được tái sử dụng. File `week678-runtime-state.json` chỉ chứa tên database/bucket và PID, không chứa secret. Chạy lại startup script khi cổng 5080 đang có process sẽ bị từ chối để tránh nhắm nhầm API.

---

# Bản ghi lịch sử trước khi khôi phục runtime — không dùng làm kết luận hiện hành

## Phạm vi và nguồn yêu cầu

Làm trực tiếp trên `fr-obs`, tái sử dụng source hiện có. Hai bản `SRS_Culinary_Blog_v1.0.1_Final.md` ở root và `docs/` có SHA256 giống nhau. Đã kiểm tra README, cấu trúc Clean Architecture, repository/UnitOfWork/DbContext, auth và ownership, migrations, API, frontend, Compose và báo cáo bảo mật trước đó. Không tạo lại dự án, không thêm migration, không thay credentials, không reset database, không xóa volumes, không commit/push/pull/merge/rebase hoặc áp dụng stash.

Trước khi sửa: nhánh `fr-obs`, chỉ có `FINAL_WORK_ACCEPTANCE_REPORT.md` chưa được theo dõi; giữ nguyên file đó. Hai stash dự phòng được giữ nguyên.

## A. Tuần 6 — FR-SRCH-004

Tái sử dụng `Common/Models/PagedResult.cs`, `Persistence/Repositories/RecipeRepository.cs`, `RecipeEndpoints.cs` và các trang public `/recipes`, `/recipes/search`.

- `TotalCount` được COUNT sau filter và trước Skip/Take; PostgreSQL thực thi phân trang trên IQueryable.
- Có `TotalPages`, `HasNextPage`, `HasPreviousPage`, thứ tự phụ `Id` tránh trùng do các bản ghi bằng khóa sort.
- Giới hạn pageSize 1–50, page >= 1 và kiểm tra offset bằng phép nhân long.
- Sửa lỗi pagination trả 422 thành HTTP **400**, giữ validation search/filter khác ở 422 theo contract hiện có.
- Thống nhất default search pageSize backend từ 10 thành **12** theo SRS. Frontend vẫn được phép gửi pageSize riêng.
- Bổ sung kiểm tra trong GetRecipesQueryHandler để lệnh nội bộ cũng không vượt giới hạn.
- URL page/sort/filter và thao tác reset page đã có ở các trang public; tái sử dụng.
- Playwright kiểm tra first/middle/last page, disabled buttons, request page thực, URL và browser Back bằng API interception. Đây là UI contract test, không phải PostgreSQL E2E.

Kết quả: phần code/build/unit/UI đạt các kiểm tra đã chạy; **chưa nghiệm thu integration PostgreSQL**.

## B. Tuần 7 — FR-OBS-003

File chính:

- `backend/src/CulinaryBlog.API/Extensions/TelemetryExtensions.cs`
- `backend/src/CulinaryBlog.Application/Common/Behaviors/TelemetryBehavior.cs`
- `backend/src/CulinaryBlog.Application/Features/Recipes/Commands/RecipeCommandHandler.cs`
- `backend/src/CulinaryBlog.Infrastructure/Storage/MinioFileStorageService.cs`
- `infrastructure/observability/otel/collector.yaml`
- `docker-compose.observability.yml`

Đã tích hợp SDK + OTLP, ASP.NET Core/HttpClient instrumentation và Npgsql native ActivitySource; không gắn thêm instrumentation EF gây duplicate. Tạo child spans cho MediatR operations và MinIO upload/read/delete. Custom telemetry không ghi request payload, keyword, email, userId hoặc credentials. Correlation middleware đưa TraceId/SpanId vào Serilog LogContext. Có startup/shutdown logs.

Metrics:

| Metric | Ý nghĩa |
|---|---|
| `culinary.recipe.searches` | Search hoàn thành thành công |
| `culinary.recipe.searches.empty` | Search thành công có TotalCount = 0 |
| `culinary.recipe.changes` | Create thành công hoặc trạng thái thực sự đổi sau SaveChanges |
| `culinary.image.uploads` | Outcome success/failure |
| `culinary.operation.duration` | Histogram giây theo tên operation và outcome hữu hạn |
| ASP.NET Core/HttpClient metrics | HTTP duration và status dimensions phục vụ count/error rate |

Publish/archive đã ở trạng thái đích không tăng metric. Create được đếm theo command lưu thành công; chưa có cơ chế idempotency key cho retry tạo mới qua nhiều request.

SDK mặc định bật, exporter mặc định tắt. Bật bằng `OpenTelemetry__ExportEnabled=true`; endpoint phát triển `OpenTelemetry__Endpoint=http://localhost:4317`. Collector bind localhost, debug exporter xuất traces và metrics. Hướng dẫn tại `docs/verification/WEEK678_OBSERVABILITY.md`.

Bằng chứng: `TelemetryBehaviorTests` kiểm tra child span cùng trace/parent, empty-search metrics và không đưa keyword vào tags. `TelemetryExportTests` gửi OTLP HTTP protobuf thật đến receiver trong test và nhận cả traces/metrics; test HTTP giữ W3C trace và trả 200 khi exporter endpoint không khả dụng. **Receiver test không phải collector Docker**; chưa tuyên bố collector PASS.

## C. Lab 8 và đối chiếu SRS

| Mã/phần | Ưu tiên | File triển khai chính | Trạng thái, phần thiếu và bằng chứng |
|---|---|---|---|
| FR-SRCH-001/002/003 | Cao | RecipeRepository, SearchRecipesQueryHandler, public search page | Giữ lại FTS/filter/sort; validation tests chạy; DB integration chưa đạt điều kiện môi trường |
| FR-SRCH-004 | Cao | PagedResult, RecipeEndpoints, public recipes/search pages | Sửa 400/default/overflow; unit và browser UI contract PASS; DB chưa kiểm chứng |
| FR-OBS-001 | Cao | API/HealthChecks, HealthEndpoints | Tái sử dụng, health tests PASS; readiness của dịch vụ thật chưa kiểm chứng |
| FR-OBS-002 | Cao | Program, CorrelationIdMiddleware, GlobalExceptionMiddleware | Structured logging hiện có + TraceId/SpanId + operation outcome/startup/shutdown; correlation tests PASS; sink/runtime toàn hệ thống chưa kiểm chứng |
| FR-OBS-003 | Cao | TelemetryExtensions, TelemetryBehavior, collector config | Trace propagation/metric/OTLP receiver tests PASS; collector Docker và DB spans thật chưa kiểm chứng |
| FR-FILE-001 / FR-RCP-008 | Cao | RecipeImageEndpoints, RecipeImageUploadValidator, UploadRecipeImageCommandHandler, MinioFileStorageService | Tái sử dụng MIME/signature/size/ownership, UUID key và xử lý lỗi; validator/storage unit tests PASS; MinIO thật chưa kiểm chứng |
| FR-RCP-003 | Cao | RecipeCommandHandler, dashboard/recipes/new/page.tsx | Create thật + nguyên liệu/bước nấu/upload đã có; thêm client image validation và preview; build/typecheck PASS; flow runtime chưa kiểm chứng |
| FR-RCP-004 | Cao | RecipeCommandHandler, dashboard recipe edit, RecipeContentManager | Update/RowVersion/child CRUD/upload đã có; thêm image preview; ownership/concurrency logic tests hiện có; runtime chưa kiểm chứng |
| FR-RCP-005/006/007 | Cao | Domain/Entities/Recipe.cs, RecipeCommandHandler, dashboard recipes | Giữ state machine, owner/Admin, Draft/Published/Archived và soft delete; workflow tests PASS; toàn luồng DB chưa kiểm chứng |
| User listing Lab 8 | Cao | IUserDirectory, UserDirectory, UserEndpoints, dashboard/users/page.tsx | API thật, DB pagination/search/role filter, DTO không password; Author chỉ hồ sơ Author không email, Admin có email; role/validation pre-DB tests PASS; DB/UI theo role thật chưa kiểm chứng |
| Authentication/Profile/Homepage/Categories/Detail | Giữ ổn định | Các handlers/API/pages hiện có | Không xây lại; regression unit/build chạy; tích hợp DB hiện chưa chạy thành công |

SRS không mô tả module quản trị user riêng. Quyền directory Author được giới hạn ở hồ sơ tác giả công khai, không cấp dữ liệu quản trị. Cần nhóm xác nhận chính sách mở rộng này cho Lab 8; không thêm field trạng thái tài khoản không tồn tại trong entity.

Upload preview dùng object URL tạm để xem trước; file vẫn gửi multipart qua API thật, không thay lưu trữ bằng Base64. Không sửa chính sách bucket/credentials. Chưa có bằng chứng ảnh hiển thị từ MinIO thật trong phiên này.

Ngoài phạm vi: không triển khai thêm các chức năng SRS không liên quan, không thay authentication hoặc kiến trúc Identity hiện có. Các module ngoài phạm vi không được coi là đã nghiệm thu chỉ vì file tồn tại.

## D. Build và tests thực tế

| Kiểm tra | Kết quả | Bằng chứng |
|---|---|---|
| Backend build .NET 10 | PASS, 0 errors | `week678-backend-build.log` |
| Application tests không bật DB | 87 PASS, 8 SKIP | `week678-tests.log`, TRX trong test project |
| API tests không bật DB | 30 PASS, 5 SKIP | `week678-tests.log`, TRX |
| Tổng bộ tests không bật DB | **117 PASS, 13 SKIP, 0 FAIL** | Bao gồm telemetry, upload validation, workflow, authorization/validation |
| Targeted telemetry/search validation | 35 PASS trong lần chạy trước nghiệm thu | `week678-targeted-tests.log` |
| TypeScript | PASS | `week678-typecheck.log` |
| ESLint | PASS, 0 errors, 18 warnings | `week678-lint.log` |
| Frontend production build | PASS | `week678-frontend-build.log` |
| Playwright UI với API interception | 1 PASS | `week678-e2e.log` |
| Playwright backend E2E | 1 SKIP | Thiếu stack thật; không tính là PASS |
| PostgreSQL integration có bật cấu hình | FAIL khởi tạo môi trường, chưa chạy nghiệp vụ | `week678-postgres-tests.log`: configured PostgreSQL `127.0.0.1:5433` từ chối kết nối |
| MinIO/Redis/Collector runtime | CHƯA KIỂM CHỨNG | localhost:9000/6379/4317 không khả dụng; Docker daemon không kết nối được |
| Backend readiness toàn stack | CHƯA KIỂM CHỨNG | Không thay credentials hoặc chạy ứng dụng vào DB khác |

Lệnh PostgreSQL đọc connection hiện có từ User Secrets, không in secret. Fixture tạo database tên ngẫu nhiên và chỉ migrate database test. Đặt KEEP_TEST_DATABASES=1 để không xóa database. Lần chạy này lỗi kết nối ngay ở khởi tạo, chưa tạo được database. Cổng 5432 có listener nhưng không xác định thuộc dự án; không tự thay cấu hình sang dịch vụ đó.

`npm.cmd` được dùng do PowerShell chặn npm.ps1. Playwright runner báo 1 passed/1 skipped; PowerShell redirection ghi NativeCommandError cho cảnh báo Node NO_COLOR và wrapper exit 1. Báo cáo dựa vào kết quả test runner, không giấu khác biệt này.

Cảnh báo tồn tại: dependency gián tiếp `System.Security.Cryptography.Pkcs` 9.0.11 có advisory NU1902/NU1903; ImageSharp 4.1.3 cảnh báo thiếu license cho Release build. Không chứng nhận production Release build. Các bản OpenTelemetry bổ sung dùng 1.20.0, thay bản 1.15.0 thử ban đầu vì NuGet cảnh báo bảo mật.

## E. Git và bảo mật

Nhánh `fr-obs`, chưa commit/push. Giữ nguyên cả hai stash. Danh sách file cuối báo cáo được lấy bằng `git status --short`. Các log kiểm chứng cục bộ bị ignore theo cấu hình repository. Không ghi `.env`, User Secrets hoặc credentials vào báo cáo/code.

Credential PostgreSQL từng xuất hiện trong lịch sử: **CHƯA XÁC NHẬN THU HỒI** theo `docs/verification/GIT_SECURITY_2026-10-27.md`; không coi việc xóa secret khỏi working tree là đã xử lý lịch sử. Git Security tổng thể **CHƯA ĐẠT** cho đến khi chủ credential xác nhận thu hồi.

## F. Kết luận nghiệm thu

1. **FR-SRCH-004: CHƯA ĐẠT nghiệm thu đầy đủ** — code/unit/UI đạt; PostgreSQL integration thiếu bằng chứng thành công.
2. **FR-OBS-003: CHƯA ĐẠT nghiệm thu đầy đủ** — SDK/traces/metrics/OTLP receiver tests đạt; Docker collector và DB trace thật chưa kiểm chứng.
3. **Lab 8: CHƯA ĐẠT nghiệm thu đầy đủ** — đã bổ sung code cần thiết, còn thiếu MinIO/DB/browser flows thật và xác nhận chính sách user directory.
4. **Backend/Frontend: PASS build Debug/production Next.js**; còn các cảnh báo nêu trên.
5. **Docker Runtime: CHƯA KIỂM CHỨNG**.
6. **Git Security: CHƯA ĐẠT** — chưa xác minh thu hồi credential lịch sử.
7. **Sẵn sàng nộp bài: CHƯA**.

Cần khôi phục đúng stack development (PostgreSQL tại endpoint cấu hình, Redis, MinIO, collector), chạy lại integration bằng `python scripts/verify-week678-postgres.py`, chạy E2E thật với CULINARYBLOG_E2E=1 và thu log collector theo hướng dẫn. Nhóm xác nhận quyền directory Author và chủ credential xác nhận thu hồi. Không có kết quả runtime nào được suy ra chỉ từ build/source.

Docker Compose b? sung collector: `docker compose -f docker-compose.yml -f docker-compose.observability.yml config --quiet` PASS (exit 0); ch? x?c minh c?u h?nh, kh?ng ph?i runtime.

### File s?a/th?m cu?i phi?n

```text
 M backend/src/CulinaryBlog.API/CulinaryBlog.API.csproj
 M backend/src/CulinaryBlog.API/Endpoints/RecipeEndpoints.cs
 M backend/src/CulinaryBlog.API/Extensions/EndpointExtensions.cs
 M backend/src/CulinaryBlog.API/Extensions/ServiceCollectionExtensions.cs
 M backend/src/CulinaryBlog.API/Middleware/CorrelationIdMiddleware.cs
 M backend/src/CulinaryBlog.API/Middleware/GlobalExceptionMiddleware.cs
 M backend/src/CulinaryBlog.API/Program.cs
 M backend/src/CulinaryBlog.Application/Features/Recipes/Commands/RecipeCommandHandler.cs
 M backend/src/CulinaryBlog.Application/Features/Recipes/Queries/GetRecipes/GetRecipesQueryHandler.cs
 M backend/src/CulinaryBlog.Application/Features/Recipes/Queries/SearchRecipes/SearchRecipesQueryHandler.cs
 M backend/src/CulinaryBlog.Infrastructure/Storage/MinioFileStorageService.cs
 M backend/tests/CulinaryBlog.API.Tests/MissingFeaturesHttpTests.cs
 M backend/tests/CulinaryBlog.Application.Tests/RecipeSearchValidationTests.cs
 M backend/tests/CulinaryBlog.Application.Tests/SearchRecipesQueryTests.cs
 M frontend/src/app/(dashboard)/dashboard/recipes/new/page.tsx
 M frontend/src/components/dashboard/mobile-navigation.tsx
 M frontend/src/components/layout/Sidebar.tsx
 M frontend/src/components/recipes/RecipeContentManager.tsx
?? FINAL_WEEK6_WEEK7_LAB8_REPORT.md
?? FINAL_WORK_ACCEPTANCE_REPORT.md
?? backend/src/CulinaryBlog.API/Endpoints/UserEndpoints.cs
?? backend/src/CulinaryBlog.API/Extensions/TelemetryExtensions.cs
?? backend/src/CulinaryBlog.Application/Common/Behaviors/TelemetryBehavior.cs
?? backend/src/CulinaryBlog.Application/Common/Exceptions/PaginationException.cs
?? backend/src/CulinaryBlog.Application/Common/Interfaces/IUserDirectory.cs
?? backend/src/CulinaryBlog.Infrastructure/Identity/UserDirectory.cs
?? backend/tests/CulinaryBlog.API.Tests/TelemetryExportTests.cs
?? backend/tests/CulinaryBlog.API.Tests/UserDirectoryTests.cs
?? backend/tests/CulinaryBlog.Application.Tests/TelemetryBehaviorTests.cs
?? docker-compose.observability.yml
?? docs/verification/WEEK678_OBSERVABILITY.md
?? frontend/src/app/(dashboard)/dashboard/users/
?? frontend/tests/e2e/pagination-ui.spec.ts
?? infrastructure/observability/otel/collector.yaml
?? scripts/verify-week678-postgres.py
```
