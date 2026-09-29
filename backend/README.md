# Culinary Blog Backend

## Chạy local

Backend cần .NET 10 SDK và PostgreSQL 16. Database mặc định có tên
`culinary_blog` trên `localhost:5432`.

Không ghi mật khẩu thật vào `appsettings*.json`. Cấu hình mật khẩu local bằng
.NET User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=culinary_blog;Username=postgres;Password=MAT_KHAU_POSTGRES_CUA_BAN" --project .\src\CulinaryBlog.API\CulinaryBlog.API.csproj
```

Khôi phục package, build và chạy API:

```powershell
dotnet restore .\CulinaryBlog.slnx
dotnet build .\CulinaryBlog.slnx --no-restore
dotnet run --project .\src\CulinaryBlog.API\CulinaryBlog.API.csproj --no-build --no-launch-profile --urls http://localhost:5080
```

Khi khởi động, API tự áp dụng EF Core migrations và tạo dữ liệu mẫu nếu
database đang trống.

Swagger: `http://localhost:5080/swagger`

## API public hiện có

- `GET /health/live`
- `GET /health/ready`
- `GET /health`
- `GET /api/v1/categories`
- `GET /api/v1/categories/{slug}?page=1&pageSize=12`
- `GET /api/v1/recipes?page=1&pageSize=12`
- `GET /api/v1/recipes/{slug}`
- `GET /api/v1/recipes/search?q=...&page=1&pageSize=12`

Danh sách Recipe hỗ trợ:

- `categoryId`
- `difficulty=Easy|Medium|Hard|Expert`
- `maxCookTime`
- `minServings`
- `search`
- `sortBy=title|createdAt|cookTimeMinutes`
- `sortOrder=asc|desc`

Các endpoint ghi dữ liệu chưa được mở trong vertical slice này. Chúng phải được
kết hợp với Authentication/Authorization trước khi đưa vào sử dụng để tránh cho
phép người dùng ẩn danh tạo, sửa hoặc xóa dữ liệu.
