Các chức năng cần làm
3.2. Module Quản lý Danh mục (FR-CAT)
Module quản lý danh mục (Category) phân loại công thức nấu ăn. Danh mục được tạo và duy
trì bởi Admin; Author và Guest chỉ có quyền đọc. Mỗi danh mục có Slug duy nhất phục vụ
URL thân thiện SEO. Danh mục được cache với IMemoryCache (TTL 1 giờ) vì thay đổi ít
thường xuyên.
FR-CAT-001: Xem Danh sách Danh mục
Mã yêu cầu FR-CAT-001
Tên yêu cầu Xem Danh sách Tất cả Danh mục
Culinary Blog – Tài liệu Đặc tả Yêu cầu Phần mềm (SRS) v1.0.0
CONFIDENTIAL • Phát triển Ứng dụng Web Nâng cao V4 • Trang 24 / 71
Nhóm chức năng Module Quản lý Danh mục (FR-CAT)
Tác nhân Tất cả (Guest / Author / Admin)
Mức ưu tiên
(MoSCoW)
M – Must Have
Mô tả
Trả về danh sách tất cả danh mục công thức hiện có trong hệ thống,
kèm số lượng công thức đã xuất bản (Published) trong mỗi danh mục.
Kết quả được cache với IMemoryCache (TTL 60 phút) và sắp xếp theo
Name tăng dần.
Điều kiện tiên quyết
1. Ít nhất một danh mục tồn tại trong database (hoặc trả về mảng rỗng).
2. Không yêu cầu xác thực.
Luồng chính (Happy
Path)
1. Client gửi GET /api/v1/categories.
2. GetCategoriesQuery dispatch qua MediatR.
3. Handler kiểm tra IMemoryCache với key "categories:all".
4. Cache hit: trả về dữ liệu từ cache.
5. Cache miss: query database
(IUnitOfWork.Categories.GetAllWithRecipeCount()), map sang
CategoryDto[].
6. Lưu vào IMemoryCache với TTL 60 phút (sliding expiration).
7. Trả về HTTP 200 OK với CategoryDto[].
Luồng thay thế /
Ngoại lệ
A1 – Không có danh mục nào: HTTP 200 OK với mảng rỗng [].
HTTP Method &
Endpoint
GET /api/v1/categories
Kết quả mong đợi
Mảng CategoryDto[] với các field: { id, name, slug, description,
recipeCount }. Kết quả được serve từ cache khi có.
HTTP Status Code trả
về
200 OK – Thành công (kể cả khi trống).
FR-CAT-002: Xem Chi tiết Danh mục và Công thức
Mã yêu cầu FR-CAT-002
Tên yêu cầu Xem Chi tiết Danh mục và Danh sách Công thức thuộc Danh mục
Nhóm chức năng Module Quản lý Danh mục (FR-CAT)
Tác nhân Tất cả (Guest / Author / Admin)
Mức ưu tiên
(MoSCoW)
M – Must Have
Mô tả
Trả về thông tin chi tiết của một danh mục cụ thể (theo Slug) kèm danh
sách phân trang các công thức đã xuất bản (Published) thuộc danh
mục đó. Guest chỉ thấy Published recipes; Author thấy thêm Draft
recipes của chính mình trong danh mục.
Điều kiện tiên quyết
1. Danh mục với slug tương ứng phải tồn tại. 2. Không yêu cầu xác
thực.
Luồng chính (Happy
Path)
1. Client gửi GET /api/v1/categories/{slug}?page=1&pageSize=12.
2. GetCategoryBySlugQuery dispatch qua MediatR.
Culinary Blog – Tài liệu Đặc tả Yêu cầu Phần mềm (SRS) v1.0.0
CONFIDENTIAL • Phát triển Ứng dụng Web Nâng cao V4 • Trang 25 / 71
3. Handler tìm category theo slug:
_unitOfWork.Categories.GetBySlugAsync(slug).
4. Query recipes thuộc category với Status == Published (+ Draft của
currentUser nếu đã đăng nhập).
5. Apply pagination (OFFSET-based: SKIP (page-1)*pageSize TAKE
pageSize).
6. Map sang CategoryDetailDto kèm
PagedResult<RecipeSummaryDto>.
7. Trả về HTTP 200 OK.
Luồng thay thế /
Ngoại lệ
A1 – Slug không tồn tại: HTTP 404 Not Found với RFC 7807 body.
HTTP Method &
Endpoint
GET /api/v1/categories/{slug}?page={n}&pageSize={n}
Kết quả mong đợi
{ category: CategoryDto, recipes: { items: RecipeSummaryDto[],
totalCount, page, pageSize, totalPages } }
HTTP Status Code trả
về
200 OK – Thành công. 404 Not Found – Slug không tồn tại.
FR-CAT-003: Tạo Danh mục Mới [Admin]
Mã yêu cầu FR-CAT-003
Tên yêu cầu Tạo Danh mục Công thức Mới
Nhóm chức năng Module Quản lý Danh mục (FR-CAT)
Tác nhân Quản trị viên (Admin)
Mức ưu tiên
(MoSCoW)
M – Must Have
Mô tả
Admin tạo danh mục công thức mới. Slug được tự động sinh từ Name
(slugify: chuyển sang chữ thường, bỏ dấu, thay khoảng trắng bằng "-").
Nếu Slug đã tồn tại, hệ thống thêm suffix số (e.g., "mon-chinh-2"). Sau
khi tạo, cache danh mục (IMemoryCache key "categories:all") bị
invalidate.
Điều kiện tiên quyết
1. Người dùng đang đăng nhập với role Admin. 2. Name chưa tồn tại
trong database.
Luồng chính (Happy
Path)
1. Admin gửi POST /api/v1/categories với Authorization: Bearer
{adminJwt} và body: { "name": "...", "description": "..." }.
2. RequireAuthorization("Admin") middleware kiểm tra role.
3. CreateCategoryCommand dispatch qua MediatR.
4. ValidationBehavior: name 2–50 ký tự, không chứa HTML.
5. SlugHelper.Generate(name) tạo slug.
6. Kiểm tra slug chưa tồn tại. Nếu trùng, thêm "-2", "-3",... cho đến khi
unique.
7. Category.Create(name, slug, description) tạo entity.
8. _unitOfWork.Categories.AddAsync(entity).
9. _unitOfWork.SaveChangesAsync().
10. MemoryCache.Remove("categories:all") – invalidate cache.
11. Trả về HTTP 201 Created với CategoryDto và Location header.
Culinary Blog – Tài liệu Đặc tả Yêu cầu Phần mềm (SRS) v1.0.0
CONFIDENTIAL • Phát triển Ứng dụng Web Nâng cao V4 • Trang 26 / 71
Luồng thay thế /
Ngoại lệ
A1 – Thiếu role Admin: HTTP 403 Forbidden.
A2 – Dữ liệu không hợp lệ: HTTP 422.
HTTP Method &
Endpoint
POST /api/v1/categories
Kết quả mong đợi
Danh mục mới được tạo trong database. Cache danh mục bị xóa.
Location header trỏ đến /api/v1/categories/{newSlug}.
HTTP Status Code trả
về
201 Created – Tạo thành công. 403 Forbidden – Không có quyền
Admin. 409 Conflict – Name đã tồn tại. 422 Unprocessable Entity – Dữ
liệu không hợp lệ.
FR-CAT-004: Cập nhật Danh mục [Admin]
Mã yêu cầu FR-CAT-004
Tên yêu cầu Cập nhật Thông tin Danh mục
Nhóm chức năng Module Quản lý Danh mục (FR-CAT)
Tác nhân Quản trị viên (Admin)
Mức ưu tiên
(MoSCoW)
M – Must Have
Mô tả
Admin cập nhật Name và/hoặc Description của danh mục. Slug
KHÔNG thay đổi khi đổi tên (để tránh broken links). Sau khi cập nhật,
cache bị invalidate.
Điều kiện tiên quyết 1. Admin đang đăng nhập. 2. Danh mục với ID tương ứng tồn tại.
Luồng chính (Happy
Path)
1. Admin gửi PUT /api/v1/categories/{id} với body: { "name": "...",
"description": "..." }.
2. Kiểm tra role Admin.
3. UpdateCategoryCommand dispatch qua MediatR.
4. Tìm category theo ID, cập nhật Name và Description.
5. Lưu thay đổi, invalidate cache.
6. Trả về HTTP 200 OK với CategoryDto đã cập nhật.
Luồng thay thế /
Ngoại lệ
A1 – ID không tồn tại: HTTP 404.
A2 – Thiếu role Admin: HTTP 403.
HTTP Method &
Endpoint
PUT /api/v1/categories/{id:guid}
Kết quả mong đợi Thông tin danh mục được cập nhật. Cache invalidated.
HTTP Status Code trả
về
200 OK – Cập nhật thành công. 403 Forbidden. 404 Not Found. 422
Unprocessable Entity.
FR-CAT-005: Xóa Danh mục [Admin]
Mã yêu cầu FR-CAT-005
Tên yêu cầu Xóa Danh mục
Nhóm chức năng Module Quản lý Danh mục (FR-CAT)
Tác nhân Quản trị viên (Admin)
Culinary Blog – Tài liệu Đặc tả Yêu cầu Phần mềm (SRS) v1.0.0
CONFIDENTIAL • Phát triển Ứng dụng Web Nâng cao V4 • Trang 27 / 71
Mức ưu tiên
(MoSCoW)
S – Should Have
Mô tả
Admin xóa một danh mục. Quy tắc nghiệp vụ: KHÔNG được xóa danh
mục còn chứa công thức (dù là Published hay Draft). Admin phải
chuyển tất cả công thức sang danh mục khác trước khi xóa. Đây là soft
constraint để bảo vệ toàn vẹn dữ liệu.
Điều kiện tiên quyết
1. Admin đang đăng nhập. 2. Danh mục tồn tại và không còn công thức
nào.
Luồng chính (Happy
Path)
1. Admin gửi DELETE /api/v1/categories/{id}.
2. Kiểm tra role Admin.
3. DeleteCategoryCommand dispatch.
4. Đếm số recipe trong category: nếu > 0 → Throw
ConflictException("Danh mục còn chứa {count} công thức.").
5. Xóa entity, lưu thay đổi, invalidate cache.
6. Trả về HTTP 204 No Content.
Luồng thay thế /
Ngoại lệ
A1 – Danh mục có recipe: HTTP 409 Conflict với thông báo số lượng
recipe.
A2 – ID không tồn tại: HTTP 404.
HTTP Method &
Endpoint
DELETE /api/v1/categories/{id:guid}
Kết quả mong đợi Danh mục bị xóa khỏi database. HTTP 204 được trả về.
HTTP Status Code trả
về
204 No Content – Xóa thành công. 403 Forbidden. 404 Not Found. 409
Conflict – Danh mục còn recipe.


