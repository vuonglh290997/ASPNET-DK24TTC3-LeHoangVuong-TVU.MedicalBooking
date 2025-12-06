using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;

namespace TVU.MedicalBooking.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReportsModel : PageModel
    {
        private readonly IReportService _reportService;

        public ReportsModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = DateTime.Now;

        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePayment { get; set; }
        public Dictionary<string, int> AppointmentStats { get; set; }
        public Dictionary<string, int> DepartmentStats { get; set; }
        public List<dynamic> TopServices { get; set; }
        public List<dynamic> TopDoctors { get; set; }

        public async Task OnGetAsync()
        {
            // Get appointment statistics
            AppointmentStats = await _reportService.GetAppointmentStatsByStatusAsync(StartDate, EndDate);
            TotalAppointments = AppointmentStats.Values.Sum();
            CompletedAppointments = AppointmentStats.ContainsKey("Completed")
                ? AppointmentStats["Completed"]
                : 0;

            // Get revenue report
            var revenueReport = await _reportService.GetRevenueReportAsync(StartDate, EndDate);
            TotalRevenue = revenueReport.ContainsKey("TotalRevenue")
                ? revenueReport["TotalRevenue"]
                : 0;
            AveragePayment = revenueReport.ContainsKey("AveragePayment")
                ? revenueReport["AveragePayment"]
                : 0;

            // Get top services and doctors
            TopServices = await _reportService.GetTopServicesAsync(10);
            TopDoctors = await _reportService.GetTopDoctorsByAppointmentsAsync(10);

            // Get department statistics
            DepartmentStats = await _reportService.GetAppointmentsByDepartmentAsync();
        }
    }
}
