# Health checks

Các health endpoints không yêu cầu xác thực.

| Method | Route | Kiểm tra | Phản hồi |
|---|---|---|---|
| GET | `/health/live` | Process đang hoạt động (`self`) | `200` khi process còn phục vụ request |
| GET | `/health/ready` | PostgreSQL, Redis và MinIO | `200` khi tất cả healthy; `503` nếu có dependency không healthy |
| GET | `/health` | Tất cả checks, gồm self và dependencies | `200` khi tất cả healthy; `503` nếu có check không healthy |

Readiness kiểm tra kết nối PostgreSQL, gửi Redis `PING`, và gọi MinIO
`/minio/health/ready`. Cấu hình cần thiết:

- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:Redis`
- `Minio:Endpoint`; `Minio:UseSSL` xác định HTTP/HTTPS

`/health` và `/health/ready` trả JSON gồm `status` và trạng thái từng check.
`/health/live` chỉ trả `status`, không phụ thuộc tình trạng database hay object
storage. Timeout cho mỗi dependency check là 3 giây.