using Microsoft.EntityFrameworkCore;
using TVU.MedicalBooking.Data;
using TVU.MedicalBooking.Interfaces;

namespace TVU.MedicalBooking.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>> GetAppointmentStatsByStatusAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= endDate.Value);

            var stats = await query
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            return stats;
        }

        public async Task<Dictionary<string, decimal>> GetRevenueReportAsync(
            DateTime startDate,
            DateTime endDate)
        {
            var payments = await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Service)
                .Where(p => p.Status == "Completed" &&
                           p.PaidAt.HasValue &&
                           p.PaidAt.Value.Date >= startDate.Date &&
                           p.PaidAt.Value.Date <= endDate.Date)
                .ToListAsync();

            var report = new Dictionary<string, decimal>
            {
                { "TotalRevenue", payments.Sum(p => p.Amount ?? 0) },
                { "CompletedPayments", payments.Count },
                { "AveragePayment", payments.Any() ? payments.Average(p => p.Amount ?? 0) : 0 }
            };

            return report;
        }

        public async Task<List<dynamic>> GetTopDoctorsByAppointmentsAsync(int topCount = 10)
        {
            var topDoctors = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .GroupBy(a => new { a.DoctorId, a.Doctor.User.FullName })
                .Select(g => new
                {
                    DoctorId = g.Key.DoctorId,
                    DoctorName = g.Key.FullName,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(a => a.Status == "Completed")
                })
                .OrderByDescending(x => x.TotalAppointments)
                .Take(topCount)
                .ToListAsync<dynamic>();

            return topDoctors;
        }

        public async Task<List<dynamic>> GetTopServicesAsync(int topCount = 10)
        {
            var topServices = await _context.Appointments
                .Include(a => a.Service)
                .GroupBy(a => new { a.ServiceId, a.Service.ServiceName, a.Service.Price })
                .Select(g => new
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    Price = g.Key.Price,
                    TotalBookings = g.Count(),
                    TotalRevenue = g.Count() * g.Key.Price
                })
                .OrderByDescending(x => x.TotalBookings)
                .Take(topCount)
                .ToListAsync<dynamic>();

            return topServices;
        }

        public async Task<Dictionary<string, int>> GetAppointmentsByDepartmentAsync()
        {
            var departmentStats = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                        .ThenInclude(u => u.UserGroups)
                            .ThenInclude(ug => ug.Group)
                .SelectMany(a => a.Doctor.User.UserGroups.Select(ug => ug.Group.GroupName))
                .GroupBy(groupName => groupName)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Department, x => x.Count);

            return departmentStats;
        }

        public async Task<int> GetTotalPatientsAsync()
        {
            return await _context.Patients.CountAsync();
        }

        public async Task<int> GetTotalAppointmentsAsync(DateTime? date = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (date.HasValue)
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);

            return await query.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Payments
                .Where(p => p.Status == "Completed");

            if (startDate.HasValue)
                query = query.Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PaidAt.HasValue && p.PaidAt.Value <= endDate.Value);

            return await query.SumAsync(p => p.Amount ?? 0);
        }
    }
}
