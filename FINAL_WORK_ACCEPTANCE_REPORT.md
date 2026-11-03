# Báo cáo nghiệm thu Culinary Blog

Ngày kiểm tra: 27/10/2026, Asia/Saigon. Phạm vi: source thực tế tại HEAD `0bf7b25c2ce79623340bbb2fdf8c6bda332163ae` và các yêu cầu nghiệm thu trong file đính kèm. Báo cáo dựa trên đọc source, các lệnh build/test chạy mới và kiểm tra API/trình duyệt trong lượt này, không lấy kết luận PASS từ báo cáo cũ.

## 1. Git và phạm vi thay đổi

- Nhánh: `fr-obs`.
- Commit mới nhất: `0bf7b25 Merge develop into fr-obs`, hai parent `02e506a` và `24da869`.
- Không còn MERGING, 0 unmerged files, không có conflict markers trong `backend/src` và `frontend/src`.
- Khi bắt đầu kiểm tra: working tree sạch; nhánh ahead local remote-tracking ref `origin/fr-obs` 9 commits.
- Merge commit thay đổi **113 file** so với parent thứ nhất. Đó là số file của commit merge, không phải 113 file đang sửa trong working tree.
- `origin/develop` local tại `24da8699977ea9cc7a9bd16d50f367ec0551bf10` là ancestor của HEAD: PASS.
- **Remote develop đã thay đổi:** `git ls-remote origin refs/heads/develop` trong lượt nghiệm thu trả `a6583e8f47242b1606ce83fbe786bd8b190e80ef`. Hash này không có trong `git rev-list HEAD`. Vì vậy **chưa tích hợp develop mới nhất trên GitHub**. Không fetch/pull/merge thêm, không suy đoán nội dung commit mới chưa tải về.
- Hai stash dự phòng giữ nguyên: `27b1d82d033b8516c6c7f909cf4d3d896dc41920` và `7ea0ecefbb56cc1cfe5bfa844428140c2890166e`. Không apply/drop.
- Không sửa source, không stage/commit/push. File cần giao mới là báo cáo này. Các helper nghiệm thu nằm trong `scripts/local/`, bị ignore; logs và test outputs cũng bị ignore.

## 2. Bảng nghiệm thu

PASS nghĩa là đã thực thi kiểm chứng cho hành vi được mô tả trong hàng; không đại diện cho mọi tình huống production có thể có.

