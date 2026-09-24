## Thông tin sinh viên

|MSSV      |Họ tên sinh viên        |Mail cá nhân           |Github cá nhân                  |Số điện thoại   |
|---|---|---|---|---|
|2312610   |Nguyễn Trung Hiệp       |2312610@dlu.edu.vn     |https://github.com/Rikokyu      |0358756475      |
|2312569   |Lê Thị Mai Anh          |2312569@dlu.edu.vn     |https://github.com/maianhtl     |0941617043      |
|2312789   |K'Nguyễn Quang Trường   |2312789@dlu.edu.vn     |https://github.com/2312789-rgb  |0365630966      |
|2312622   |Nguyễn Đức Hoàng        |2312622@dlu.edu.vn     |https://github.com/Sunz9420     |0916152901      |

## Phân công công việc

|MSSV        |Mã module                |Tên module                                              |Ghi chú       |
|---|---|---|---|
|2312610     |FR-RCP                   |Quản lý Công thức Nấu ăn                                |10 chức năng  |
|2312569     |FR-JOB/FR-CAT            |Background Jobs/Quản lý Danh mục                        |9 chức năng   |
|2312789     |FR-SRCH/FR-FILE/FR-OBS   |Tìm kiếm & Phân trang/Quản lý Tệp tin/Quan sát Hệ thống |9 chức năng   |
|2312622     |FR-AUTH                  |Xác thực & Quản lý Người dùng                           |7 chức năng   |

## Chi tiết phân công

|MSSV        |Chi tiết công việc       |Tuần thực hiện         |Tiến độ                                             |Kết quả (Hoàn thành || Không hoàn thành)       |
|---|---|---|---|
|2312610     |Xem Danh sách Công thức
<br> Xem Chi tiết Công thức
<br> Tạo Công thức Nấu ăn Mới
<br> Cập nhật Công thức
<br> Xuất bản / Hủy Xuất bản Công thức
<br> Lưu trữ Công thức (Archive)
<br> Xóa Công thức
<br> Quản lý Ảnh Công thức
<br> Quản lý Nguyên liệu (CRUD)
<br> Quản lý Các bước Thực hiện (CRUD)          |          |                      |  |
|2312569     |Welcome Email Job
<br>Image Resize / Thumbnail Job
<br>Sitemap Generation Job
<br>Xem Danh sách Danh mục
<br>Xem Chi tiết Danh mục & Công thức
<br>Tạo Danh mục Mới [Admin]
<br>Cập nhật Danh mục [Admin]
<br>Xóa Danh mục [Admin]            |               |         |   |
|2312789     |Tìm kiếm Toàn văn bản (FTS)
<br>Bộ lọc Công thức nâng cao
<br>Sắp xếp Kết quả
<br>Phân trang Offset-based
<br>Upload File lên MinIO
<br>Xóa File khỏi MinIO
<br>Health Check Endpoints
<br>Structured Logging
<br>Distributed Tracing & Metrics   | |   |  |
|2312622     |Đăng ký Tài khoản
<br>Đăng nhập Email/Mật khẩu
<br>Đăng nhập Google OAuth 2.0
<br>Làm mới Access Token
<br>Đăng xuất
<br>Xem Hồ sơ Cá nhân
<br>Cập nhật Hồ sơ Cá nhân  |                          |   |   |

## Nhiệm vụ cho tuần từ ngày 16 - 22/9/2026

2312610	Tạo giao diện, database mẫu cho công thức nấu ăn (FR-RCP)

2312569 Tạo giao diện, database mẫu cho danh mục phía admin (FR-CAT)

2312789 Tạo giao diện, database mẫu cho tìm kiếm, phân trang (FR-SRCH)

2312622 Tạo giao diện, database dăng nhập/đăng ký/Xem hồ sơ cá nhân (FR-AUTH)

# Culinary Blog — Blog Ẩm thực và Nấu ăn

**Dự án môn Phát triển Ứng dụng Web Nâng cao — Nhóm 23, gồm 4 thành viên.**

Đây là bản README với các yêu cầu đã được thống nhất lại từ PDF và 5 điểm giảng viên nêu trong file TXT. Các phương án dưới đây được chọn theo tiêu chí: dễ sử dụng, phù hợp nhóm 4 người, giữ dữ liệu nhất quán và thuận tiện kiểm thử.

**Trạng thái:** tài liệu để triển khai; chưa phải danh sách chức năng đã hoàn thành. Khi các mô tả cũ khác nhau, nhóm sử dụng quy tắc cụ thể trong bản này làm căn cứ cho phần chức năng tương ứng.

## Các phương án được lựa chọn

