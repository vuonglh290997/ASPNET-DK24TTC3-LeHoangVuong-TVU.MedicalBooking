✅ 1. RESTORE bằng giao diện SSMS (dễ nhất)
Bước 1: Mở SSMS → Kết nối SQL Server
Bước 2: Chuột phải vào “Databases” → chọn Restore Database…
Bước 3: Chọn nguồn file .BAK

Ở mục Source → Device

Chọn dấu […]

Nhấn Add… và chọn file TVU_Appointment.bak

Bước 4: Chọn database đích

Ở mục Destination

Name: nhập TVU_Appointment (hoặc tên mới bạn muốn)

Bước 5: Kiểm tra đường dẫn file .MDF & .LDF

Vào tab Files

Nếu bạn restore sang server khác, nên tick:
✔ Relocate all files to folder (nếu có)
— hoặc chỉnh lại thủ công đường dẫn về thư mục chứa database của SQL:

Ví dụ:

C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\

Bước 6: Thực hiện Restore

Vào tab Options

Tick:

✔ Overwrite the existing database (WITH REPLACE) nếu restore đè

Nhấn OK để chạy restore

✅ 2. RESTORE bằng T-SQL (nếu thích dùng lệnh)
Trường hợp 1: Restore lần đầu (không đè)
RESTORE DATABASE TVU_Appointment
FROM DISK = 'D:\Backup\TVU_Appointment.bak'
WITH MOVE 'TVU_Appointment' TO 'D:\Data\TVU_Appointment.mdf',
     MOVE 'TVU_Appointment_log' TO 'D:\Data\TVU_Appointment_log.ldf',
     NORECOVERY, REPLACE;

Trường hợp 2: Restore đè database đang tồn tại
ALTER DATABASE TVU_Appointment SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE TVU_Appointment
FROM DISK = 'D:\Backup\TVU_Appointment.bak'
WITH REPLACE;

ALTER DATABASE TVU_Appointment SET MULTI_USER;

🔎 3. Nếu gặp lỗi thường gặp

Dưới đây là 3 lỗi hay gặp khi restore .bak:

❌ 1. Lỗi “The database is in use”

→ Có kết nối đang mở.

Fix:

ALTER DATABASE TVU_Appointment SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

❌ 2. Lỗi “Cannot find logical file …”

→ Tên file logic trong .bak khác với file cũ.

Fix: xem tên file logic:

RESTORE FILELISTONLY 
FROM DISK = 'D:\Backup\TVU_Appointment.bak';


Sau đó sử dụng đúng tên đó trong RESTORE.

❌ 3. Lỗi “Access denied”

→ Không đủ quyền ghi vào thư mục.

Fix:
Copy file .bak vào thư mục có quyền truy cập, ví dụ:

C:\Backup\
D:\Backup\