| STT | Chức năng | File triển khai | Bằng chứng kiểm thử mới | Trạng thái |
|---|---|---|---|---|
| 1 | FR-SRCH-002: Category/Difficulty/CookTime, AND, kết hợp full-text | `RecipeEndpoints.cs:304`, `SearchRecipesQuery.cs:8`, `SearchRecipesQueryHandler.cs:37`, `RecipeRepository.cs:275` | `FiltersAndAllSorts_ComposeBeforePagination_WithStableTies` chạy trên PostgreSQL thật; E2E kết hợp category/difficulty/maxCookTime | PASS |
| 2 | Filter validation, kết quả rỗng, phân trang sau lọc | `RecipeEndpoints.cs:304`, `SearchRecipesQueryHandler.cs:29`, `RecipeRepository.cs:282` | HTTP tests invalid GUID/enum/time/sort/pagination; PostgreSQL tests count và empty; browser empty search | PASS |
| 3 | FR-SRCH-003: createdAt ASC/DESC | `RecipeRepository.cs:94`, `RecipeRepository.cs:296` | Integration test so sánh ID theo thứ tự mong đợi, cả list/search và bộ lọc | PASS |
| 4 | FR-SRCH-003: title ASC/DESC | `RecipeRepository.cs:100`, `RecipeRepository.cs:298` | Cùng integration test và E2E `-title` với filter | PASS |
| 5 | FR-SRCH-003: cookTime ASC/DESC | `RecipeRepository.cs:105`, `RecipeRepository.cs:300` | Cùng integration test với thời gian nấu 10/20 phút | PASS |
| 6 | Sort sai và thứ tự phụ ổn định | `SearchRecipesQuery.cs:35`, `RecipeRepository.cs:116`, `RecipeRepository.cs:310` | Trùng title/date/cookTime vẫn cho hai trang không trùng ID; HTTP sort sai 422 | PASS |
| 7 | Đăng ký/đăng nhập/đăng xuất qua frontend và API | `AuthEndpoints.cs`, `IdentityService.cs`, `LoginForm.tsx`, `RegisterForm.tsx`, `AuthProvider.tsx` | Browser đăng ký mới 201, logout 200 và xóa accessToken; login sai 401, login đúng 200 và header cập nhật tài khoản | PASS |
| 8 | Đổi mật khẩu và thu hồi phiên cũ | `AccountService.cs:31`, `AuthenticationExtensions.cs:29`, `PasswordForm.tsx` | PostgreSQL tests mật khẩu cũ/confirmation/revocation; HTTP/E2E JWT cũ 401, mật khẩu mới đăng nhập được | PASS |
| 9 | Forgot/reset, token sai/hết hạn/đã dùng, email Mailpit | `AccountService.cs:43`, `AccountService.cs:61`, `PasswordResetEmailSender.cs`, `PasswordForm.tsx` | Integration dùng TimeProvider kiểm tra expiry; HTTP token dùng lại 422; E2E nhận email SMTP thật, reset form và login | PASS |
| 10 | Profile, field được phép, validation/quyền, Auth Store | `AuthEndpoints.cs`, `AccountService.cs:97`, `ProfileForm.tsx`, `AuthProvider.tsx` | Test chặn guest, role giả mạo không đổi; PostgreSQL lưu tên/bio; E2E lưu/reload; error/loading/success có trong source và success hiển thị thực tế | PASS |
| 11 | Upload avatar và hiển thị ảnh mới | `AuthEndpoints.cs`, `RecipeImageUploadValidator.cs`, `ProfileForm.tsx:23` | HTTP upload PNG; E2E ảnh hiển thị, profile reload và tài khoản hợp lệ | PASS |
| 12 | Homepage: header/footer/sidebar/cards/latest/search/detail links/responsive | `frontend/src/app/page.tsx:12`, `header.tsx`, `footer.tsx`, `recipe-card.tsx` | E2E title mới nhất khớp API, category gọi API thật, viewport 390px không tràn; browser Recipe Detail hoạt động | PASS |
| 13 | Homepage loading/error/empty | `frontend/src/app/page.tsx:27`, `recipe-grid.tsx:20` | Browser làm chậm rồi ngắt request để xác minh loading/error; empty bằng API thật với categoryId không tồn tại, totalCount=0 | PASS |
| 14 | Recipe Detail: API, nguyên liệu, steps, thời gian/độ khó, missing slug | `RecipeDetailPageContent.tsx:53`, `GetRecipeBySlugQueryHandler.cs` | GET 200, ingredients/steps không rỗng và hiển thị trong browser; slug không tồn tại API 404, UI error sau React Query retries | PASS |
| 15 | Recipe Detail: ảnh ngoài trong dữ liệu mẫu | `RecipeDetailPageContent.tsx:56`, `RecipeDetailPageContent.tsx:67` | API có image URL Picsum; browser naturalWidth=0, chưa tải xong trong 30 giây; chưa có bằng chứng ảnh hiển thị thành công | CHƯA KIỂM CHỨNG |
| 16 | Search tiếng Việt có dấu/không dấu, full-text, empty/pagination | `RecipeSearchText.cs`, `RecipeRepository.cs:269`, `recipes/search/page.tsx` | PostgreSQL tests phở/pho/prefix/ranking/vector trigger; browser từ khóa có dấu/không dấu từ title thực tế trả cùng ID; empty UI hoạt động | PASS |
| 17 | Categories list/detail, recipes theo category và pagination | `CategoryEndpoints.cs`, `categories/page.tsx`, `categories/[slug]/page.tsx` | Browser list/detail/page 2; API pagination size 1; Categories lấy từ API thật | PASS |
| 18 | Categories page/pageSize defaults và input 400 | `CategoryEndpoints.cs:92`, `MissingFeaturesHttpTests.cs:112` | 14 HTTP theory cases PASS; kiểm tra thêm trực tiếp default 1/12, 0/51/chữ/rỗng/lặp page trả 400 | PASS |
| 19 | PostgreSQL/Redis/Mailpit/MinIO readiness | Docker services và health endpoints | PostgreSQL accepting connections; Redis PONG; Mailpit `/livez` 200; MinIO live/ready 200 | PASS |
| 20 | MinIO upload/avatar end-to-end trong môi trường hiện tại | `MinioFileStorageService.cs` | Service đang chạy, có tests của implementation; E2E nghiệm thu dùng Local storage, không thực hiện upload thật đến MinIO | CHƯA KIỂM CHỨNG |
| 21 | Cấu hình hiện tại không chứa credential hạ tầng thật | appsettings, `.env.example`, Compose, `.gitignore` | Scan hiện tại; User Secrets source=secrets.json và SELECT 1 PASS; template secrets là placeholder | PASS |
| 22 | Credential PostgreSQL lịch sử đã thu hồi | Commit `87ec472`, hai appsettings lịch sử | Vẫn có password-bearing connection strings; không có xác nhận rotate/revoke từ chủ sở hữu | CHƯA KIỂM CHỨNG |
| 23 | Tích hợp code develop mới nhất trên remote | Git commit graph | Đã có `24da869`; remote hiện là `a6583e8`, chưa reachable từ HEAD | FAIL |

