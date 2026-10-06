# Register API — FR-AUTH-001

## POST /api/v1/auth/register

Tạo tài khoản Author mới, hash mật khẩu bằng PBKDF2, lưu refresh token dưới dạng SHA-256 và trả access token để đăng nhập ngay.

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "fullName": "Nguyễn Văn Test",
  "email": "test@example.com",
  "userName": "nguyenvantest",
  "password": "MatKhau@123"
}
```

Response `201 Created`:

```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<refresh-token>",
  "expiresAt": "2026-10-06T13:45:00Z",
  "user": {
    "id": "00000000-0000-0000-0000-000000000000",
    "fullName": "Nguyễn Văn Test",
    "email": "test@example.com",
    "userName": "nguyenvantest",
    "avatarUrl": null,
    "roles": ["Author"]
  }
}
```

Quy tắc validation:

- `fullName`: 2–100 ký tự, không chứa HTML.
- `email`: email hợp lệ, chưa tồn tại.
- `userName`: 3–50 ký tự, chỉ gồm chữ, số và dấu gạch dưới.
- `password`: tối thiểu 8 ký tự, có chữ hoa, chữ số và ký tự đặc biệt.

Status code: `201` thành công, `409` trùng email/userName, `422` dữ liệu không hợp lệ.

Secret ký JWT phải được cung cấp qua `Jwt__Secret` và dài tối thiểu 32 byte.

> Điểm nối `WelcomeEmailJob` sẽ được enqueue tại luồng đăng ký khi FR-JOB-001 được triển khai. API đăng ký hiện không giả lập việc gửi email.
