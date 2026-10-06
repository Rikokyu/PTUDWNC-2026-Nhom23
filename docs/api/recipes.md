# Recipes

Base route: `/api/v1/recipes`. Recipe request/response dùng JSON; enum difficulty
được serialize dưới dạng chuỗi (`Easy`, `Medium`, `Hard`, `Expert`).

## Đọc và tìm kiếm

| Method | Route | Tham số chính | Kết quả |
|---|---|---|---|
| GET | `/` | `page`, `pageSize`, `categoryId`, `difficulty`, `maxCookTime`, `sort` | `PagedResult<RecipeSummaryDto>` |
| GET | `/search` | `q`, `page`, `pageSize`, `sort` | `PagedResult<RecipeSummaryDto>` |
| GET | `/{slug}` | slug | `RecipeDetailDto` |

`page` mặc định là 1, `pageSize` của danh sách mặc định 12, tìm kiếm mặc định
10; cả hai endpoint giới hạn `pageSize` tối đa 50. Các sort được hỗ trợ gồm
`createdAt`, `title`, `cookTime`, `prepTime`, `categoryName`, thêm dấu `-` để
sắp xếp giảm dần. Có thể dùng thay thế `sortBy` và `sortOrder=asc|desc`.
Danh sách recipes còn hỗ trợ lọc theo category, difficulty và thời gian nấu tối
đa.

## Quản lý

Các route ghi dữ liệu yêu cầu role Author hoặc Admin. JSON tạo recipe gồm
`title`, `description`, `categoryId`, `prepTimeMinutes`, `cookTimeMinutes`,
`servings`, `difficulty`, và tùy chọn `nutrition`, `ingredients`, `steps`.
Nutrition có `calories`, `protein`, `carbs`, `fat`, `fiber`, `sodium`. Ingredient
có `name`, `quantity`, `unit`, `notes`, `orderIndex`; step có `title`,
`description`, `timerMinutes`, `imageUrl`.

| Method | Route | Mô tả |
|---|---|---|
| POST | `/` | Tạo recipe, trả `201` và recipe detail |
| PUT | `/{id}` | Cập nhật thông tin recipe và nutrition |
| PATCH | `/{id}/publish` | Publish recipe |
| PATCH | `/{id}/unpublish` | Chuyển về Draft |
| PATCH | `/{id}/archive` | Archive recipe |
| DELETE | `/{id}` | Xóa mềm, trả `204` |

Publish/unpublish/archive trả recipe đã cập nhật. Các thay đổi recipe, ảnh,
steps và ingredients invalidate output cache liên quan tới recipes.

Thao tác child resources:

- [Ingredients](./ingredients.md)
- [Steps](./steps.md)
- [Images](./images.md)