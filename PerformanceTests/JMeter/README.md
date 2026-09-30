# Hướng Dẫn Kiểm Thử Hiệu Năng & Tải (Performance & Load Testing) Bằng Apache JMeter

Thư mục này chứa kịch bản kiểm thử tải và hiệu năng cho hệ thống **Quiz_Web Online Learning System** bằng **Apache JMeter**.

---

## 1. Yêu Cầu Cài Đặt

1. **Java Runtime**: Đã có sẵn trên máy bạn (`OpenJDK 25 LTS`).
2. **Tải Apache JMeter**:
   - Truy cập trang chủ: [https://jmeter.apache.org/download_jmeter.cgi](https://jmeter.apache.org/download_jmeter.cgi)
   - Tải file nén dạng Zip: **Binaries -> `apache-jmeter-5.6.3.zip`**
   - Giải nén vào thư mục bất kỳ (ví dụ: `C:\apache-jmeter-5.6.3` hoặc `D:\apache-jmeter-5.6.3`).

*(Khuyên dùng)*: Thêm đường dẫn thư mục `bin` của JMeter (ví dụ: `C:\apache-jmeter-5.6.3\bin`) vào biến môi trường `PATH` của Windows để có thể gõ lệnh `jmeter` ở mọi cửa sổ PowerShell/CMD.

---

## 2. Cấu Trúc Kịch Bản Kiểm Thử (`load_test_plan.jmx`)

Kịch bản mô phỏng hành vi của nhiều học viên truy cập đồng thời vào hệ thống:

| STT | Bước Kiểm Thử (Sampler) | Endpoint | Mục Đích |
|---|---|---|---|
| 1 | **01_GET_HomePage** | `/` | Kiểm tra tải trang chủ / trang đón tiếp |
| 2 | **02_GET_Introduce** | `/Introduce` | Kiểm tra trang giới thiệu hệ thống |
| 3 | **03_GET_CourseCatalog** | `/Course` | Kiểm tra truy vấn danh mục khóa học từ CSDL PostgreSQL |
| 4 | **04_GET_LoginPage** | `/Account/Login` | Kiểm tra trang đăng nhập |
| 5 | **05_GET_FlashcardExplore** | `/Flashcard/Explore` | Kiểm tra trang khám phá thẻ ghi nhớ flashcard |

Mỗi bước đều có:
- **Think Time**: 1 giây mô phỏng thời gian người dùng đọc/dừng trên trang.
- **Assertion**: Kiểm tra mã phản hồi HTTP `200 OK`.
- **Cookie Manager**: Tự động lưu và gửi Cookie phiên làm việc (Session).

---

## 3. Cách Chạy Kiểm Thử

### Cách 1: Chạy Tự Động Bằng PowerShell (Tạo Báo Cáo HTML Đẹp Mắt)

Mở PowerShell tại thư mục `PerformanceTests/JMeter` và chạy:

```powershell
# Chạy kiểm thử trên server Render (20 người dùng đồng thời trong 60 giây)
.\run_load_test.ps1 -Target "online-learning-system-543q.onrender.com" -Protocol "https" -Users 20 -Duration 60

# Hoặc chạy kiểm thử ở môi trường Local (localhost:5000)
.\run_load_test.ps1 -Target "localhost" -Port "5000" -Protocol "http" -Users 10 -Duration 30
```

> **Sau khi chạy xong**, kịch bản sẽ tự động mở trang báo cáo HTML Dashboard (`report/index.html`) trực tiếp trên trình duyệt.

---

### Cách 2: Mở Bằng Giao Diện Đồ Họa JMeter (GUI Mode)

1. Mở thư mục cài đặt JMeter -> vào thư mục `bin` -> bấm đúp chuột vào `jmeter.bat`.
2. Trên thanh menu JMeter: chọn **File** -> **Open** (Ctrl+O) -> chọn tệp `load_test_plan.jmx`.
3. Bạn có thể:
   - Thay đổi số lượng người dùng đồng thời (`Number of Threads`) trong `Learners Simulation Thread Group`.
   - Bấm nút **Start (Play màu xanh)** để chạy.
   - Nhấp vào **View Results Tree**, **Summary Report**, hoặc **Aggregate Report** để xem kết quả theo thời gian thực.

---

## 4. Các Chỉ Số Cần Đánh Giá Sau Kiểm Thử

Khi đọc báo cáo HTML hoặc bảng Summary Report, bạn cần chú ý các chỉ số quan trọng:

1. **Average / 90th pct / 95th pct Response Time**:
   - Thời gian máy chủ phản hồi (tính bằng mili-giây, ms).
   - Chuẩn tốt cho Web: `< 1000ms` (1 giây).
2. **Throughput (TPS / RPS)**:
   - Số lượng request mà server xử lý được trong 1 giây. Càng cao càng tốt.
3. **Error % (Tỉ lệ lỗi)**:
   - Tỉ lệ request bị lỗi (mã lỗi 5xx, 4xx, timeout).
   - Chuẩn đạt yêu cầu: **0.00%** hoặc `< 1%`.