## 3. Filter và sort thực hiện ở PostgreSQL

Không chỉ lọc mock: `RecipeRepository` dùng `IQueryable` của DbContext, nối các `Where` cho Category/Difficulty/CookTime; search dùng Npgsql `Matches(ToTsQuery("public.vietnamese", ...))` trên SearchVector. Các điều kiện lọc đặt trước CountAsync và trước Skip/Take. Sáu kiểu OrderBy/OrderByDescending thực hiện trước ToListAsync; list dùng Id làm thứ tự phụ, search dùng CreatedAt DESC rồi Id.

Integration test tạo dữ liệu đối chứng ở database PostgreSQL riêng: hai công thức đủ điều kiện, một quá lâu, một sai độ khó và một khác category. Cả sáu sort trên list/search được so với ID mong đợi. Sau đó tạo ties và xác minh phân trang không lặp. Đây là test chạy mới, không suy từ mô tả code.

Search endpoint hỗ trợ `sort` và alias `sortBy`/`sortOrder`. List endpoint hiện dùng `sort`. Kết luận sáu kiểu sort ở trên dựa trên contract `sort` hiện có, không khẳng định mọi alias đều có trên mọi endpoint.

## 4. Build và test thực thi mới

| Kiểm tra | Kết quả |
|---|---|
| Backend `dotnet build backend/CulinaryBlog.slnx --no-restore -v quiet` | PASS, 0 errors |
| Frontend `npm.cmd --prefix frontend run build` | PASS, production, 22 routes/pages trong output |
| Frontend `npm.cmd --prefix frontend run typecheck` | PASS |
| Frontend `npm.cmd --prefix frontend run lint` | PASS, 0 errors / 15 warnings |
| `dotnet test ... --no-build --no-restore` với PostgreSQL thật | **128 passed / 0 failed / 0 skipped** |
| Application.Tests | 88 passed |
| API.Tests | 40 passed |
| PostgreSQL integration trong RecipeFullTextSearchTests | 8 passed |
| PostgreSQL + HTTP trong MissingFeaturesHttpTests | 18 passed, gồm 14 trường hợp Categories pagination |
| Playwright E2E có sẵn, chạy lại với `CULINARYBLOG_E2E=1` | **1 passed / 0 failed / 0 skipped**, tổng 7,5 giây, test 5,0 giây |
| Kiểm tra browser bổ sung bằng helper cục bộ | 6 nhóm PASS; ảnh Recipe Detail ngoài CHƯA KIỂM CHỨNG |
| Git diff check | PASS |

Backend tests dùng database tên riêng do fixtures tạo. Theo chấp thuận của người dùng, bật `CULINARYBLOG_KEEP_TEST_DATABASES=1`: không xóa các database test sau chạy. E2E dùng `culinary_missing_runtime_65693c04ef024cfe93e3ab24ac8cef52`, database runtime test riêng đã có; tạo tài khoản/dữ liệu thử ở đó, không đổi database ứng dụng hoặc credential hạ tầng.

API chạy tại `http://127.0.0.1:5080`; E2E nghiệm thu dùng frontend production mới `http://localhost:3002`, SMTP/API Mailpit hiện có. FileStorage provider là Local. Không mock response dữ liệu. Kiểm tra empty Homepage chỉ đổi query categoryId đến một GUID không có dữ liệu và vẫn nhận response API thật; kiểm tra loading/error chủ động trì hoãn/ngắt request mạng để kích hoạt các nhánh UI.

### Những lần kiểm tra chưa PASS ban đầu

- Backend build đầu tiên thất bại vì DLL đang bị tiến trình API test khóa. Dừng đúng API của workspace rồi build lại PASS; không sửa source.
- E2E đầu tiên chạy trên frontend cũ cổng 3001 sau khi build output đã đổi gặp error page ở Profile. Dùng tiến trình production mới cổng 3002 và đồng bộ URL reset trong environment API: toàn bộ E2E PASS. Không có thay đổi source giữa hai lần.
- Helper bổ sung ban đầu dùng sai locator heading cho CardTitle Categories, từ khóa phở không có trong dataset runtime mẫu và timeout 5 giây ngắn hơn React Query retries cho 404. Đã sửa helper bị ignore theo dữ liệu/source thực tế và kiểm chứng lại các hành vi; đây không phải sửa source sản phẩm. Ảnh Recipe Detail vẫn không được nâng lên PASS.

