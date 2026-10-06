# Authentication

Base route: `/api/v1/auth`. Các token được trả về bởi login, Google login và
refresh có dạng `{ accessToken, refreshToken, expiresIn }` (`expiresIn` tính
bằng giây).

| Method | Route | Body | Thành công |
|---|---|---|---|
| POST | `/register` | `{ "email", "displayName", "password" }` | `201` `{ userId, email, displayName }` |
| POST | `/login` | `{ "email", "password" }` | `200` token response |
| POST | `/google` | `{ "idToken" }` | `200` token response |
| POST | `/refresh` | `{ "refreshToken" }` | `200` token response; refresh token được xoay vòng |
| POST | `/logout` | `{ "refreshToken" }` | `204`; yêu cầu Bearer token |
| GET | `/me` | — | `200` hồ sơ hiện tại |
| PATCH | `/me` | Một hoặc nhiều trường `displayName`, `avatarUrl`, `bio` | `200` hồ sơ đã cập nhật |

## Validation và lỗi thường gặp

- Register yêu cầu email hợp lệ, `displayName` từ 2 đến 100 ký tự, password
  từ 8 đến 128 ký tự. Email đã tồn tại trả `409`.
- Login bị giới hạn 10 lần/phút theo địa chỉ IP; vượt hạn mức trả `429`.
- Google login cần cấu hình `Google:ClientId` và ID token hợp lệ.
- Refresh token đã hết hạn/không hợp lệ trả `401`. Refresh token đã dùng lại sẽ
  thu hồi các refresh token còn hoạt động của tài khoản.
- PATCH `/me` cần ít nhất một trường; avatar URL chỉ nhận HTTP/HTTPS; bio tối đa
  1000 ký tự.
- Các endpoint `/logout`, `/me` yêu cầu header
  `Authorization: Bearer <accessToken>`.