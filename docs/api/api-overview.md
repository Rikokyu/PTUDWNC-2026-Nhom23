# Culinary Blog API

REST API dùng ASP.NET Core Minimal APIs. Các endpoint nghiệp vụ dùng tiền tố
`/api/v1`; health checks nằm ở `/health`.

## Nhóm endpoint

| Nhóm | Tài liệu | Mô tả |
|---|---|---|
| Authentication | [authentication.md](./authentication.md) | Đăng ký, đăng nhập, token và hồ sơ |
| Categories | [categories.md](./categories.md) | Danh mục công thức |
| Recipes | [recipes.md](./recipes.md) | Danh sách, tìm kiếm và quản lý công thức |
| Ingredients | [ingredients.md](./ingredients.md) | Nguyên liệu của công thức |
| Steps | [steps.md](./steps.md) | Các bước nấu |
| Images | [images.md](./images.md) | Ảnh của công thức |
| Health | [health-check.md](./health-check.md) | Liveness và trạng thái dịch vụ phụ thuộc |

## Danh sách route

| Method | Route | Quyền |
|---|---|---|
| POST | `/api/v1/auth/register` | Công khai |
| POST | `/api/v1/auth/login` | Công khai, giới hạn tốc độ |
| POST | `/api/v1/auth/google` | Công khai, giới hạn tốc độ |
| POST | `/api/v1/auth/refresh` | Công khai |
| POST | `/api/v1/auth/logout` | Đăng nhập |
| GET | `/api/v1/auth/me` | Đăng nhập |
| PATCH | `/api/v1/auth/me` | Đăng nhập |
| GET | `/api/v1/categories` | Công khai |
| GET | `/api/v1/categories/{slug}` | Công khai |
| POST | `/api/v1/categories` | Admin |
| PUT | `/api/v1/categories/{id}` | Admin |
| DELETE | `/api/v1/categories/{id}` | Admin |
| GET | `/api/v1/recipes` | Công khai |
| GET | `/api/v1/recipes/search` | Công khai |
| GET | `/api/v1/recipes/{slug}` | Công khai; quyền xem draft phụ thuộc chính sách |
| POST | `/api/v1/recipes` | Author hoặc Admin |
| PUT | `/api/v1/recipes/{id}` | Author hoặc Admin |
| PATCH | `/api/v1/recipes/{id}/publish` | Author hoặc Admin |
| PATCH | `/api/v1/recipes/{id}/unpublish` | Author hoặc Admin |
| PATCH | `/api/v1/recipes/{id}/archive` | Author hoặc Admin |
| DELETE | `/api/v1/recipes/{id}` | Author hoặc Admin |
| POST | `/api/v1/recipes/{id}/ingredients` | Author hoặc Admin |
| PUT | `/api/v1/recipes/{id}/ingredients/{ingredientId}` | Author hoặc Admin |
| DELETE | `/api/v1/recipes/{id}/ingredients/{ingredientId}` | Author hoặc Admin |
| POST | `/api/v1/recipes/{id}/steps` | Author hoặc Admin |
| PUT | `/api/v1/recipes/{id}/steps/{stepId}` | Author hoặc Admin |
| DELETE | `/api/v1/recipes/{id}/steps/{stepId}` | Author hoặc Admin |
| POST | `/api/v1/recipes/{id}/images` | Author hoặc Admin |
| PATCH | `/api/v1/recipes/{id}/images/{imageId}` | Author hoặc Admin |
| PATCH | `/api/v1/recipes/{id}/images/{imageId}/primary` | Author hoặc Admin |
| DELETE | `/api/v1/recipes/{id}/images/{imageId}` | Author hoặc Admin |
| GET | `/health` | Công khai |
| GET | `/health/live` | Công khai |
| GET | `/health/ready` | Công khai |

Endpoints yêu cầu đăng nhập dùng header `Authorization: Bearer <accessToken>`.
Request và response lỗi sử dụng JSON; lỗi validation và lỗi nghiệp vụ được ánh
xạ bởi middleware toàn cục. Swagger được bật trong môi trường Development.