# Categories API — FR-CAT-001 đến FR-CAT-005

Base path: `/api/v1/categories`.

API đọc không cần đăng nhập. API tạo, cập nhật và xóa yêu cầu JWT Bearer có claim `role: Admin`. Khi chạy local, cấu hình secret bằng biến môi trường `Jwt__Secret`; không commit secret vào `appsettings.json`.

## GET /api/v1/categories

Trả tất cả danh mục chưa bị xóa mềm, sắp xếp `name` tăng dần. `recipeCount` chỉ đếm công thức `Published`. Kết quả được cache với key `categories:all`, sliding expiration 60 phút.

```http
GET /api/v1/categories
```

```json
[
  {
    "id": "da4d6789-72c6-40ca-af25-140fe8b4a274",
    "name": "Món chính",
    "slug": "mon-chinh",
    "description": "Các món dùng trong bữa chính",
    "imageUrl": null,
    "orderIndex": 1,
    "recipeCount": 12
  }
]
```

## GET /api/v1/categories/{slug}

Trả thông tin category và recipe phân trang. `page` mặc định 1; `pageSize` mặc định 12, tối đa 50. Guest và Admin chỉ thấy recipe Published; Author thấy recipe Published và Draft do chính mình tạo. Admin không được xem Draft chỉ nhờ quyền Admin.

```http
GET /api/v1/categories/mon-chinh?page=1&pageSize=12
```

Response `200`:

```json
{
  "category": {
    "id": "da4d6789-72c6-40ca-af25-140fe8b4a274",
    "name": "Món chính",
    "slug": "mon-chinh",
    "description": "Các món dùng trong bữa chính",
    "imageUrl": null,
    "orderIndex": 1,
    "recipeCount": 12
  },
  "recipes": {
    "items": [],
    "totalCount": 0,
    "page": 1,
    "pageSize": 12,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Slug không tồn tại trả RFC 7807 `404`. Phân trang không hợp lệ trả `422`.

## POST /api/v1/categories

Yêu cầu Admin. `name` dài 2–50 ký tự và không chứa HTML. Slug được sinh tự động, bỏ dấu tiếng Việt; nếu trùng sẽ thêm `-2`, `-3`,... kể cả slug thuộc category đã xóa mềm.

```http
POST /api/v1/categories
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Món chính",
  "description": "Các món dùng trong bữa chính",
  "imageUrl": "https://example.com/categories/mon-chinh.jpg",
  "orderIndex": 1
}
```

Thành công trả `201 Created`, body là CategoryDto và header:

```http
Location: /api/v1/categories/mon-chinh
```

Tên đã tồn tại trả `409`; validation trả `422`; thiếu/không đúng quyền trả `401/403`.

## PUT /api/v1/categories/{id}

Yêu cầu Admin. Cho phép cập nhật `name`, `description`, `imageUrl`, `orderIndex`; phải gửi ít nhất một field. Slug không thay đổi khi đổi tên. Gửi chuỗi rỗng cho description/imageUrl để xóa giá trị hiện tại.

```http
PUT /api/v1/categories/da4d6789-72c6-40ca-af25-140fe8b4a274
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Món ăn chính",
  "description": "Mô tả mới",
  "orderIndex": 2
}
```

Thành công trả `200`; không tìm thấy trả `404`; tên trùng trả `409`; validation trả `422`.

## DELETE /api/v1/categories/{id}

Yêu cầu Admin. Chỉ xóa mềm khi category không còn recipe. Nếu còn recipe ở bất kỳ trạng thái nào, trả `409` kèm số lượng. Thành công trả `204 No Content`; category sau đó bị loại khỏi các query mặc định nhưng name/slug vẫn được giữ để tránh tái sử dụng.

```http
DELETE /api/v1/categories/da4d6789-72c6-40ca-af25-140fe8b4a274
Authorization: Bearer <admin-jwt>
```

## Cache và lỗi

POST/PUT/DELETE chỉ invalidate cache sau khi `SaveChangesAsync` thành công. Lỗi API dùng RFC 7807 Problem Details. Validation là `422`, không tìm thấy `404`, xung đột dữ liệu `409`, chưa đăng nhập `401`, không có role Admin `403`.
