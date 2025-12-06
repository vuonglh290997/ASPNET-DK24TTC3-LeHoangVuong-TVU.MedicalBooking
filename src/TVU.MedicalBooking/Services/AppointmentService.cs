using Microsoft.EntityFrameworkCore;
using TVU.MedicalBooking.Data;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AppointmentService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<(bool Success, string Message, int AppointmentId)> CreateAppointmentAsync(Appointment appointment)
        {
            try
            {
                // Check if time slot is available
                var existingAppointment = await _context.Appointments
                    .FirstOrDefaultAsync(a =>
                        a.DoctorId == appointment.DoctorId &&
                        a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                        a.TimeSlot == appointment.TimeSlot &&
                        a.Status != "Cancelled");

                if (existingAppointment != null)
                {
                    return (false, "Khung giờ này đã được đặt", 0);
                }

                appointment.Status = "Pending";
                appointment.CreatedAt = DateTime.Now;

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    appointment.PatientId,
                    "CreateAppointment",
                    "Appointment",
                    appointment.AppointmentId,
                    $"Created appointment for {appointment.AppointmentDate:dd/MM/yyyy} at {appointment.TimeSlot}");

                return (true, "Đặt lịch thành công", appointment.AppointmentId);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", 0);
            }
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Service)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Service)
                .Include(a => a.Payment)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId, DateTime? date = null)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Service)
                .Include(a => a.Payment)
                .Where(a => a.DoctorId == doctorId);

            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }

            return await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetPendingAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Service)
                .Where(a => a.Status == "Pending")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate.Date >= startDate.Date &&
                           a.AppointmentDate.Date <= endDate.Date)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();
        }

        public async Task<bool> ApproveAppointmentAsync(int appointmentId, int approvedBy)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null || appointment.Status != "Pending")
                    return false;

                appointment.Status = "Approved";
                appointment.ApprovedAt = DateTime.Now;
                appointment.ApprovedBy = approvedBy;

                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    approvedBy,
                    "ApproveAppointment",
                    "Appointment",
                    appointmentId,
                    $"Approved appointment #{appointmentId}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectAppointmentAsync(int appointmentId, int rejectedBy, string reason)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null || appointment.Status != "Pending")
                    return false;

                appointment.Status = "Rejected";
                appointment.Notes = reason;
                appointment.ApprovedBy = rejectedBy;
                appointment.ApprovedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    rejectedBy,
                    "RejectAppointment",
                    "Appointment",
                    appointmentId,
                    $"Rejected appointment #{appointmentId}: {reason}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CompleteAppointmentAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null || appointment.Status != "Approved")
                    return false;

                appointment.Status = "Completed";
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string reason)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                    return false;

                appointment.Status = "Cancelled";
                appointment.Notes = reason;
                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    appointment.PatientId,
                    "CancelAppointment",
                    "Appointment",
                    appointmentId,
                    $"Cancelled appointment #{appointmentId}: {reason}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
        {
            // Define all possible time slots
            var allTimeSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30", "10:00", "10:30",
                "11:00", "11:30", "13:00", "13:30", "14:00", "14:30",
                "15:00", "15:30", "16:00", "16:30", "17:00", "17:30"
            };

            // Get booked time slots
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != "Cancelled")
                .Select(a => a.TimeSlot)
                .ToListAsync();

            // Return available slots
            return allTimeSlots.Except(bookedSlots).ToList();
        }

        public async Task<List<Service>> GetAllServicesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();
        }

        public async Task<Service> GetServiceByIdAsync(int serviceId)
        {
            return await _context.Services.FindAsync(serviceId);
        }
    }
}
