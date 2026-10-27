# Git Security — 27/10/2026

## Phạm vi và kết quả

Đã sửa cấu hình working tree, không commit/push, không rewrite history, không đổi credential thật, không cập nhật/xóa/reset database hoặc Docker volumes. Không sửa `.env` thật hoặc cấu hình container đang chạy. Không chạy lại toàn bộ tests/frontend build.

| Hạng mục | Kết luận |
|---|---|
| appsettings hiện tại trong working tree | PASS |
| .env.example | PASS |
| .gitignore | PASS |
| Backend Configuration (Development) | PASS |
| Backend Build | PASS, 0 errors |
| PostgreSQL SELECT 1 | PASS, chỉ đọc |
| docker compose config --quiet | PASS |
| git diff --check | PASS |
| Credential lịch sử | CHƯA XÁC MINH thu hồi |
| Git Security tổng thể | CHƯA ĐẠT |
| Sẵn sàng push GitHub | CHƯA |

## File sửa trong lượt này

- `backend/src/CulinaryBlog.API/appsettings.json`: để trống DefaultConnection; giữ các cấu hình không nhạy cảm.
- `backend/src/CulinaryBlog.API/appsettings.Development.json`: để trống DefaultConnection và bỏ JWT secret hardcode; giữ Redis và các thiết lập không nhạy cảm.
- `.env.example`: thay MinIO access/secret key, password connection string và các secret mẫu bằng placeholder `REPLACE_WITH_*`; thêm biến POSTGRES và hướng dẫn production dùng credential riêng.
- `docker-compose.yml`: PostgreSQL user/password lấy từ `.env` hoặc environment, không có password mặc định; healthcheck lấy user/database của container bằng cú pháp `$$` để tránh Compose nội suy sớm. Chỉ kiểm tra cấu hình; không chạy compose up/down và không thay container.
- `.gitignore`: thêm `**/test-results/` và `**/playwright-report/` để cả metadata test nằm ngoài frontend cũng được ignore.
- `docs/verification/GIT_SECURITY_2026-10-27.md`: báo cáo này.
- `docs/verification/resume-file-inventory.txt`: cập nhật danh sách thay đổi/mới hiện tại, gồm cả chính file inventory và công việc trước đó.

Đã tạo helper kiểm tra cục bộ trong `scripts/local/security-check/`, bị ignore theo quy tắc đã có, không thuộc phần commit. Helper không chứa secrets; nó chỉ xác minh nguồn cấu hình, độ dài JWT secret và chạy SELECT 1.

## Nguồn cấu hình an toàn

API đã có `UserSecretsId` là `CulinaryBlog-API-Development`. Kiểm tra ngoài sandbox và bổ sung các khóa còn thiếu từ `.env` cục bộ vào User Secrets ngoài repository, bảo toàn các giá trị đã tồn tại. Không sinh hay đổi password/key vận hành.

Kiểm chứng sau sửa bằng cấu hình appsettings → appsettings.Development → User Secrets → environment:

```text
ConnectionStrings:DefaultConnection: configured=True; source=secrets.json
Jwt:Secret: configured=True; source=secrets.json
PostgreSQL read-only connection check: PASS
```

`WebApplication.CreateBuilder` của API hiện có tự nạp User Secrets trong Development theo UserSecretsId. `.env` không được ASP.NET Core tự nạp. Các tiến trình test đang chạy từ trước vẫn dùng environment riêng của chúng; không dùng các tiến trình đó làm bằng chứng cho cấu hình mới. Kiểm chứng SELECT 1 ở trên dùng cấu hình mới nạp từ file và User Secrets, không chạy seeder/migration.

Để chạy local trên máy này, dùng launch profile Development hiện có hoặc đặt `ASPNETCORE_ENVIRONMENT=Development`, rồi chạy API. Khi clone trên máy khác, thiết lập User Secrets của riêng máy đó trước khi chạy. Không sao chép credential vào appsettings.

