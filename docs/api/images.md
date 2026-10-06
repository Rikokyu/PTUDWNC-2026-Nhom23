# Recipe images

Image endpoints yêu cầu role Author hoặc Admin và tiền tố
`/api/v1/recipes/{id}/images`.

| Method | Route | Mô tả |
|---|---|---|
| POST | `/` | Upload ảnh bằng `multipart/form-data`; trường file tên `file`, có thể gửi `altText` và `isPrimary`; thành công trả `201` |
| PATCH | `/{imageId}` | Cập nhật metadata JSON `{ "altText": "...", "isPrimary": false, "orderIndex": 0 }`; trả `200` |
| PATCH | `/{imageId}/primary` | Chọn ảnh chính; trả `200` |
| DELETE | `/{imageId}` | Xóa ảnh; trả `204` |

Upload giới hạn kích thước theo `FileUpload:MaxFileSizeBytes` (mặc định 5 MiB)
và chấp nhận JPEG, PNG, WebP, AVIF; phần mở rộng và chữ ký file phải khớp
MIME type. Lỗi dữ liệu trả lỗi validation; MinIO không sẵn sàng trả
`503 Service Unavailable`.