"# PTUDWNC-2026-Nhom23" 
## Thông tin sinh viên

|MSSV      |Họ tên sinh viên        |Mail cá nhân           |Github cá nhân                  |Số điện thoại   |
|----------|------------------------|-----------------------|--------------------------------|----------------|
|2312610   |Nguyễn Trung Hiệp       |2312610@dlu.edu.vn     |https://github.com/Rikokyu      |0358756475      |
|2312569   |Lê Thị Mai Anh          |2312569@dlu.edu.vn     |https://github.com/maianhtl     |0941617043      |
|2312789   |K'Nguyễn Quang Trường   |2312789@dlu.edu.vn     |https://github.com/2312789-rgb  |0365630966      |
|2312622   |Nguyễn Đức Hoàng        |2312622@dlu.edu.vn     |https://github.com/Sunz9420     |0916152901      |

## Phân công công việc

|MSSV        |Mã module                |Tên module                                              |Ghi chú       |
|------------|-------------------------|--------------------------------------------------------|--------------|
|2312610     |FR-RCP                   |Quản lý Công thức Nấu ăn                                |10 chức năng  |             
|2312569     |FR-JOB/FR-CAT            |Background Jobs/Quản lý Danh mục                        |9 chức năng   |
|2312789     |FR-SRCH/FR-FILE/FR-OBS   |Tìm kiếm & Phân trang/Quản lý Tệp tin/Quan sát Hệ thống |9 chức năng   |
|2312622     |FR-AUTH                  |Xác thực & Quản lý Người dùng                           |7 chức năng   |

## Mô tả tổng quan đề tài
# 🍳 Culinary Blog

**Culinary Blog** là một nền tảng web về **ẩm thực và nấu ăn**, được xây dựng nhằm cho phép người dùng **khám phá, tìm kiếm, chia sẻ và quản lý các công thức nấu ăn** từ nhiều nền ẩm thực khác nhau.

Dự án được phát triển theo mô hình **Full-Stack, API-Driven Architecture**, với Backend và Frontend được tách biệt, giao tiếp thông qua **RESTful API**. Hệ thống hướng đến khả năng bảo mật, hiệu năng, mở rộng và tối ưu SEO.

> 📌 Project ID: `CULINARY-BLOG-V1`
> 📌 Version: `1.0.0`

## 📖 Tổng quan

Culinary Blog cung cấp một không gian để người dùng khám phá các công thức nấu ăn được tổ chức theo danh mục, độ khó và thời gian thực hiện.

Người dùng có thể xem thông tin chi tiết của từng công thức, bao gồm:

* 🖼️ Hình ảnh món ăn
* 🥕 Danh sách nguyên liệu
* 👨‍🍳 Các bước thực hiện
* 📊 Thông tin dinh dưỡng
* ⏱️ Thời gian chuẩn bị và nấu
* 📂 Danh mục và độ khó của món ăn

Bên cạnh việc xem và tìm kiếm công thức, hệ thống còn cho phép người dùng đăng ký trở thành **Author** để tạo, chỉnh sửa, quản lý và xuất bản các công thức của mình. **Admin** có quyền quản lý danh mục và kiểm soát nội dung trên toàn hệ thống.

## ✨ Chức năng chính

Hệ thống được chia thành **7 nhóm chức năng chính với 27 Functional Requirements**.

### 🔐 1. Xác thực & Quản lý người dùng

* Đăng ký tài khoản
* Đăng nhập bằng Email/Mật khẩu
* Đăng nhập bằng Google OAuth 2.0
* JWT Access Token & Refresh Token
* Token Rotation
* Đăng xuất và thu hồi Refresh Token
* Xem và cập nhật hồ sơ cá nhân

Hệ thống sử dụng JWT stateless kết hợp ASP.NET Core Identity để quản lý xác thực và bảo mật tài khoản.

### 📂 2. Quản lý danh mục