| Điểm cần thống nhất | Phương án áp dụng | Lý do chọn và đánh đổi |
| --- | --- | --- |
| Xóa công thức, danh mục | Xóa mềm bằng `IsDeleted` | Giữ dữ liệu khi xóa nhầm; phải lọc dữ liệu đã xóa và tiếp tục lưu ảnh |
| Sắp xếp danh sách | `sortBy` chỉ tên trường, `sortOrder` là `asc` hoặc `desc` | Rõ nghĩa, dễ làm giao diện và kiểm tra đầu vào; URL dài hơn cách dùng dấu trừ |
| Dinh dưỡng | Gửi `nutrition` kèm yêu cầu tạo/cập nhật công thức | Một lần lưu thống nhất, ít API; biểu mẫu phải gom dữ liệu trước khi gửi |
| Thứ tự bước nấu | Backend tự gán `stepNumber` | Người dùng không nhập số bước; backend phải đánh lại số khi xóa và xử lý thao tác đồng thời |
| Số lượng và đơn vị nguyên liệu | `quantity` là số thập phân; `unit` là chuỗi | Vừa tính toán được, vừa biểu diễn được “muỗng cà phê”; giao diện cần chuyển phân số thành số |

Các chương sau áp dụng nhất quán những lựa chọn này. Cấu trúc đi theo 8 nhóm nội dung chính của PDF, với phần mô tả được rút gọn để nhóm dễ sử dụng.

## 1. Giới thiệu và phạm vi

Culinary Blog là website giúp người dùng tìm kiếm, đọc và chia sẻ công thức nấu ăn. Mỗi công thức có thông tin món ăn, hình ảnh, nguyên liệu, các bước thực hiện, thời gian chuẩn bị/nấu, khẩu phần và thông tin dinh dưỡng nếu tác giả cung cấp.

Mục tiêu là xây dựng một ứng dụng đầy đủ từ giao diện, xử lý nghiệp vụ, lưu trữ dữ liệu đến triển khai và kiểm thử.

Phiên bản đầu gồm 7 nhóm chức năng: tài khoản, danh mục, công thức, tìm kiếm, tệp tin, công việc tự động và theo dõi hệ thống. Giữ danh sách **34 mã FR** của PDF để theo dõi công việc; các mã có thể dùng chung một chức năng hoặc API.

**Ngoài phạm vi:** bình luận, đánh giá sao, lưu món yêu thích, nhắn tin, thanh toán, thông báo thời gian thực và ứng dụng di động native.

## 2. Tổng quan và thành viên

### Người sử dụng

| Vai trò | Quyền chính |
| --- | --- |
| Khách | Xem và tìm kiếm công thức công khai, xem danh mục |
| Tác giả — Author | Có quyền của khách; tạo, sửa, đăng, ẩn và xóa công thức của mình; cập nhật hồ sơ |
| Quản trị viên — Admin | Quản lý danh mục và công thức của tất cả tác giả; theo dõi hệ thống |

### Phân công theo bảng công việc Nhóm 23

| Thành viên | Module được phân công |
| --- | --- |
| Nguyễn Trung Hiệp | Công thức nấu ăn — `FR-RCP` |
| Nguyễn Đức Hoàng | Xác thực và quản lý người dùng — `FR-AUTH` |
| Lê Thị Mai Anh | Danh mục và công việc tự động — `FR-CAT`, `FR-JOB` |
| K'Nguyễn Quang Trường | Tìm kiếm, tệp tin và theo dõi hệ thống — `FR-SRCH`, `FR-FILE`, `FR-OBS` |

Nguồn: [bảng phân công Nhóm 23](./Ph%C3%A2n%20c%C3%B4ng%20C%C3%B4ng%20vi%E1%BB%87c%20Nh%C3%B3m%2023.xlsx). Tên và module lấy theo bảng; các mô tả kỹ thuật cũ như cascade delete hoặc cache bộ nhớ được thay bằng quy tắc trong README này.

**Đề xuất phối hợp:** mỗi thành viên tham gia giao diện và kiểm thử phần chức năng phụ trách; cả nhóm thống nhất dữ liệu API, kiểm thử tích hợp, viết báo cáo và chuẩn bị demo.

## 3. Yêu cầu chức năng đã thống nhất

### 3.1. Tài khoản

- Đăng ký bằng `displayName`, `email`, `password`; backend tạo tên tài khoản nội bộ từ email. Đăng ký thành công tự đăng nhập và gán quyền Author.
- Đăng nhập bằng email/mật khẩu hoặc Google; xem và sửa `displayName`, `avatarUrl`, `bio`. Email không sửa qua API hồ sơ.
- Thống nhất response cấp phiên gồm `accessToken`, `refreshToken`, `expiresAt` theo UTC và `user`, đặt trong `data`.
- Access token có hạn 15 phút. Refresh token có hạn 7 ngày, sinh từ 64 byte ngẫu nhiên (512 bit), chỉ lưu hash SHA-256 trong database.
- Mỗi lần refresh cấp token mới, thu hồi token cũ. Dùng lại token đã thu hồi sẽ làm vô hiệu chuỗi token của cùng phiên đăng nhập; phiên trên thiết bị khác không tự động bị thu hồi.
- Logout xác minh refresh token và thu hồi phiên tương ứng, kể cả khi access token hết hạn; client xóa thông tin phiên. Không có token gửi lên là request không hợp lệ; token không còn tồn tại/đã thu hồi được xử lý như đã đăng xuất.
- Google login: Auth.js nhận callback tại frontend Next.js, chuyển `{ idToken }` cho backend xác minh trước khi cấp phiên riêng của hệ thống. Tài khoản email đã tồn tại chỉ được liên kết khi đã xác minh quyền sở hữu, không dựa vào thông tin hồ sơ do client tự khai.
- Trong phiên bản này, thao tác của Author yêu cầu đăng nhập và quyền sở hữu; không áp dụng thêm policy bắt buộc xác nhận email cho tài khoản đăng ký thường.