Bằng chứng cục bộ (Git ignored): `acceptance-backend-build.log`, `acceptance-frontend-build.log`, `acceptance-frontend-lint.log`, `docs/verification/test-results/acceptance_*.trx`, `docs/verification/test-results/acceptance-ui.json`, `frontend/test-results/.last-run.json`. Helper chỉ dùng để nghiệm thu nằm trong `scripts/local/acceptance-ui.cjs` và helper cấu hình đã có.

## 5. Authentication, Profile và dữ liệu nhạy cảm

- PasswordHasher xử lý hash mật khẩu. Change/reset kiểm tra confirmation và strength; refresh tokens bị revoke, JWT chứa phiên bản password hash và validation đối chiếu database.
- Reset token Data Protection, gắn user/password version, hết hạn 30 phút, vô hiệu sau dùng; tests hết hạn và dùng lại chạy trên PostgreSQL với clock điều khiển.
- Forgot trả thông báo chung; email reset thực tế nhận trong Mailpit. Không in token/link/secret vào báo cáo.
- Profile chỉ cho sửa displayName/bio/avatarUrl; guest bị chặn; payload role giả mạo không đổi role. Frontend gọi API thật và cập nhật Auth Store/AuthProvider.
- Homepage lấy Categories/Latest Recipes qua API thật. Recipe Detail có fallback mock **chỉ khi** bật `NEXT_PUBLIC_USE_DEMO_DATA=true` và API lỗi; đường nghiệm thu thành công lấy dữ liệu API. Không kết luận repo hoàn toàn không có mock/demo code.

## 6. Git Security

Appsettings hiện tại không còn password connection string hay secret literal. `.env.example` có placeholder rõ ràng; Compose đọc PostgreSQL credential từ environment/.env. `.env` thật, User Secrets, logs, runtime/test outputs và build cache được ignore. `UserSecretsId` hiện hữu và helper cấu hình xác nhận connection string/JWT lấy từ `secrets.json`; SELECT 1 thành công, không hiển thị giá trị.

Kiểm tra các config thay đổi trong dải `origin/fr-obs..HEAD` không phát hiện credential vận hành mới ở nội dung config hiện tại. Commit trung gian `39b3737` có thêm password-bearing line trong `.env.example`, nhưng giá trị là placeholder rõ ràng; không coi đó là bằng chứng rò rỉ credential thật. Việc không phát hiện bằng kiểm tra này không thay thế xác nhận của chủ sở hữu credential lịch sử.

Commit lịch sử `87ec472` vẫn chứa password-bearing connection string ở cả hai appsettings. **CHƯA có bằng chứng credential cũ đã rotate/revoke.** Không thử đăng nhập bằng credential cũ và không thay mật khẩu. Kết luận Git Security: **CHƯA ĐẠT**, phần xác nhận thu hồi là **CHƯA KIỂM CHỨNG**.

Source còn mật khẩu tài khoản demo cố định trong `backend/src/CulinaryBlog.Infrastructure/Persistence/Seed/DatabaseSeeder.cs:42`, có role quản trị; API gọi seeder ở `backend/src/CulinaryBlog.API/Program.cs:283` không có điều kiện chỉ Development quanh đoạn seed. Đây không phải secret PostgreSQL mới, nhưng là rủi ro nếu đưa nguyên seed lên môi trường thật. Không ghi các giá trị vào báo cáo, không sửa source theo quy tắc nghiệm thu.

## 7. Giới hạn, vấn đề và hướng khắc phục