* Xem danh sách danh mục
* Xem chi tiết danh mục
* Xem các công thức thuộc danh mục
* Admin tạo danh mục
* Admin cập nhật danh mục
* Admin xóa danh mục

Danh mục sử dụng **Slug** để tạo URL thân thiện với SEO và được cache để cải thiện hiệu năng.

### 🍲 3. Quản lý công thức nấu ăn

Đây là **module cốt lõi** của hệ thống.

Author có thể:

* Tạo công thức mới
* Chỉnh sửa công thức của mình
* Xóa công thức của mình
* Xuất bản / hủy xuất bản
* Lưu trữ công thức
* Quản lý hình ảnh
* Quản lý nguyên liệu
* Quản lý các bước nấu ăn
* Cập nhật thông tin dinh dưỡng

Recipe được thiết kế theo mô hình Aggregate Root, bao gồm các thành phần như `RecipeStep`, `RecipeIngredient`, `RecipeImage` và `RecipeNutrition`.

### 🔎 4. Tìm kiếm & phân trang

Hệ thống hỗ trợ:

* Full-Text Search
* Tìm kiếm công thức bằng tiếng Việt
* Lọc theo danh mục
* Lọc theo độ khó
* Lọc theo thời gian nấu
* Sắp xếp theo nhiều tiêu chí
* Phân trang danh sách kết quả

Full-Text Search được xây dựng trên PostgreSQL với `tsvector`, `tsquery` và extension `unaccent`.

### 🗂️ 5. Quản lý tệp tin

Hệ thống sử dụng **MinIO S3-compatible Object Storage** để lưu trữ hình ảnh công thức.

Các chức năng chính:

* Upload ảnh
* Thiết lập ảnh chính
* Xóa ảnh
* Quản lý ảnh gắn với công thức

### ⚙️ 6. Background Jobs

Sử dụng **Hangfire** để xử lý các tác vụ chạy nền:

* Gửi email chào mừng
* Tạo thumbnail cho hình ảnh
* Sinh sitemap XML

Điều này giúp các tác vụ không ảnh hưởng trực tiếp đến thời gian phản hồi của người dùng.

### 📊 7. Quan sát & giám sát hệ thống

Hệ thống hỗ trợ:

* Health Check
* Structured Logging
* Distributed Tracing
* Theo dõi tình trạng các service
* Correlation ID cho request

Các công nghệ như **Serilog** và **OpenTelemetry** được sử dụng cho logging và observability.

## 👥 Phân quyền

Culinary Blog có 3 loại người dùng:

| Vai trò       | Quyền chính                                                             |
| ------------- | ----------------------------------------------------------------------- |
| 👤 **Guest**  | Xem công thức đã xuất bản, danh mục và tìm kiếm                         |
| ✍️ **Author** | Tạo, sửa, xóa, quản lý và xuất bản công thức của mình                   |
| 🛡️ **Admin** | Toàn quyền của Author + quản lý danh mục và công thức của tất cả Author |

Hệ thống sử dụng kết hợp **Role-Based Authorization**, **Resource-Based Authorization** và **Policy-Based Authorization** để kiểm soát quyền truy cập.

## 🏗️ Kiến trúc hệ thống

Culinary Blog sử dụng mô hình **API-Driven Architecture**:

```text
┌─────────────────────┐
│   Next.js Frontend  │
│    App Router       │
└──────────┬──────────┘
           │
       REST / JSON
           │
           ▼
┌─────────────────────┐
│   .NET 10 Backend   │
│    Minimal APIs     │
│   Clean Architecture│
└──────────┬──────────┘
           │
    ┌──────┼───────────────┐
    ▼      ▼       ▼       ▼
 PostgreSQL Redis   MinIO  Hangfire
```

Backend được xây dựng theo **Clean Architecture** gồm:

```text
Domain
   ↓
Application
   ↓
Infrastructure
   ↓
Presentation
```