### 3.2. Công thức và trạng thái

| Trạng thái | Hành vi |
| --- | --- |
| `Draft` | Bản nháp, chỉ chủ sở hữu và Admin được xem |
| `Published` | Công khai, xuất hiện trong danh sách, tìm kiếm và sitemap |
| `Archived` | Lưu trữ để ẩn khỏi nội dung công khai; chủ sở hữu/Admin vẫn quản lý được |

- Công thức mới luôn là Draft. Để xuất bản, phải có ít nhất **1 nguyên liệu và 1 bước thực hiện** đang còn sử dụng.
- Bài đang Published không được xóa nguyên liệu hoặc bước cuối cùng. Muốn sửa đến mức chưa đủ nội dung, tác giả đưa bài về Draft trước.
- Cho phép `Draft → Published`, `Published → Draft`, `Draft/Published → Archived`; đưa Archived về Draft bằng thao tác hủy xuất bản rồi hoàn thiện để đăng lại. Gửi lại trạng thái hiện có không tạo thay đổi mới.
- `prepTimeMinutes > 0`, `cookTimeMinutes >= 0`, `servings` là số nguyên dương. Thời gian nấu bằng 0 nghĩa là món không cần nấu.
- Độ khó thống nhất 4 mức: `Easy`, `Medium`, `Hard`, `Expert`, áp dụng cả khi nhập và lọc.
- Slug là phần tên dễ đọc trong URL, sinh từ tiêu đề; nếu trùng thì thêm hậu tố. Slug được giữ nguyên sau khi tạo và không tái sử dụng slug của bài đã xóa mềm.
- Danh sách công khai chỉ có Published. Danh sách quản lý riêng trả các trạng thái của đúng tác giả; Admin có danh sách quản lý toàn bộ bài chưa xóa.

### 3.3. Quy tắc xóa mềm

**Áp dụng xóa mềm cho `Recipe` và `Category`:** ghi `IsDeleted = true`, không xóa hàng dữ liệu khỏi database.

1. Công thức đã xóa không xuất hiện trong danh sách công khai, danh sách quản lý thông thường, tìm kiếm, số lượng thống kê hoặc sitemap. Truy cập API theo ID/slug của bài đã xóa trả `404`.
2. Xóa công thức không xóa các bước, nguyên liệu, dinh dưỡng, bản ghi ảnh hay tệp MinIO. API thao tác dữ liệu con cũng phải kiểm tra công thức cha chưa bị xóa.
3. Không chạy job dọn ảnh chỉ vì xóa mềm công thức. Bản đầu chưa có chức năng xóa vĩnh viễn hoặc giao diện thùng rác; dữ liệu được giữ để có thể xây chức năng khôi phục sau này.
4. Chỉ xóa mềm danh mục khi không còn công thức **chưa bị xóa mềm** thuộc danh mục đó, kể cả Draft và Archived. Công thức mới không được chọn danh mục đã xóa.
5. Tên/slug danh mục vẫn giữ duy nhất kể cả bản ghi đã xóa. Nếu bổ sung khôi phục công thức sau này, phải bảo đảm danh mục còn hoạt động và đưa bài về Draft trước.
6. `Archived` và `IsDeleted` là hai việc khác nhau: bài lưu trữ còn trong màn hình quản lý; bài xóa mềm bị loại khỏi màn hình đó.

**Ngoại lệ có chủ ý:** xóa riêng một bước, nguyên liệu hoặc ảnh khỏi công thức đang tồn tại là xóa dữ liệu con đó. Xóa riêng ảnh được phép lên lịch xóa tệp không còn được tham chiếu; không áp dụng cơ chế này cho thao tác xóa mềm cả công thức. Refresh token dùng trạng thái thu hồi riêng, không áp dụng soft delete đồng loạt.

