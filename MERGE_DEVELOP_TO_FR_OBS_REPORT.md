# Bảo vệ code và kiểm tra điều kiện merge develop vào fr-obs

Ngày: 03/11/2026. **Đã sao lưu đủ 42 file; dừng trước stage/commit/merge vì phát hiện credentials ghi trực tiếp trong cấu hình incoming từ develop.**

## 1. Sao lưu và xác minh

- Nhánh hiện tại: `fr-obs`.
- HEAD giữ nguyên: `c56ef2f592b663e6af748bcc5253e080ee78e1be`.
- Thư mục backup ngoài repository: `C:\Users\PC\Desktop\PTUDWNC-2026-Nhom23-backups\fr-obs-20261103-144800`.
- `files/`: bản sao nguyên vẹn của **19 file modified + 23 file untracked**.
- `manifest.json`: danh sách file, kích thước, SHA256, HEAD và định danh hai stash.
- `working-files.zip`: chứa đủ 42 file, đã kiểm tra CRC và SHA256 từng entry so với source.
- SHA256 ZIP: `4b545e014a40644528532864fc84da5b958adab73c180c305e0d85cd937bbfb7`.
- Không copy `.env`, User Secrets, `.git`, logs, node_modules, bin/obj hoặc volumes. Không copy lịch sử Git vì lịch sử từng chứa credentials.
- Bản sao có cả runtime-state và reports để bảo toàn đủ inventory; điều này không có nghĩa tất cả các file đó nên được stage.

Không phát hiện JWT secret/MinIO access key/secret key hiện có trong User Secrets xuất hiện trong 42 file. Các assignment cần rà soát ở tests là fixture/test data; script xác minh chỉ đọc secret và không in giá trị. Đây là kiểm tra các giá trị đã biết, không phải chứng nhận tuyệt đối không có mọi loại secret.

## 2. Phân loại thay đổi local

| Nhóm | Nội dung | Dự kiến khi được phép tiếp tục |
|---|---|---|
| Backend source | Pagination, telemetry pipeline, logging correlation, user directory, MinIO instrumentation | Stage chọn lọc |
| Frontend source | Recipe create/edit/image preview, navigation, user listing, API update client | Stage chọn lọc |
| Tests | Pagination, telemetry, user directory, Playwright Lab8/UI contract | Stage sau kiểm tra fixture/test credentials |
| Docker/telemetry config | Compose bổ sung collector, collector YAML | Stage |
| Reports và bằng chứng | Các báo cáo nghiệm thu, observability guide, readiness/evidence JSON | Stage các file cần thiết sau review |
| Verification scripts | Startup API/database test, PostgreSQL tests, collector verification | Stage sau review |
| Runtime artifact | `week678-runtime-state.json` | Đã backup, không stage |
| Sensitive configuration | `.env` thật/User Secrets không thuộc 42 file và không thay đổi | Không stage/copy |

Không có merge/rebase đang dang dở. Hai stash được giữ nguyên; không tạo/apply/drop stash. Index vẫn trống, chưa stage file nào.

## 3. Điều kiện chặn an toàn

`origin/develop` hiện tại (`ff0e0e8`) có các trường sau ghi trực tiếp trong `backend/src/CulinaryBlog.API/appsettings.Development.json`:

- `ConnectionStrings:DefaultConnection`: connection string có password nhúng.
- `Jwt:Secret`: literal không rỗng, không phải placeholder rõ ràng.
- `Minio:AccessKey` và `Minio:SecretKey`: literal không rỗng, không phải placeholder rõ ràng.

Không ghi bất kỳ giá trị nào vào báo cáo/tool output này. Chưa xác định các giá trị incoming còn hiệu lực hay chỉ dùng development; không tự coi chúng là an toàn. Chúng không khớp với các secret hiện có được kiểm tra trong User Secrets. Cấu hình working tree hiện tại không có các literal incoming đó; merge develop chưa được thực hiện.

Yêu cầu người dùng quy định: **“Nếu phát hiện thay đổi ngoài phạm vi hoặc thông tin nhạy cảm, dừng và báo cáo thay vì tự ý commit.”** Giai đoạn merge còn yêu cầu **“Không có credential mới trong phần chuẩn bị tích hợp.”** Vì vậy phiên này dừng trước stage/commit/merge; không tự sửa/xóa/rotate credentials hoặc bỏ qua điều kiện an toàn.