Tầng Application sử dụng **CQRS + MediatR**, trong đó mỗi use case được tổ chức thành Command hoặc Query Handler riêng biệt.

## 🛠️ Công nghệ sử dụng

### Backend

* **C#**
* **.NET 10**
* ASP.NET Core Minimal APIs
* Entity Framework Core 10
* ASP.NET Core Identity
* JWT
* Google OAuth 2.0
* MediatR
* FluentValidation
* Serilog
* Hangfire
* OpenTelemetry

### Frontend

* **Next.js**
* **App Router**
* **TypeScript**
* Auth.js

### Database & Storage

* **PostgreSQL 16**
* **Redis 7**
* **MinIO S3-Compatible Object Storage**

### DevOps & Deployment

* Docker
* Docker Compose
* Nginx

Các công nghệ và phiên bản chính được quy định trong SRS của dự án.

## 🔒 Bảo mật

Hệ thống tập trung vào bảo mật thông qua:

* JWT Stateless Authentication
* Role-Based Access Control
* Resource-Based Authorization
* Google OAuth 2.0
* PBKDF2 password hashing
* Refresh Token Rotation
* Input Validation với FluentValidation
* API Error Response theo RFC 7807
* Giới hạn và kiểm tra MIME type đối với file upload

Access Token có thời hạn **15 phút**, Refresh Token có thời hạn **7 ngày** và được rotation khi sử dụng.

## 🚀 Hiệu năng & SEO

Culinary Blog được thiết kế với các cơ chế tối ưu:

* Redis Distributed Cache
* Output Cache
* Next.js ISR
* PostgreSQL Full-Text Search
* Lazy loading hình ảnh
* Open Graph
* JSON-LD Schema.org `Recipe`
* SEO-friendly URL với Slug

Mục tiêu là cung cấp trải nghiệm truy cập nhanh đồng thời giúp các công thức có khả năng được lập chỉ mục tốt trên công cụ tìm kiếm.

## 📌 Phạm vi phiên bản 1.0

Phiên bản 1.0 tập trung vào nền tảng cốt lõi của một Culinary Blog.

Các tính năng **chưa nằm trong phạm vi**:

* ❌ Bình luận
* ❌ Đánh giá sao
* ❌ Bookmark / Favorite
* ❌ Thông báo Real-time
* ❌ Ứng dụng mobile native
* ❌ Thanh toán / thương mại điện tử
* ❌ Nhắn tin trực tiếp
* ❌ GraphQL API

Các tính năng này có thể được xem xét trong những phiên bản tiếp theo.

## 📁 Cấu trúc dự án

```text
Culinary_Blog/
│
├── README.md
├── LICENSE
├── .gitignore
├── .editorconfig
├── docker-compose.yml
├── .env.example
│
├── backend/          # .NET 10 Clean Architecture + Minimal APIs
├── frontend/         # Next.js App Router (culinary-blog-web)
├── infrastructure/   # Nginx, Postgres init, MinIO, observability
├── docs/             # Architecture, ADRs, API overview
└── scripts/          # Setup and database scripts
```

## 🎯 Mục tiêu dự án

Culinary Blog hướng tới việc xây dựng một ứng dụng web full-stack thực tế, đồng thời áp dụng các kiến trúc và kỹ thuật phát triển phần mềm hiện đại như **Clean Architecture, CQRS, RESTful API, JWT Authentication, Full-Text Search, Caching, Object Storage, Background Jobs, Logging và Observability**.

Dự án cũng đóng vai trò là sản phẩm thực hành xuyên suốt môn **Phát triển Ứng dụng Web Nâng cao**, với SRS được xây dựng theo tiêu chuẩn **IEEE 830 / ISO/IEC/IEEE 29148:2018**.

---

## 📄 Documentation

* Software Requirements Specification (SRS) – `v1.0.0`
* Project ID: `CULINARY-BLOG-V1`
* Architecture: API-Driven + Clean Architecture
* API: RESTful
* Database: PostgreSQL 16
