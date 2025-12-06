 định hình project và các tính năng.
 TVU.MedicalBooking/
│
├── 📂 Data/                              # Data Access Layer
│   └── ApplicationDbContext.cs          # DbContext chính + Seed data
│
├── 📂 Models/                            # Domain Models
│   └── EFCoreEntities.cs                # Tất cả entities của hệ thống
│
├── 📂 Interfaces/                        # Service Interfaces
│   ├── IAuthService.cs                  # Interface xác thực
│   ├── IUserService.cs                  # Interface quản lý user
│   ├── IAppointmentService.cs           # Interface quản lý lịch hẹn
│   ├── IPaymentService.cs               # Interface thanh toán
│   ├── IReportService.cs                # Interface báo cáo
│   └── IAuditService.cs                 # Interface audit log
│
├── 📂 Services/                          # Business Logic Layer
│   ├── AuthService.cs                   # Service xác thực & phân quyền
│   ├── UserService.cs                   # Service quản lý người dùng
│   ├── AppointmentService.cs            # Service quản lý lịch hẹn
│   ├── PaymentService.cs                # Service thanh toán (QR, Cash, Card)
│   ├── ReportService.cs                 # Service tạo báo cáo thống kê
│   └── AuditService.cs                  # Service ghi log hoạt động
│
├── 📂 Pages/                             # Presentation Layer (Razor Pages)
│   ├── 📂 Account/                       # Xác thực người dùng
│   │   ├── Login.cshtml                 # Đăng nhập
│   │   ├── Register.cshtml              # Đăng ký
│   │   └── Logout.cshtml                # Đăng xuất
│   │
│   ├── 📂 Patient/                       # Trang dành cho bệnh nhân
│   │   ├── Dashboard.cshtml             # Trang chủ bệnh nhân
│   │   ├── BookAppointment.cshtml       # Đặt lịch khám
│   │   ├── MyAppointments.cshtml        # Danh sách lịch hẹn
│   │   ├── AppointmentDetail.cshtml     # Chi tiết lịch hẹn
│   │   ├── CompleteProfile.cshtml       # Hoàn thiện hồ sơ
│   │   └── Payment.cshtml               # Thanh toán
│   │
│   ├── 📂 Doctors/                       # Trang dành cho bác sĩ
│   │   ├── Dashboard.cshtml             # Trang chủ bác sĩ
│   │   └── AppointmentDetail.cshtml     # Xem chi tiết & xử lý lịch hẹn
│   │
│   ├── 📂 Admin/                         # Trang quản trị
│   │   ├── Dashboard.cshtml             # Trang tổng quan thống kê
│   │   ├── Users.cshtml                 # Quản lý người dùng
│   │   └── Reports.cshtml               # Báo cáo chi tiết
│   │
│   ├── 📂 Shared/                        # Layouts & Partials
│   │   ├── _Layout.cshtml               # Layout chính
│   │   └── _ValidationScriptsPartial.cshtml
│   │
│   ├── Index.cshtml                     # Trang chủ
│   ├── Privacy.cshtml                   # Chính sách bảo mật
│   ├── Error.cshtml                     # Trang lỗi
│   ├── _ViewImports.cshtml              # Import chung
│   └── _ViewStart.cshtml                # Start configuration
│
├── 📂 wwwroot/                           # Static Files
│   ├── 📂 css/                          # CSS files
│   ├── 📂 js/                           # JavaScript files
│   └── 📂 lib/                          # Third-party libraries
│       ├── bootstrap/
│       ├── jquery/
│       └── jquery-validation/
│
├── 📂 Properties/
│   └── launchSettings.json              # Launch configuration
│
├── Program.cs                           # Entry point & Service configuration
├── appsettings.json                     # Cấu hình ứng dụng
├── appsettings.Development.json         # Cấu hình môi trường Development
└── TVU.MedicalBooking.csproj           # Project file
