# Online Learning System

[![CI](https://github.com/DaoTienDat5345/online-learning-system/actions/workflows/dotnet.yml/badge.svg)](https://github.com/DaoTienDat5345/online-learning-system/actions/workflows/dotnet.yml)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-Proprietary-All%20rights%20reserved-lightgrey)](https://github.com/DaoTienDat5345/online-learning-system)

Nền tảng học tập trực tuyến: bán và học khóa học, luyện flashcard, tổ chức bài kiểm tra, theo dõi tiến độ và thanh toán qua MoMo — trong một ứng dụng web ASP.NET Core duy nhất.

Trang chủ: <https://github.com/DaoTienDat5345/online-learning-system>

---

## Mục lục

- [Tính năng](#tính-năng)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Yêu cầu trước khi cài đặt](#yêu-cầu-trước-khi-cài-đặt)
- [Cài đặt và chạy](#cài-đặt-và-chạy)
- [Cấu hình](#cấu-hình)
- [Tài khoản mẫu](#tài-khoản-mẫu)
- [Kiểm thử hiệu năng](#kiểm-thử-hiệu-năng)
- [Cấu trúc mã nguồn](#cấu-trúc-mã-nguồn)
- [Đóng góp](#đóng-góp)
- [Hỗ trợ](#hỗ-trợ)
- [Giấy phép](#giấy-phép)

## Tính năng

- **Khóa học** — danh mục theo chủ đề, trang chi tiết, tiến độ học, chứng nhận hoàn thành, đánh giá và bình luận.
- **Giỏ hàng & thanh toán** — thanh toán qua cổng MoMo (quét QR), quản lý đơn hàng và lịch sử mua.
- **Flashcard** — tạo bộ thẻ, chế độ học lật thẻ, ghi lại lịch sử luyện tập, khám phá bộ thẻ công khai.
- **Bài kiểm tra (Test)** — ngân hàng câu hỏi trắc nghiệm, thi có chấm điểm và giới hạn lần thi.
- **Bài giảng (Lesson)** — trình soạn thảo với CKEditor, slide, nhúng video và flashcard vào nội dung bài học.
- **Tin nhắn & thông báo** — thông báo nền tảng, nhắn tin nội bộ, nhắc học.
- **Trang quản trị** — quản lý người dùng, khóa học, bài kiểm tra, báo cáo doanh thu và phân tích người dùng.
- **Gợi ý khóa học** — nền tảng chạy nền đề xuất khóa học theo sở thích người học.

## Công nghệ sử dụng

| Lớp | Công nghệ |
| --- | --- |
| Web framework | ASP.NET Core MVC (Razor Views) + API Controllers, .NET 9 |
| ORM | Entity Framework Core 9 (`Npgsql.EntityFrameworkCore.PostgreSQL`) |
| Cơ sở dữ liệu | PostgreSQL (migration `InitialPostgreSQL`) |
| Xác thực | Cookie Authentication, session 3 giờ, phân quyền theo `Role` |
| Lưu trữ media | Cloudflare R2 (S3-compatible) qua `AWSSDK.S3`, fallback lưu `wwwroot/uploads` |
| Thanh toán | MoMo (`captureWallet`) + webhook IPN |
| Email | SMTP cho đặt lại mật khẩu |
| Theo dõi lỗi | Sentry.AspNetCore |
| Front-end | Bootstrap, jQuery, CKEditor, Toastr, LibMan |
| Kiểm thử tải | Apache JMeter |

## Yêu cầu trước khi cài đặt

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (`dotnet --version` phải ≥ 9.0.400)
- [PostgreSQL 14+](https://www.postgresql.org/download/) đang chạy, hoặc Docker Desktop
- Git
- Tùy chọn: [Apache JMeter](https://jmeter.apache.org/download_jmeter.cgi) + JDK 17+ nếu muốn chạy bài kiểm thử tải

> **Lưu ý:** `docker-compose.yml` trong repo hiện vẫn cấu hình SQL Server và chuỗi kết nối MSSQL, trong khi ứng dụng đã chuyển sang PostgreSQL (Npgsql). Hãy dùng PostgreSQL và chỉnh `ConnectionStrings:DefaultConnection` theo hướng dẫn bên dưới.

## Cài đặt và chạy

### 1. Lấy mã nguồn

```bash
git clone https://github.com/DaoTienDat5345/online-learning-system.git
cd online-learning-system
```

### 2. Chuẩn bị cơ sở dữ liệu

Tạo database rồi áp dụng migration:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project Quiz_Web
```

Nếu database đã có sẵn dữ liệu mẫu (khóa học, flashcard, bài kiểm tra), dùng các script trong thư mục `Database/` và `Quiz_Web/LearningPlatform.sql` — script `.sql` dùng cú pháp SQL Server, cần chỉnh lại cho PostgreSQL trước khi chạy.

### 3. Cấu hình kết nối

Sửa `Quiz_Web/appsettings.json` (hoặc dùng biến môi trường `ConnectionStrings__DefaultConnection`):

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=LearningPlatform;Username=postgres;Password=postgres"
}
```

### 4. Chạy ứng dụng

```bash
dotnet restore Quiz_Web.sln
dotnet run --project Quiz_Web
```

Mở trình duyệt tại `http://localhost:5206` (hoặc `https://localhost:7158` với profile `https`).

### 5. Chạy bằng Docker

```bash
docker build -t online-learning-system .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=LearningPlatform;Username=postgres;Password=postgres" online-learning-system
```

### 6. Chạy bằng Docker Compose

```bash
$env:DB_PASSWORD="mat-khau-cua-ban"
docker compose up --build
```

## Cấu hình

Các khóa cấu hình được đọc trong `Quiz_Web/appsettings.json`:

| Khóa | Bắt buộc | Mô tả |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | Có | Chuỗi kết nối PostgreSQL |
| `Sentry:Dsn` | Không | Gửi lỗi lên Sentry (đã điền sẵn DSN công khai) |
| `CloudflareR2:AccountId` / `AccessKeyId` / `SecretAccessKey` / `BucketName` / `PublicUrl` | Không | Lưu media lên R2; bỏ trống thì file nằm ở `wwwroot/uploads` |
| `MoMoSettings:PartnerCode` / `AccessKey` / `SecretKey` / `ApiEndpoint` / `RedirectUrl` / `IpnUrl` | Không | Thanh toán MoMo; `IpnUrl` phải trỏ tới endpoint nhận callback |
| `EmailSettings:SmtpHost` / `SmtpPort` / `FromEmail` / `FromPassword` / `FromName` | Không | Gửi email đặt lại mật khẩu |

Không commit secret thật vào `appsettings.json` — dùng biến môi trường hoặc user secrets:

```bash
dotnet user-secrets set "MoMoSettings:AccessKey" "your-access-key"
```

## Tài khoản mẫu

Các tài khoản seed sẵn trong `Quiz_Web/LearningPlatform.sql`:

| Tên đăng nhập | Email | Vai trò |
| --- | --- | --- |
| `admin` | admin@learn.vn | Quản trị viên (`/admin`) |
| `teacher` | teacher@learn.vn | Giảng viên |
| `student1` | student1@learn.vn | Học viên |

Mật khẩu seed được lưu dưới dạng SHA-256 base64 trong file SQL — hãy đổi mật khẩu ngay khi triển khai thật.

## Kiểm thử hiệu năng

Bộ kịch bản Apache JMeter nằm ở `PerformanceTests/JMeter/`:

```powershell
cd PerformanceTests/JMeter
.\run_load_test.ps1 -Target "localhost" -Port "5206" -Protocol "http" -Users 10 -Duration 30
```

Báo cáo HTML được tạo tự động tại `PerformanceTests/JMeter/report/index.html`. Chi tiết về cách chạy và cách đọc chỉ số: [`PerformanceTests/JMeter/README.md`](PerformanceTests/JMeter/README.md).

CI chạy `dotnet restore` → `build` → `test` trên mỗi pull request nhắm vào `main`/`develop`.

## Cấu trúc mã nguồn

```
online-learning-system/
├── Quiz_Web/                  # Ứng dụng ASP.NET Core MVC
│   ├── Controllers/           # Controller MVC + API (Controllers/API)
│   ├── Models/
│   │   ├── Entities/          # 50+ entity EF Core
│   │   ├── EF/                # DbContext + migrations
│   │   ├── ViewModels/        # ViewModel cho các view
│   │   └── Settings/          # CloudflareR2Settings, MoMoSettings
│   ├── Services/              # Business logic + interface (IServices)
│   ├── Views/                 # Razor views (Home, Course, Flashcard, Test, Admin…)
│   ├── Migrations/            # Migration PostgreSQL
│   └── wwwroot/               # CSS, JS, lib client, uploads
├── Database/                  # Script seed dữ liệu
├── PerformanceTests/JMeter/   # Kịch bản kiểm thử tải
├── Dockerfile
├── docker-compose.yml
└── Quiz_Web.sln
```


