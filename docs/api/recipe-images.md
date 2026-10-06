# Recipe Images API — Upload ảnh FR-RCP-008

## POST /api/v1/recipes/{id}/images

Upload ảnh cho recipe bằng `multipart/form-data`. Chỉ Author sở hữu recipe hoặc Admin được phép thực hiện.

Các field:

- `file`: bắt buộc; JPEG, PNG, WebP hoặc AVIF; tối đa 5 MB.
- `altText`: tùy chọn, tối đa 200 ký tự.
- `isPrimary`: tùy chọn. Ảnh đầu tiên luôn trở thành ảnh chính.

Ví dụ:

```http
POST /api/v1/recipes/{recipeId}/images
Authorization: Bearer <access-token>
Content-Type: multipart/form-data

file=<binary image>
altText=Món ăn sau khi hoàn thành
isPrimary=true
```

Response `201 Created`:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "originalUrl": "/uploads/recipes/{recipeId}/image.png",
  "mediumUrl": null,
  "thumbnailUrl": null,
  "altText": "Món ăn sau khi hoàn thành",
  "isPrimary": true,
  "orderIndex": 1
}
```

Backend kiểm tra cả MIME type và magic bytes, không tin phần mở rộng do client gửi. File hiện được lưu qua `IFileStorageService` với provider local để môi trường phát triển có thể chạy ngay.

Status code: `201`, `400` file sai, `401` chưa đăng nhập, `403` không phải owner/Admin, `404` recipe không tồn tại.

> `mediumUrl` và `thumbnailUrl` có thể còn null trong response `201` vì FR-JOB-002 chạy bất đồng bộ; job cập nhật hai URL này sau khi hoàn tất. Job được enqueue nội bộ sau upload, không có API riêng cho frontend gọi. Trong Development, bật `Hangfire:Enabled=true` khi PostgreSQL hoạt động; `Hangfire:DashboardEnabled=true` mở `/hangfire` cho Admin. File storage hiện dùng local filesystem; chuyển sang MinIO thuộc FR-FILE.