Production phải cung cấp `ConnectionStrings__DefaultConnection` và `Jwt__Secret` qua environment/secret manager; nếu dùng MinIO, cung cấp thêm `Minio__AccessKey`, `Minio__SecretKey`. Production không tự nạp Development User Secrets. Các placeholder trong `.env.example` phải được thay bằng giá trị riêng trong `.env` không tracked hoặc secret manager. JWT secret phải đáp ứng tối thiểu 32 byte UTF-8 theo validation hiện có.

Docker Compose đọc `.env` cục bộ để nội suy cấu hình, nhưng không tự đưa mọi biến `.env` vào tiến trình API chạy bằng dotnet. Không chạy Compose PostgreSQL mới lên cổng 5433 khi container PostgreSQL hiện có đang chiếm cổng. Thay đổi biến POSTGRES trong Compose không phải thao tác rotate password của database đã khởi tạo.

## Kiểm tra file hiện tại

Hai appsettings tracked hiện không còn password connection string hoặc secret literal. Các trường password/key/secret của `.env.example` và `frontend/.env.example` là placeholder hoặc rỗng; các URL, tên database, timeout và cấu hình không nhạy cảm vẫn được giữ. Source runtime không bổ sung credential hardcode.

Đã kiểm tra ignore cho `.env`, `.env.local`, `.env.development.local`, `.env.production.local`, `.env.*.local` trong thư mục con, logs, test outputs và Next cache. Các file mẫu `.env.example`, `frontend/.env.example`, Compose và source vẫn được theo dõi/không bị ignore. User Secrets nằm ngoài repository. Không stage file trong lượt này.

Backend build `dotnet build backend/CulinaryBlog.slnx --no-restore` PASS với 0 errors. Các cảnh báo dependency Pkcs và ImageSharp license đã có từ trước vẫn còn; không nâng dependency trong nhiệm vụ này.

## Lịch sử Git — chưa giải quyết bằng việc sửa working tree

Đã xác minh commit `87ec472531664609c6644430cc28eb2433fcb5ab` chứa password-bearing connection string trong:

- `backend/src/CulinaryBlog.API/appsettings.json`
- `backend/src/CulinaryBlog.API/appsettings.Development.json`

Commit vẫn reachable từ các local remote-tracking refs `origin/main` và `origin/Lab02_2312610_NguyenTrungHiep`. Đây là bằng chứng từ Git local; không fetch hoặc suy đoán trạng thái server mới hơn. Không hiển thị giá trị secret. HEAD/index cũ vẫn còn cấu hình trước sửa vì chưa commit; các giá trị đó cũng vẫn nằm trong lịch sử.

**Credential PostgreSQL lịch sử: CHƯA XÁC MINH đã thu hồi.** Không có bằng chứng từ chủ database xác nhận rotate/revoke. Việc chuyển cấu hình sang User Secrets và thay placeholder không thu hồi credential đã lộ. Tương tự, việc loại các giá trị MinIO khỏi template không chứng minh key cũ đã vô hiệu nếu chúng từng là key thật.

Phối hợp trưởng nhóm và chủ PostgreSQL qua kênh riêng:

1. Cung cấp commit ID và đường dẫn file, không gửi password trong issue/report công khai; xác định credential từng áp dụng cho môi trường/user nào.
2. Chủ database thực hiện rotate password hoặc thu hồi quyền/role theo kế hoạch đã thống nhất, đồng thời cập nhật secret manager/biến môi trường/User Secrets của các dịch vụ liên quan. Cân nhắc kết thúc phiên cũ nếu cần thu hồi truy cập đang tồn tại, do chủ database quyết định.
3. Xác nhận credential cũ không còn xác thực được và dịch vụ vẫn kết nối bằng credential mới; lưu xác nhận không chứa secrets. Với MinIO, chủ hệ thống xác nhận key cũ là ví dụ hoặc rotate/revoke nếu là key thật.
4. Sau xác nhận mới đánh giá lại Git Security và khả năng push. Nếu nhóm yêu cầu dọn lịch sử, maintainer lập kế hoạch riêng với các thành viên; lượt này không rewrite history/force push.

Các báo cáo cũ mô tả thời điểm trước đó. Báo cáo này là kết luận hiện tại cho phần cấu hình và Git Security; không thay kết quả kiểm thử chức năng đã có.
