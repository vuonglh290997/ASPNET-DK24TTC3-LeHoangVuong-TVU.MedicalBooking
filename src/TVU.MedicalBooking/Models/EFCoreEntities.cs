using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVU.MedicalBooking.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public virtual ICollection<UserGroup> UserGroups { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
    }

    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; } // Admin, Doctor, Patient, Staff

        [MaxLength(255)]
        public string Description { get; set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }

    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required, MaxLength(100)]
        public string PermissionName { get; set; } // ViewAppointments, ApproveAppointments, ViewReports, etc.

        [MaxLength(255)]
        public string Description { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }

    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }
    }

    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required, MaxLength(100)]
        public string GroupName { get; set; } // Khoa Nội, Khoa Ngoại, etc.

        [MaxLength(255)]
        public string Description { get; set; }

        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }

    public class UserGroup
    {
        [Key]
        public int UserGroupId { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
    }

    // ============ PATIENT & DOCTOR ============
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required, MaxLength(20)]
        public string IdentityCard { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? EmergencyContact { get; set; }

        [MaxLength(20)]
        public string? EmergencyPhone { get; set; }

        [MaxLength(500)]
        public string? MedicalHistory { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
    }

    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        [MaxLength(100)]
        public string Specialization { get; set; }

        [MaxLength(50)]
        public string LicenseNumber { get; set; }

        public int YearsOfExperience { get; set; }

        [MaxLength(500)]
        public string Biography { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<DoctorSchedule> Schedules { get; set; }
    }

    // ============ APPOINTMENT SYSTEM ============
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required, MaxLength(200)]
        public string ServiceName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Appointment> Appointments { get; set; }
    }

    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }

        public int? ServiceId { get; set; }
        public virtual Service Service { get; set; }

        public DateTime AppointmentDate { get; set; }

        [MaxLength(10)]
        public string? TimeSlot { get; set; } // "08:00", "09:00", etc.

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } // Pending, Approved, Rejected, Completed, Cancelled

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public virtual Payment Payment { get; set; }
    }

    public class DoctorSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }

        [MaxLength(20)]
        public string DayOfWeek { get; set; } // Monday, Tuesday, etc.

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    // ============ PAYMENT SYSTEM ============
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // QRCode, Cash, Card

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } // Pending, Completed, Failed

        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? QRCodeData { get; set; }
    }

    // ============ REPORTING ============
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        public int UserId { get; set; }

        [MaxLength(100)]
        public string Action { get; set; } // Login, CreateAppointment, ApproveAppointment, etc.

        [MaxLength(100)]
        public string EntityType { get; set; } // Appointment, User, etc.

        public int? EntityId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string IpAddress { get; set; }
    }
}
