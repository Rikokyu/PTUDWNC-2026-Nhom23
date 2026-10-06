# Categories

Base route: `/api/v1/categories`.

| Method | Route | Quyền | Thành công |
|---|---|---|---|
| GET | `/` | Công khai | `200` danh sách `CategoryDto` |
| GET | `/{slug}?page=1&pageSize=12` | Công khai | `200` danh mục và trang recipes |
| POST | `/` | Admin | `201` danh mục mới |
| PUT | `/{id}` | Admin | `200` danh mục đã cập nhật |
| DELETE | `/{id}` | Admin | `204` |

Create và update nhận JSON với các trường `name`, `description`, `imageUrl`,
`orderIndex`. `name` là bắt buộc; các trường còn lại tùy chọn/giá trị mặc định
theo request model. `id` là GUID, `slug` là chuỗi URL.

`page` và `pageSize` của endpoint chi tiết phải là số nguyên dương; mặc định lần
lượt là 1 và 12, `pageSize` tối đa 50. Xóa danh mục đang được công thức sử dụng
có thể trả `409 Conflict`. Các route quản trị cần Bearer token có role `Admin`.