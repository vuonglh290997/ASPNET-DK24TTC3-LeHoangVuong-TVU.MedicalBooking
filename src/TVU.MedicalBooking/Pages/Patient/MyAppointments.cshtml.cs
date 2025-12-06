using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Patient
{
    [Authorize(Roles = "Patient")]
    public class MyAppointmentsModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;

        public MyAppointmentsModel(
            IAuthService authService,
            IUserService userService,
            IAppointmentService appointmentService)
        {
            _authService = authService;
            _userService = userService;
            _appointmentService = appointmentService;
        }

        public List<Appointment> Appointments { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FilterStatus { get; set; }

        public int AllCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var patient = await _userService.GetPatientByUserIdAsync(currentUser.UserId);
            if (patient == null)
            {
                return RedirectToPage("/Patient/CompleteProfile");
            }

            // Get all appointments
            var allAppointments = await _appointmentService.GetAppointmentsByPatientAsync(patient.PatientId);

            // Calculate counts
            AllCount = allAppointments.Count;
            PendingCount = allAppointments.Count(a => a.Status == "Pending");
            ApprovedCount = allAppointments.Count(a => a.Status == "Approved");
            CompletedCount = allAppointments.Count(a => a.Status == "Completed");
            CancelledCount = allAppointments.Count(a => a.Status == "Cancelled");

            // Apply filter
            if (!string.IsNullOrEmpty(FilterStatus) && FilterStatus != "All")
            {
                Appointments = allAppointments.Where(a => a.Status == FilterStatus).ToList();
            }
            else
            {
                Appointments = allAppointments;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int appointmentId, string reason)
        {
            var success = await _appointmentService.CancelAppointmentAsync(appointmentId, reason);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã hủy lịch hẹn thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy lịch hẹn";
            }

            return RedirectToPage();
        }
    }
}
