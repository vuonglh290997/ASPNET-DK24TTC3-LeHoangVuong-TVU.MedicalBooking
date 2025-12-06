Thực hiện vẽ sơ đồ UML chức năng
Đánh giá phân tích các đối tượng sửa dụng chức năng hẹn tái khám
Thiết kế cơ sở dữ liệu.

┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│    Role     │────────>│     User     │<────────│    Group    │
│             │  1:N    │              │  M:N    │             │
│  - RoleId   │         │  - UserId    │         │  - GroupId  │
│  - RoleName │         │  - Username  │         │  - GroupName│
└─────────────┘         │  - Password  │         └─────────────┘
      │                 │  - FullName  │               │
      │ 1:N             │  - Email     │               │
      v                 │  - RoleId    │               │
┌─────────────┐         └──────┬───────┘               │
│ Permission  │                │ 1:1                   │
│             │                ├──────────┐            │
│-PermissionId│                │          │            │
└──────┬──────┘                v          v            │
       │              ┌─────────────┐ ┌─────────────┐  │
       │     M:N      │   Patient   │ │   Doctor    │  │
       │              │             │ │             │  │
       └─────>┌───────┤-PatientId   │ │-DoctorId    │  │
              │RolePer│-IdentityCard│ │-Specializa..│  │
              │mission│-DateOfBirth │ │-LicenseNum..│  │
              └───────│-MedicalHist.│ │-YearsOfExp..│  │
                      └──────┬──────┘ └──────┬──────┘  │
                             │ 1:N           │ 1:N     │
                             │               │         │
                             v               v         │
                      ┌──────────────────────────┐    │
                      │     Appointment          │    │
                      │                          │    │
                      │  - AppointmentId         │    │
                      │  - PatientId (FK)        │<───┘
                      │  - DoctorId (FK)         │  UserGroup
                      │  - ServiceId (FK)        │   (M:N)
                      │  - AppointmentDate       │
                      │  - TimeSlot              │
                      │  - Status                │
                      │  - Reason                │
                      └──────┬──────┬────────────┘
                             │ 1:1  │
                             v      │
                      ┌──────────┐  │ N:1
                      │ Payment  │  │
                      │          │  v
                      │-PaymentId│ ┌──────────┐
                      │-Amount   │ │ Service  │
                      │-Method   │ │          │
                      │-Status   │ │-ServiceId│
                      │-QRData   │ │-Name     │
                      └──────────┘ │-Price    │
                                   │-Duration │
                                   └──────────┘
       
       ┌──────────────┐         ┌──────────────┐
       │ AuditLog     │         │DoctorSchedule│
       │              │         │              │
       │  - LogId     │         │ - ScheduleId │
       │  - UserId    │         │ - DoctorId   │
       │  - Action    │         │ - DayOfWeek  │
       │  - EntityType│         │ - StartTime  │
       │  - CreatedAt │         │ - EndTime    │
       └──────────────┘         └──────────────┘
