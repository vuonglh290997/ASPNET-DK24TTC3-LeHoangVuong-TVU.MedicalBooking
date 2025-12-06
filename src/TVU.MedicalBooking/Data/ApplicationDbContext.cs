using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== USER CONFIGURATIONS ==========
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Patient)
                    .WithOne(p => p.User)
                    .HasForeignKey<Patient>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Doctor)
                    .WithOne(d => d.User)
                    .HasForeignKey<Doctor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ROLE & PERMISSION ==========
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            });

            // ========== USER GROUP ==========
            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.HasOne(ug => ug.User)
                    .WithMany(u => u.UserGroups)
                    .HasForeignKey(ug => ug.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ug => ug.Group)
                    .WithMany(g => g.UserGroups)
                    .HasForeignKey(ug => ug.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.GroupId }).IsUnique();
            });

            // ========== PATIENT ==========
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasIndex(e => e.IdentityCard).IsUnique();
            });

            // ========== DOCTOR ==========
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasIndex(e => e.LicenseNumber).IsUnique();
            });

            // ========== APPOINTMENT ==========
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Service)
                    .WithMany(s => s.Appointments)
                    .HasForeignKey(a => a.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Payment)
                    .WithOne(p => p.Appointment)
                    .HasForeignKey<Payment>(p => p.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.DoctorId, e.AppointmentDate, e.TimeSlot });
            });

            // ========== DOCTOR SCHEDULE ==========
            modelBuilder.Entity<DoctorSchedule>(entity =>
            {
                entity.HasOne(ds => ds.Doctor)
                    .WithMany(d => d.Schedules)
                    .HasForeignKey(ds => ds.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek });
            });

            // ========== PAYMENT ==========
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(e => e.TransactionId).IsUnique();
            });

            // ========== SEED DATA ==========
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị viên hệ thống" },
                new Role { RoleId = 2, RoleName = "Doctor", Description = "Bác sĩ" },
                new Role { RoleId = 3, RoleName = "Patient", Description = "Bệnh nhân" },
                new Role { RoleId = 4, RoleName = "Staff", Description = "Nhân viên y tế" }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { PermissionId = 1, PermissionName = "ViewAllAppointments", Description = "Xem tất cả lịch hẹn" },
                new Permission { PermissionId = 2, PermissionName = "ApproveAppointments", Description = "Duyệt lịch hẹn" },
                new Permission { PermissionId = 3, PermissionName = "ViewReports", Description = "Xem báo cáo" },
                new Permission { PermissionId = 4, PermissionName = "ManageUsers", Description = "Quản lý người dùng" },
                new Permission { PermissionId = 5, PermissionName = "ManageServices", Description = "Quản lý dịch vụ" },
                new Permission { PermissionId = 6, PermissionName = "ViewOwnAppointments", Description = "Xem lịch hẹn của mình" },
                new Permission { PermissionId = 7, PermissionName = "CreateAppointment", Description = "Tạo lịch hẹn" },
                new Permission { PermissionId = 8, PermissionName = "CancelAppointment", Description = "Hủy lịch hẹn" }
            );

            // Seed Role-Permission mapping
            // Admin - Full permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RolePermissionId = 1, RoleId = 1, PermissionId = 1 },
                new RolePermission { RolePermissionId = 2, RoleId = 1, PermissionId = 2 },
                new RolePermission { RolePermissionId = 3, RoleId = 1, PermissionId = 3 },
                new RolePermission { RolePermissionId = 4, RoleId = 1, PermissionId = 4 },
                new RolePermission { RolePermissionId = 5, RoleId = 1, PermissionId = 5 }
            );

            // Doctor permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RolePermissionId = 6, RoleId = 2, PermissionId = 1 },
                new RolePermission { RolePermissionId = 7, RoleId = 2, PermissionId = 2 },
                new RolePermission { RolePermissionId = 8, RoleId = 2, PermissionId = 3 }
            );

            // Patient permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RolePermissionId = 9, RoleId = 3, PermissionId = 6 },
                new RolePermission { RolePermissionId = 10, RoleId = 3, PermissionId = 7 },
                new RolePermission { RolePermissionId = 11, RoleId = 3, PermissionId = 8 }
            );

            // Seed Groups (Departments)
            modelBuilder.Entity<Group>().HasData(
                new Group { GroupId = 1, GroupName = "Khoa Nội", Description = "Khoa Nội tổng quát" },
                new Group { GroupId = 2, GroupName = "Khoa Ngoại", Description = "Khoa Phẫu thuật" },
                new Group { GroupId = 3, GroupName = "Khoa Nhi", Description = "Khoa Nhi khoa" },
                new Group { GroupId = 4, GroupName = "Khoa Sản", Description = "Khoa Sản phụ khoa" }
            );

            // Seed Services
            modelBuilder.Entity<Service>().HasData(
                new Service { ServiceId = 1, ServiceName = "Khám Nội khoa", Description = "Khám bệnh nội khoa tổng quát", Price = 200000, DurationMinutes = 30, IsActive = true },
                new Service { ServiceId = 2, ServiceName = "Khám Tim mạch", Description = "Khám chuyên khoa tim mạch", Price = 300000, DurationMinutes = 45, IsActive = true },
                new Service { ServiceId = 3, ServiceName = "Khám Nhi khoa", Description = "Khám bệnh cho trẻ em", Price = 250000, DurationMinutes = 30, IsActive = true },
                new Service { ServiceId = 4, ServiceName = "Khám Sản khoa", Description = "Khám thai và sản phụ khoa", Price = 350000, DurationMinutes = 40, IsActive = true },
                new Service { ServiceId = 5, ServiceName = "Tư vấn dinh dưỡng", Description = "Tư vấn chế độ ăn uống", Price = 150000, DurationMinutes = 30, IsActive = true }
            );
        }
    }
}