| Vấn đề | File/dòng hoặc bằng chứng | Nguyên nhân xác minh được | Hướng xử lý, chưa thực hiện |
|---|---|---|---|
| Chưa tích hợp remote develop mới nhất | `git ls-remote`: `a6583e8`; không có trong `git rev-list HEAD` | Remote đổi sau mốc `24da869` đã merge | Được phép riêng rồi đọc diff commit mới, đánh giá và tích hợp; không tự pull trong nghiệm thu |
| Ảnh Recipe Detail chưa xác minh hiển thị | `RecipeDetailPageContent.tsx:56`, `:67`; URL Picsum từ API | Image request chưa hoàn tất, naturalWidth=0 sau 30 giây; chưa xác định lỗi server ảnh, mạng hay xử lý asset | Kiểm tra endpoint ảnh ngoài từ môi trường đích; dùng ảnh kiểm soát được/MinIO test và kiểm tra lại; cân nhắc fallback/onError. Không quy kết lỗi merge khi chưa có bằng chứng |
| Credential lịch sử chưa xác nhận thu hồi | `87ec472`, hai appsettings lịch sử | Không có xác nhận từ chủ PostgreSQL | Trưởng nhóm/chủ database rotate/revoke và cập nhật nơi lưu secrets; xác nhận credential cũ vô hiệu trước push/nộp |
| Seed tài khoản demo có password cố định | `DatabaseSeeder.cs:42`, `Program.cs:283` | Seed không bị giới hạn Development trong code hiện tại | Xin phép sửa riêng: chỉ seed demo trong Development/test hoặc dùng quy trình provisioning an toàn cho môi trường thật |
| MinIO upload thực tế chưa được nghiệm thu | E2E chạy Local provider; MinIO health 200 | Readiness không chứng minh toàn bộ object upload flow | Chạy ca upload/read ảnh trên bucket test khi được cho phép; không dùng bucket thật |
| Release/backend dependency warnings | Output Backend Build | Pkcs 9.0.11 warnings NU1902/NU1903; ImageSharp yêu cầu license cho Release | Đánh giá dependency/license riêng; không tự nâng thư viện trong nghiệm thu |

Không phát hiện lỗi chức năng tái hiện do merge trong các ca đã PASS. Không đồng nghĩa chứng minh không thể có lỗi ngoài phạm vi kiểm tra. Chưa test SMTP production, mọi thiết bị/trình duyệt hoặc deployment production.

## 8. Trả lời 12 câu hỏi nghiệm thu

1. **FR-SRCH-002 hoàn thành 100% chưa?** Đạt toàn bộ tiêu chí filter trong yêu cầu nghiệm thu: code có thật, PostgreSQL thực thi, tests và UI kết hợp PASS. Kết luận giới hạn trong các tiêu chí đã liệt kê.
2. **FR-SRCH-003 hoàn thành 100% chưa?** Đạt sáu kiểu sort và các tiêu chí kết hợp filter/search, invalid sort, database sorting, stable pagination đã yêu cầu.
3. **Authentication đủ chưa?** Có đủ sáu chức năng; backend tests và frontend/API/Mailpit nghiệm thu PASS trong môi trường phát triển.
4. **Profile/Upload Avatar hoạt động chưa?** PASS với Local storage; đồng bộ và reload thực tế. Không gộp kết luận này với MinIO E2E chưa chạy.
5. **Homepage đáp ứng chưa?** PASS các thành phần và trạng thái được yêu cầu, dữ liệu từ API thật, responsive 390px đã kiểm chứng.
6. **Recipe Detail/Search/Categories hoàn thành chưa?** Search và Categories PASS. Recipe Detail data/404 PASS; ảnh ngoài chưa kiểm chứng thành công, nên chưa kết luận toàn bộ Recipe Detail 100%.
7. **Backend/Frontend build thành công không?** Có, các lần build cuối PASS; TypeScript/ESLint PASS với cảnh báo đã ghi.
8. **Tests PASS không?** Backend 128/128, không fail/skip; E2E chính 1/1 PASS. Kiểm tra bổ sung ảnh ngoài vẫn CHƯA KIỂM CHỨNG; không che các lần thất bại ban đầu của môi trường/helper.
9. **Code mới từ develop tích hợp đầy đủ chưa?** Đã tích hợp mốc `24da869`; **chưa** tích hợp remote hiện tại `a6583e8`, không thể kết luận mới nhất đầy đủ.
10. **Có chức năng lỗi sau merge không?** Không có lỗi merge tái hiện trong bộ tests cuối; còn giới hạn ảnh ngoài và các vấn đề mục 7, không tự suy chúng là lỗi merge.
11. **Git Security đạt chưa?** **CHƯA ĐẠT**, credential lịch sử chưa có xác nhận thu hồi; có thêm rủi ro seed demo nếu triển khai thật.
12. **Đủ điều kiện nộp GitHub chưa?** **CHƯA** theo phạm vi nghiệm thu toàn bộ: cần xác nhận credential lịch sử, xử lý/đánh giá develop mới và hoàn tất bằng chứng ảnh Recipe Detail. Không tự commit/push báo cáo hay source.