Việc lọc mặc định bằng `IsDeleted` có thể triển khai qua [Global Query Filters của EF Core](https://learn.microsoft.com/en-us/ef/core/querying/filters); các truy vấn thống kê và dữ liệu con cũng phải tuân thủ điều kiện này.

### 3.4. Tìm kiếm, sắp xếp và phân trang

- Tìm kiếm từ 2 ký tự trở lên; hỗ trợ tiếng Việt có dấu/không dấu bằng PostgreSQL Full-Text Search, `unaccent`, `pg_trgm` và GIN index.
- Lọc theo danh mục, độ khó, thời gian nấu tối đa và số khẩu phần tối thiểu; nhiều bộ lọc kết hợp bằng điều kiện “và”.
- Chỉ dùng `sortBy` và `sortOrder`. Giá trị hợp lệ của `sortBy`: `title`, `createdAt`, `cookTimeMinutes`; `sortOrder`: `asc` hoặc `desc`.
- Danh sách thông thường mặc định `createdAt` giảm dần. Tìm kiếm mặc định xếp theo độ liên quan; nếu người dùng chọn `sortBy` thì dùng thứ tự đó.
- Khi truyền `sortBy` nhưng thiếu `sortOrder`, dùng `asc`. `sortOrder` đứng riêng, tham số `sort` cũ, `sortBy=-title`, giá trị `dsc` hoặc trường không hỗ trợ đều trả `422`.
- Dùng ID làm tiêu chí phụ khi các giá trị sắp xếp bằng nhau để thứ tự phân trang ổn định. `page` mặc định 1; `pageSize` mặc định 12, tối đa 50.

```http
GET /api/v1/recipes?sortBy=title&sortOrder=asc&page=1&pageSize=12
GET /api/v1/recipes?sortBy=title&sortOrder=desc&page=1&pageSize=12
```

### 3.5. Tạo công thức kèm dinh dưỡng

**Lựa chọn:** `nutrition` là một phần của công thức, gửi trong cùng request tạo/cập nhật; không tạo API dinh dưỡng riêng.

- Khi tạo: được bỏ qua `nutrition` hoặc gửi `null` nếu chưa có thông tin. Nếu có, gửi một object chứa các giá trị đã biết; không gọi lần lượt từng API cho từng chỉ số.
- Khi cập nhật một phần công thức bằng `PATCH`: thiếu thuộc tính `nutrition` nghĩa là giữ nguyên; `nutrition: null` nghĩa là xóa toàn bộ thông tin dinh dưỡng; object `nutrition` thay thế cả nhóm, các chỉ số bị bỏ qua trong object mới trở thành `null`.
- Đơn vị theo **mỗi khẩu phần**: calories tính bằng kcal; protein, carbohydrates, fat, fiber tính bằng gram; sodium tính bằng mg. Giá trị có nhập phải hữu hạn, không âm và phù hợp `decimal(8,2)`.
- Giá trị chưa biết lưu `null`, không tự điền 0 và không tự suy ra dinh dưỡng từ danh sách nguyên liệu. Đổi khẩu phần không tự tính lại các chỉ số đã nhập.
- Nutrition lưu trong các cột `Nutrition_*` của bảng `Recipes`, không có bảng hoặc ID riêng.
- Khi tạo kèm nguyên liệu và các bước, backend lưu công thức cùng toàn bộ dữ liệu này trong một giao dịch database: một phần không hợp lệ thì không lưu phần nào. Upload ảnh là thao tác riêng sau khi có ID công thức.

PDF đã có `nutrition?` trong luồng tạo/cập nhật (trang 29–31) và mô tả lưu cùng bảng Recipes (trang 56). Cách gửi riêng được xem là phương án đối chiếu trong góp ý; bản này chọn gửi kèm. Giao dịch giúp tránh lưu công thức thành công nhưng mất phần dữ liệu đi kèm — xem [cơ chế transactions của EF Core](https://learn.microsoft.com/en-us/ef/core/saving/transactions).

### 3.6. Hệ thống tự đánh số bước

- Client gửi `title`, `description`, `timerMinutes` và `imageUrl`; `title` được để trống, `description` bắt buộc.
- Client không gửi `stepNumber` khi tạo/sửa; nếu gửi thì API trả `422` để tránh hai nơi cùng quyết định số bước.
- Khi tạo nhiều bước cùng công thức, vị trí trong mảng quyết định thứ tự; backend gán số 1, 2, 3… Khi thêm riêng một bước, backend đặt ở cuối.
- Xóa một bước sẽ đánh lại số liên tục. Ví dụ `1, 2, 3, 4`, xóa bước 2 thì các bước còn lại được đánh số `1, 2, 3`.
- Nếu không có tiêu đề, giao diện hiển thị “Bước n” từ `stepNumber`; database cho phép `Title = null`. Dùng duy nhất tên `timerMinutes`, giá trị là số nguyên không âm hoặc `null`.
- Thêm/xóa/đánh lại số phải nằm trong cùng giao dịch và kiểm soát cập nhật đồng thời; một công thức không có hai bước cùng số. Bản đầu chưa có chức năng kéo thả đổi thứ tự.

### 3.7. Số lượng và đơn vị nguyên liệu

| Trường | Kiểu và quy tắc |
| --- | --- |
| `name` | Chuỗi từ 1–100 ký tự, bắt buộc |
| `quantity` | C# `decimal?`, PostgreSQL `numeric(10,3)`, API nhận JSON number hoặc `null` |
| `unit` | Chuỗi tối đa 50 ký tự, ví dụ `g`, `ml`, `quả`, `muỗng cà phê` |
| `notes` | Chuỗi ghi chú tối đa 500 ký tự |
| `orderIndex` | Số nguyên từ 0, thứ tự hiển thị nguyên liệu |

- **Có định lượng:** `quantity > 0`, tối đa 3 chữ số sau dấu thập phân và trong giới hạn kiểu dữ liệu; `unit` bắt buộc, không chỉ gồm khoảng trắng.
- **Không định lượng:** cả `quantity` và `unit` là `null`; `notes` bắt buộc giải thích, ví dụ “vừa đủ”. Không dùng số 0 để thay cho chưa biết lượng.
- Form tách ô số lượng và ô đơn vị. Ví dụ nhập `1/2` ở số lượng, chọn `muỗng canh` ở đơn vị; frontend chuyển số lượng thành `0.5` trước khi gửi API.
- Với phân số không biểu diễn chính xác trong 3 chữ số thập phân, form hiển thị giá trị làm tròn để người dùng kiểm tra trước khi lưu; không được làm tròn một lượng dương thành 0 để gửi đi.
- Backend chỉ nhận số đã chuẩn hóa, không nhận chuỗi `"1/2"`, `"0,5"` hoặc `"1/2 muỗng canh"` trong `quantity`. Mẫu số 0 và dữ liệu không phải số bị từ chối ở form.

| Người dùng nhập | `quantity` gửi API | `unit` gửi API | `notes` |
| --- | --- | --- | --- |
| 2 muỗng cà phê | `2` | `"muỗng cà phê"` | `null` |
| 1/2 muỗng canh | `0.5` | `"muỗng canh"` | `null` |
| 250 g | `250` | `"g"` | `null` |
| Muối vừa đủ | `null` | `null` | `"vừa đủ"` |

Chọn số thập phân thay cho lưu cả cụm dưới dạng chuỗi giúp kiểm tra lượng và hỗ trợ tính theo khẩu phần trong tương lai. [PostgreSQL `numeric`](https://www.postgresql.org/docs/16/datatype-numeric.html) lưu số thập phân chính xác trong phạm vi khai báo. API phải kiểm tra số chữ số thập phân trước khi ghi, không dựa vào việc database tự làm tròn.

### 3.8. Ảnh và công việc tự động

- Nhận JPEG, PNG, WebP, AVIF; mỗi ảnh tối đa 5 MB; kiểm tra nội dung thực của tệp và định dạng.
- Ảnh đầu tiên tự là ảnh chính. Cập nhật metadata với `isPrimary: true` sẽ chuyển ảnh đó thành ảnh chính; không cho đặt ảnh chính hiện tại thành `false` mà chưa chọn ảnh thay thế. Xóa riêng ảnh chính thì tự chọn ảnh còn lại đầu tiên nếu còn ảnh.
- Upload trả `imageId`, `originalUrl`, `altText`, `isPrimary` trong `data`. Thumbnail 300 × 300 và ảnh medium 800 × 600 được Hangfire tạo sau; trong lúc chờ dùng ảnh gốc.
- Hangfire gửi email chào mừng, xử lý ảnh và tạo sitemap lúc 02:00 UTC mỗi ngày. Sitemap chỉ chứa công thức Published chưa xóa và các danh mục còn hoạt động.
- Job đã ghi thành công vào PostgreSQL được giữ khi worker dừng; lỗi trước khi enqueue phải được ghi nhận để thử lại/đối soát. Tác vụ được thiết kế chịu được việc chạy lại, tránh giả định mỗi job chỉ chạy đúng một lần.

## 4. Yêu cầu chất lượng và bảo mật

| Nội dung | Mục tiêu và quy tắc áp dụng |
| --- | --- |
| Giao diện | Dùng được trên điện thoại, tablet, máy tính; có trạng thái đang tải, lỗi và lưu thành công; hướng đến WCAG 2.1 AA |
| Tài khoản | Mật khẩu tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt; hash bằng ASP.NET Core Identity; khóa 15 phút sau 5 lần đăng nhập sai |
| Quyền truy cập | Kiểm tra vai trò và chủ sở hữu ở backend, kể cả với bước, nguyên liệu và ảnh; không chỉ ẩn nút ở frontend |
| Bảo mật kết nối | HTTPS khi triển khai; CORS chỉ cho origin được cấu hình; secrets nằm trong biến môi trường/User Secrets |
| Giới hạn request | Auth 10, API chung 100, upload 5 request/phút/IP; vượt mức trả `429` kèm `Retry-After` |
| Hiệu năng | Theo điều kiện đo của SRS: tối thiểu 100 người dùng đồng thời; cache warm thì GET có cache p50 ≤ 150 ms, API p95 ≤ 500 ms, p99 ≤ 1.000 ms |
| Frontend | LCP ≤ 2,5 giây, CLS ≤ 0,1, INP ≤ 200 ms; đây là chỉ tiêu cần kiểm thử |
| Độ tin cậy | Uptime mục tiêu ≥ 99,5%, tương ứng tối đa 43,8 giờ gián đoạn/năm 365 ngày; báo cáo theo khoảng thời gian đo thực tế, gồm thời gian bảo trì |
| Kiểm thử | Unit test tầng Application đạt ≥ 80% line coverage; API có ca thành công và lỗi; E2E gồm đăng ký, đăng nhập, tạo bài, đăng bài, tìm kiếm |
| SEO | Tiêu đề, mô tả, URL, JSON-LD Recipe; không tạo dữ liệu đánh giá sao giả; nội dung nháp/lưu trữ không được index |

## 5. Giao diện và môi trường sử dụng

Các màn hình gồm: trang chủ; danh sách/tìm kiếm; chi tiết công thức; danh mục; đăng nhập/đăng ký; hồ sơ; quản lý công thức; tạo/sửa bài; quản lý danh mục của Admin.

Trang công khai chỉ hiển thị nội dung công khai. Xem trước bản nháp hoặc bài lưu trữ yêu cầu đăng nhập và kiểm tra quyền, không dùng trang tĩnh được chia sẻ cho mọi người.

Môi trường phát triển cần .NET 10 SDK, Node.js 22, Docker Compose và Git. Các dịch vụ chính gồm PostgreSQL 16, Redis 7 và MinIO. Bộ phiên bản thư viện cụ thể sẽ được khóa trong mã nguồn khi khởi tạo.

Mốc hỗ trợ được chọn từ Chương 5 của SRS: Chrome/Edge 112+, Firefox 113+, Safari 16+; đây là phạm vi phải xác minh bằng kiểm thử tương thích. Máy chủ production tối thiểu 2 vCPU, 4 GB RAM và 50 GB SSD; máy development chạy đầy đủ dịch vụ nên có 8 GB RAM.

Chưa có mã nguồn và cấu hình chạy hoàn chỉnh trong thư mục. Các lệnh cài đặt, migration và khởi động sẽ được bổ sung sau khi bộ khung được tạo và chạy kiểm chứng.

## 6. Kiến trúc và công nghệ

| Thành phần | Công nghệ / trách nhiệm |
| --- | --- |
| Frontend | Next.js App Router, TypeScript, Tailwind CSS; Auth.js cho luồng Google; TanStack Query, React Hook Form và Zod |
| Backend | .NET 10 Minimal APIs, C#; Clean Architecture; CQRS và MediatR |
| Dữ liệu | EF Core 10, PostgreSQL 16, migrations; FluentValidation kiểm tra dữ liệu tại Application |
| Lưu ảnh và jobs | MinIO, Hangfire, SMTP/MailKit |
| Vận hành | Docker Compose, Nginx, Serilog, OpenTelemetry; Scalar tại `/scalar` |

Domain chỉ chứa nghiệp vụ và định danh tác giả, không phụ thuộc Identity hoặc thư viện ngoài. `ApplicationUser` kế thừa `IdentityUser<string>` đặt ở Infrastructure. Application phụ thuộc Domain và khai báo interface; Infrastructure triển khai các interface; API kết nối qua dependency injection.

**Cache thống nhất dùng Redis:** danh mục 30 phút, danh sách công thức công khai 15 phút, chi tiết công khai 5 phút, tìm kiếm 1 phút; đều dùng thời hạn cố định. Không thêm IMemoryCache hay Output Cache riêng có TTL khác cho cùng dữ liệu trong bản đầu. Không cache dùng chung response cá nhân. Frontend đọc dữ liệu từ backend theo chính sách này; invalidation phải diễn ra sau khi ghi thành công, gồm dữ liệu công khai, danh mục liên quan và bộ nhớ dữ liệu của giao diện đang dùng.

**Khi Redis lỗi:** đọc trực tiếp database, ghi cảnh báo và tiếp tục phục vụ. Sau khi Redis phục hồi, làm mới vùng cache của ứng dụng trước khi dùng lại để không trả dữ liệu cũ từ trước các lần sửa/xóa. `/health/live` kiểm tra tiến trình; `/health/ready` phụ thuộc database, không từ chối lưu lượng chỉ vì cache lỗi. `/health` báo riêng tình trạng PostgreSQL, Redis và MinIO. Redis chỉ phục vụ cache dữ liệu trong cấu hình này, không giữ phiên hay điều kiện bắt buộc để đăng nhập.

## 7. Mô hình dữ liệu

| Thực thể | Nội dung và quy tắc chính |
| --- | --- |
| `ApplicationUser` | Identity có ID string; email, `DisplayName`, `AvatarUrl`, `Bio`, trạng thái tài khoản |
| `Category` | ID Guid; tên, slug, mô tả, ảnh, `IsDeleted`; tên nhập 2–50 ký tự, slug duy nhất |
| `Recipe` | ID Guid; tác giả, danh mục, tiêu đề 5–200 ký tự, mô tả tối đa 2.000 ký tự, thời gian, khẩu phần, độ khó, trạng thái, `IsDeleted`, phiên bản cập nhật |
| `RecipeNutrition` | Nhóm cột nullable `Nutrition_*` trong `Recipes`; gồm calories, protein, carbohydrates, fat, fiber, sodium |
| `RecipeStep` | ID Guid, RecipeId, số bước tự sinh, tiêu đề tùy chọn, mô tả bắt buộc tối đa 2.000 ký tự, thời gian và ảnh tùy chọn |
| `RecipeIngredient` | ID Guid, RecipeId, tên, quantity dạng số, unit dạng chuỗi, notes và orderIndex theo mục 3.7 |
| `RecipeImage` | ID Guid, RecipeId, URL ảnh gốc/medium/thumbnail, alt text, ảnh chính và thứ tự |
| `RefreshToken` | ID Guid, UserId dạng string; hash, thời hạn, thời điểm thu hồi và chuỗi phiên |

ID của các thực thể nghiệp vụ là Guid; `AuthorId` và `UserId` tham chiếu Identity là string. Không yêu cầu mọi lớp đều kế thừa cùng một BaseEntity. `IsDeleted` chỉ áp dụng theo phạm vi xóa mềm ở mục 3.3.

Tên JSON thống nhất: `displayName`, `prepTimeMinutes`, `cookTimeMinutes`, `timerMinutes`, `orderIndex`, `originalUrl`, `expiresAt`. Backend ánh xạ sang tên cột tương ứng; không dùng lẫn các tên cũ cho cùng một trường API. Hướng dẫn nấu dùng `steps`; trường `Instructions` cũ không còn bắt buộc trong model mới và không dùng làm nguồn nội dung thứ hai.

Recipe có token phiên bản `RowVersion` do backend cập nhật mỗi khi công thức hoặc dữ liệu con thay đổi; lưu dạng `bytea`, không coi đó là kiểu timestamp tự tăng của PostgreSQL. API cung cấp phiên bản qua ETag để tránh ghi đè thay đổi của người khác.

## 8. Quy ước API và ví dụ

### 8.1. Quy ước chung

- API nghiệp vụ có tiền tố `/api/v1`; JSON dùng camelCase; upload ảnh dùng `multipart/form-data`.
- Thành công trả `{ "data": ... }`; danh sách phân trang thêm `meta` gồm `page`, `pageSize`, `total`, `totalPages`. `204` không có body.
- Lỗi dùng Problem Details, gồm status, mô tả và mã lỗi để frontend xử lý: `400` cho JSON/tệp không hợp lệ; `401` cho xác thực thất bại; `403` thiếu quyền; `404` không có tài nguyên hoặc đã xóa; `409` xung đột phiên bản/dữ liệu duy nhất/danh mục còn công thức; `422` dữ liệu hoặc điều kiện nghiệp vụ không hợp lệ.
- Khi thay đổi công thức đã tồn tại hoặc dữ liệu con, client gửi phiên bản công thức qua `If-Match`; thiếu hoặc sai định dạng trả `400`, phiên bản cũ trả `409`. Backend cập nhật phiên bản trong cùng giao dịch; response thành công trả ETag mới. CORS cho phép `If-Match` và cho frontend đọc ETag.
- Các endpoint danh sách quản lý bên dưới là bổ sung của bản thống nhất, nhằm tách dữ liệu công khai khỏi dữ liệu riêng của tác giả/Admin.

### 8.2. Các endpoint chính

Các đường dẫn trong bảng đều đứng sau `/api/v1`.

| Method | Đường dẫn | Công dụng |
| --- | --- | --- |
| POST | `/auth/register`, `/auth/login`, `/auth/google` | Đăng ký/đăng nhập và nhận bộ token |
| POST | `/auth/refresh`, `/auth/logout` | Làm mới hoặc thu hồi phiên bằng refresh token |
| GET / PATCH | `/auth/me` | Xem/cập nhật hồ sơ người đang đăng nhập |
| GET | `/categories`, `/categories/{slug}` | Danh mục chưa xóa và công thức công khai thuộc danh mục |
| POST | `/categories` | Admin tạo danh mục |
| PATCH / DELETE | `/categories/{id}` | Admin sửa một phần hoặc xóa mềm danh mục |
| GET | `/recipes`, `/recipes/search`, `/recipes/{slug}` | Danh sách/tìm kiếm công khai; chi tiết kiểm tra quyền nếu bài chưa công khai |
| GET | `/me/recipes`, `/admin/recipes` | Quản lý bài chưa xóa của chính mình hoặc toàn hệ thống |
| POST | `/recipes` | Tạo Draft; nhận kèm nutrition, ingredients và steps |
| PATCH / DELETE | `/recipes/{id}` | Sửa một phần thông tin hoặc xóa mềm công thức |
| PATCH | `/recipes/{id}/publish` | Xuất bản công thức đủ điều kiện |
| PATCH | `/recipes/{id}/unpublish` | Đưa bài Published hoặc Archived về Draft |
| PATCH | `/recipes/{id}/archive` | Chuyển Draft hoặc Published sang Archived |
| POST | `/recipes/{id}/steps`, `/recipes/{id}/ingredients` | Thêm bước tự đánh số hoặc thêm nguyên liệu |
| PATCH / DELETE | `/recipes/{id}/steps/{stepId}`, `/recipes/{id}/ingredients/{ingredientId}` | Sửa một phần hoặc xóa dữ liệu con |
| POST | `/recipes/{id}/images` | Upload ảnh |
| PATCH / DELETE | `/recipes/{id}/images/{imageId}` | Cập nhật metadata/chọn ảnh chính hoặc xóa riêng ảnh |

Chủ sở hữu hoặc Admin mới được ghi vào công thức/dữ liệu con. Bản này dùng **PATCH cho cập nhật một phần** thay cho những mô tả PUT không thống nhất trong PDF; không cung cấp song song hai kiểu cập nhật cho cùng chức năng. Không có API dinh dưỡng riêng hoặc endpoint `/primary` riêng.

### 8.3. Ví dụ tạo công thức

Đây là dữ liệu minh họa cấu trúc request. `categoryId` trong ví dụ phải được thay bằng ID một danh mục thực tế chưa xóa; các chỉ số dinh dưỡng chỉ là số liệu mẫu.

```json
{
  "title": "Salad rau trộn",
  "description": "Món salad đơn giản, không cần nấu.",
  "categoryId": "64c60fa4-1fd0-4fbb-9d61-18e20fd51027",
  "prepTimeMinutes": 15,
  "cookTimeMinutes": 0,
  "servings": 2,
  "difficulty": "Easy",
  "nutrition": {
    "calories": 120,
    "protein": 3,
    "carbohydrates": 15,
    "fat": 5,
    "fiber": 4,
    "sodium": null
  },
  "ingredients": [
    { "name": "Xà lách", "quantity": 200, "unit": "g", "notes": null, "orderIndex": 0 },
    { "name": "Dầu ô liu", "quantity": 0.5, "unit": "muỗng canh", "notes": null, "orderIndex": 1 },
    { "name": "Muối", "quantity": null, "unit": null, "notes": "vừa đủ", "orderIndex": 2 }
  ],
  "steps": [
    { "title": "Sơ chế", "description": "Rửa sạch rau và để ráo.", "timerMinutes": 10, "imageUrl": null },
    { "title": "Trộn salad", "description": "Trộn rau với dầu ô liu và muối.", "timerMinutes": 5, "imageUrl": null }
  ]
}
```

Backend tạo công thức Draft và tự gán hai bước số 1, 2; response `201` trả công thức trong `data`, gồm ID, slug và số bước đã sinh. Ví dụ thể hiện dinh dưỡng gửi kèm, lượng `1/2` chuyển thành `0.5`, đơn vị là chuỗi và nguyên liệu “vừa đủ” dùng `null`.

## Kế hoạch triển khai và kiểm tra tính nhất quán

Theo quy trình ghi trong TXT, mỗi chức năng thực hiện qua hai bước: **phân tích và lập kế hoạch cài đặt chi tiết trong Markdown**, sau đó **cài đặt và kiểm thử theo kế hoạch đã được thống nhất**. README này là căn cứ yêu cầu cho bước lập kế hoạch.

| Thứ tự | Công việc | Điều kiện hoàn thành |
| --- | --- | --- |
| 1 | Khóa model, kiểu dữ liệu, DTO, response và quy tắc xóa | Backend/frontend dùng cùng tên trường và cùng điều kiện dữ liệu |
| 2 | Tạo bộ khung, migrations, Identity và danh mục | Chạy được môi trường, đăng nhập và quản lý danh mục đúng quyền |
| 3 | Công thức, nutrition, nguyên liệu, các bước | Tạo cùng giao dịch, cập nhật một phần đúng nghĩa, tự đánh số bước |
| 4 | Tìm kiếm, ảnh, jobs, cache và giao diện | Tích hợp được luồng tạo → upload → đăng → tìm kiếm → xóa mềm |
| 5 | Kiểm thử tích hợp, hiệu năng, lỗi và tài liệu | Có kết quả kiểm tra, hướng dẫn chạy thực tế và kịch bản demo |

Các tình huống phải kiểm tra trước khi coi 5 điểm góp ý đã xử lý xong:

| Điểm | Kiểm tra bắt buộc |
| --- | --- |
| Xóa mềm | Sau xóa, bản ghi và ảnh còn tồn tại nhưng API/danh sách/tìm kiếm không trả bài; truy cập dữ liệu con của bài đã xóa bị chặn |
| Sắp xếp | Cùng danh sách cho đúng hai chiều `title`; dữ liệu trùng tiêu chí có thứ tự phụ ổn định; `-title`, `dsc`, trường không hợp lệ bị từ chối |
| Dinh dưỡng | Một POST lưu đủ dữ liệu; nutrition sai làm cả giao dịch không được lưu; PATCH bỏ qua, gửi null và gửi object có ba ý nghĩa đúng như mục 3.5 |
| Bước nấu | Client không cần số bước; xóa bước giữa vẫn liên tục; thêm/xóa đồng thời không tạo trùng số hoặc ghi đè phiên bản |
| Nguyên liệu | `0.5` và đơn vị chuỗi được lưu đúng; “vừa đủ” đúng cặp null; lượng âm, 0, số quá giới hạn, thiếu đơn vị hoặc chuỗi lượng bị từ chối |

## Tài liệu tham chiếu

- [PDF đặc tả gốc](./SRS_Culinary_Blog_v1.0.0.pdf): nguồn phạm vi chức năng và công nghệ.
- [Góp ý của giảng viên](./c%C3%B3%205%206%20c%C3%A1i%20%C4%91i%E1%BB%83m%20m%C3%A2u%20thu%E1%BA%ABn%20g%C3%AC%20%C4%91%C3%B3%20ph%E1%BA%A3.txt): 5 điểm cần lựa chọn và quy trình làm việc.
- [Phân tích mâu thuẫn](./mauthuan.md): tài liệu so sánh các phương án; các lựa chọn áp dụng được ghi cụ thể trong README mới này.
