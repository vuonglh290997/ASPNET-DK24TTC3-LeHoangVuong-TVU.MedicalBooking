Hệ thống Đặt lịch & Quản lý Khám bệnh – 2025

Thời gian phát triển: 20/11/2025 – nay
Công nghệ: ASP.NET Core Razor Pages · Entity Framework Core · SQL Server 2022 · UML · PlantUML · Bootstrap/Tailwind (tuỳ chọn)

📌 1. Giới thiệu

Hệ thống hỗ trợ bệnh nhân đăng ký lịch khám, đặt lịch hẹn với bác sĩ, quản lý hoá đơn – thanh toán – chỉ định dịch vụ.
Dự án được xây dựng nhằm:

Số hoá quy trình đặt lịch khám tại cơ sở y tế

Giảm tải cho quầy tiếp nhận

Tự động hoá quy trình thanh toán (QR Code / ngân hàng)

Quản lý hiệu quả dữ liệu bệnh nhân – bác sĩ – lịch khám

🏗 2. Kiến trúc hệ thống

Hệ thống được xây dựng theo kiến trúc 3-Layer Architecture:

2.1 Các lớp chính

Presentation Layer: Razor Pages UI, xác thực người dùng, trình bày dữ liệu

Business Layer: Xử lý nghiệp vụ (đặt lịch, chỉ định, thanh toán, phân quyền…)

Data Access Layer: EF Core, Repository Pattern, truy cập SQL Server

2.2 Thành phần tích hợp

Hệ thống Ngân hàng (QR Pay API)

Module CLS / Bác sĩ thực hiện chỉ định

Quầy thu phí

📦 3. Công nghệ sử dụng
Công nghệ	Mô tả
ASP.NET Core 8.0 Razor Pages	Xây dựng giao diện & routing
Entity Framework Core 8	ORM, Code First, Migration
SQL Server 2022	Lưu trữ dữ liệu hệ thống
PlantUML	Vẽ sơ đồ Use Case, Class, Sequence
Dependency Injection	Quản lý service
Identity / Authentication	Đăng nhập – phân quyền người dùng
REST API	Giao tiếp ngân hàng & module khác
Bootstrap/Tailwind	Giao diện người dùng
🗄 4. Cơ sở dữ liệu

Các bảng chính:

Users, Roles, Permissions – Quản lý phân quyền

Doctors, Patients – Dữ liệu nhân sự & hồ sơ bệnh nhân

Appointments – Lịch hẹn & trạng thái

Services, MedicalOrders – Chỉ định dịch vụ

Invoices, Payments – Thanh toán QR / thường quy

📑 5. UML – Mô hình hóa hệ thống

Bao gồm:

Use Case Diagram: Đặt lịch, thanh toán, kiểm tra hồ sơ

Sequence Diagram: Quy trình chỉ định dịch vụ → thanh toán QR (đã cung cấp)

Class Diagram: Các thực thể chính: User, Appointment, Invoice, Service…

File PlantUML nằm tại thư mục:

/docs/uml/*.puml

⚙️ 6. Tính năng chính
6.1 Đối với bệnh nhân

Đặt lịch khám theo bác sĩ / khoa

Nhận thông báo lịch

Thanh toán chi phí qua QR hoặc quầy thu

Theo dõi trạng thái dịch vụ

6.2 Đối với bác sĩ

Xem lịch khám

Chỉ định dịch vụ

Theo dõi tình trạng thanh toán

6.3 Đối với nhân viên y tế

Quản lý lịch hẹn

Kiểm tra hóa đơn thanh toán

Xác nhận thực hiện dịch vụ

6.4 Đối với quản trị viên

Quản lý người dùng

Phân quyền & nhóm quyền

Quản lý danh mục dịch vụ

🔐 7. Bảo mật

ASP.NET Core Identity

Hash mật khẩu (PBKDF2)

JWT (tuỳ chọn)

Phân quyền theo Role / Permission

Log hoạt động người dùng

📝 8. Cài đặt & triển khai
8.1 Yêu cầu

SQL Server 2022

.NET SDK 8.0

Visual Studio 2022 hoặc VS Code

8.2 Bước cài đặt
git clone https://github.com/.../booking-system.git
cd booking-system
dotnet restore
dotnet ef database update
dotnet run

📚 9. Tài liệu tham khảo

Microsoft ASP.NET Core Docs

EF Core Docs

UML Specification – OMG

PlantUML Official

(Chi tiết danh sách tài liệu mình đã gửi bạn ở phần trên.)

🧩 10. Hướng phát triển

Tích hợp AI gợi ý lịch khám tối ưu

Mobile App (Flutter / MAUI)

Dashboard phân tích số liệu

Tự động hoá gợi ý lịch dựa theo tần suất khám

Tích hợp bảo hiểm y tế điện tử (VSS API)

🏁 11. Kết luận

Dự án đạt được mục tiêu số hoá quy trình đặt lịch, tối ưu vận hành, hỗ trợ thanh toán điện tử và nâng cao trải nghiệm cho bệnh nhân. Kiến trúc hệ thống rõ ràng, mở rộng linh hoạt và dễ dàng tích hợp thêm nhiều module trong tương lai.