3.6. Module Background Jobs (FR-JOB)
Module xử lý các tác vụ nền không đồng bộ sử dụng Hangfire. Hangfire chạy in-process trong
.NET API và sử dụng PostgreSQL làm persistent storage cho job queue. Dashboard quản lý
jobs tại /hangfire (chỉ Admin). Hỗ trợ 3 loại job: Fire-and-forget (chạy ngay), Delayed (chạy
sau N giây/phút) và Recurring (lịch cron).
Mã FR Tên Job Loại Trigger Mô tả Retry Policy
FRJOB001
Welcome
Email Job
Fire-andforget
Sau FR-AUTH-001 thành
công
(BackgroundJob.Enqueue)
Gửi email HTML
chào mừng đến
địa chỉ email vừa
đăng ký. Email
template bao gồm:
tên người dùng,
link kích hoạt email
(nếu cần), link đến
ứng dụng.
Tự động retry
3 lần với
exponential
backoff (1
phút, 5 phút,
30 phút). Sau 3
lần fail →
chuyển sang
Failed state,
log error.
FRJOB002
Image Resize
/ Thumbnail
Job
Fire-andforget
Sau FR-RCP-008 upload
ảnh thành công
Tạo thumbnail
(300x300px) và
medium image
(800x600px) từ
ảnh gốc. Lưu cả 3
phiên bản lên
MinIO. Cập nhật
URLs vào
database.
Retry 3 lần.
Nếu fail: ảnh
gốc vẫn hiển
thị, chỉ thiếu
thumbnail.
FRJOB003
Sitemap
Generation
Job
Recurring Hàng ngày lúc 02:00 AM
UTC (cron: "0 2 * * *")
Tạo file
sitemap.xml chứa
URL tất cả
Published recipes,
categories và
pages tĩnh. Upload
sitemap.xml lên
MinIO hoặc lưu
Retry 2 lần nếu
fail. Log kết
quả (số URL
trong sitemap)
qua Serilog.
Culinary Blog – Tài liệu Đặc tả Yêu cầu Phần mềm (SRS) v1.0.0
CONFIDENTIAL • Phát triển Ứng dụng Web Nâng cao V4 • Trang 39 / 71
Mã FR Tên Job Loại Trigger Mô tả Retry Policy
vào wwwroot. Gửi
thông báo đến
Google Search
Console (ping).