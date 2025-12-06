using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IReportService _reportService;
        private readonly IAuditService _auditService;

        public DashboardModel(IReportService reportService, IAuditService auditService)
        {
            _reportService = reportService;
            _auditService = auditService;
        }

        public int TotalPatients { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public Dictionary<string, int> AppointmentStats { get; set; }
        public List<dynamic> TopServices { get; set; }
        public List<dynamic> TopDoctors { get; set; }
        public List<AuditLog> RecentLogs { get; set; }

        public async Task OnGetAsync()
        {
            // Get overview stats
            TotalPatients = await _reportService.GetTotalPatientsAsync();
            TodayAppointments = await _reportService.GetTotalAppointmentsAsync(DateTime.Today);

            var stats = await _reportService.GetAppointmentStatsByStatusAsync();
            PendingAppointments = stats.ContainsKey("Pending") ? stats["Pending"] : 0;
            AppointmentStats = stats;

            // Get monthly revenue
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            MonthlyRevenue = await _reportService.GetTotalRevenueAsync(startOfMonth, endOfMonth);

            // Get top services and doctors
            TopServices = await _reportService.GetTopServicesAsync(5);
            TopDoctors = await _reportService.GetTopDoctorsByAppointmentsAsync(5);

            // Get recent activity logs
            RecentLogs = await _auditService.GetRecentLogsAsync(10);
        }
    }
}