## 4. Quan hệ các nhánh

- `origin/develop`: `ff0e0e8` theo lần fetch thành công ngay trước yêu cầu này; phiên này không fetch thêm.
- `fr-obs...origin/develop`: **10 commit riêng local / 19 commit develop chưa tích hợp**, có tính merge commits.
- `origin/fr-obs`: `0bf7b25`; so với local `fr-obs`, ahead 1 / behind 1. Không reset/rebase/merge remote feature branch để giải quyết phân kỳ.
- Merge base của `fr-obs` và `origin/develop`: `24da869`.
- 12 file modified local cũng bị develop thay đổi; đây là vùng có nguy cơ conflict, chưa phải conflict đã phát sinh.
- `frontend/src/app/(dashboard)/dashboard/users/page.tsx` đang untracked local và đã có trên develop; cần tích hợp hai implementation cẩn thận sau khi local work đã được commit.

Hai stash ban đầu giữ nguyên:

- `stash@{0}`: `before-resolve-develop-2026-10-27`.
- `stash@{1}`: `local-work-before-pull-develop-2026-10-27`.

## 5. Commit/merge và kiểm thử

| Hạng mục | Kết quả phiên này |
|---|---|
| Commit backup | Không tạo; dùng bản sao ngoài repository đã xác minh |
| Commit tuần 6/7/Lab8 | **CHƯA tạo** do điều kiện dừng an toàn |
| Commit develop tích hợp | **CHƯA** |
| File conflict thực tế | 0 — chưa bắt đầu merge |
| Cách xử lý conflict | Chưa áp dụng ours/theirs hoặc chỉnh sửa conflict nào |
| Source/credentials/database/volumes | Không thay đổi |
| Backend/Frontend build sau merge | **CHƯA KIỂM CHỨNG** — chưa merge, không chạy lại |
| PostgreSQL integration sau merge | **CHƯA KIỂM CHỨNG** |
| Playwright E2E sau merge | **CHƯA KIỂM CHỨNG** |
| OpenTelemetry/MinIO/Redis sau merge | **CHƯA KIỂM CHỨNG** |
| Kết quả lịch sử | 26/26 PostgreSQL PASS; 3 E2E + 1 UI contract PASS; services và frontend build/typecheck PASS — không tính là lần kiểm thử mới |
| Git Security | **CHƯA ĐẠT**: incoming literals cần review và credential lịch sử chưa xác nhận thu hồi |

Không commit/push hoặc viết lại history. Các commit chưa push trước phiên này không thay đổi: local `c56ef2f` chưa nằm trên `origin/fr-obs`; các thay đổi tuần 6/7/Lab8 vẫn nằm trong working tree và nay có thêm bản sao ngoài repository. Báo cáo này là file mới, chưa commit và được tạo sau snapshot 42 file.

## 6. Kết luận và bước tiếp theo

- Đã cập nhật develop: **CHƯA**.
- Code tuần 6/7/Lab8 đã được bảo toàn: **CÓ** — working tree giữ nguyên và 42/42 file đã backup/đối chiếu hash; **chưa được commit vào Git**.
- Build/Tests sau merge: **CHƯA KIỂM CHỨNG**, không gán PASS/FAIL khi chưa chạy.
- Có conflict còn lại: **KHÔNG**, vì chưa bắt đầu merge; nguy cơ conflict vẫn tồn tại.
- Git Security: **CHƯA ĐẠT**.
- Sẵn sàng push GitHub: **CHƯA**.

Cần người dùng/nhóm quyết định xử lý các literal nhạy cảm incoming trước khi cho phép tiếp tục. Phương án có thể review là giữ cấu hình sạch hiện tại, lấy secrets qua environment/User Secrets và tích hợp riêng các thay đổi chức năng mới; cách đó không rotate credentials dịch vụ, nhưng chưa được tự thực hiện vì yêu cầu đã quy định dừng khi phát hiện thông tin nhạy cảm. Việc thu hồi credential lịch sử vẫn cần chủ sở hữu xác nhận riêng.
