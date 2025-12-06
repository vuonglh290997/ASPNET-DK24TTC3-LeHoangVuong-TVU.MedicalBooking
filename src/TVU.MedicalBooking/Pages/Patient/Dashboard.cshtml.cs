using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Patient
{
    [Authorize(Roles = "Patient")]
    public class DashboardModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;

        public DashboardModel(
            IAuthService authService,
            IUserService userService,
            IAppointmentService appointmentService)
        {
            _authService = authService;
            _userService = userService;
            _appointmentService = appointmentService;
        }

        public User CurrentUser { get; set; }
        public MedicalBooking.Models.Patient PatientInfo { get; set; }
        public List<Appointment> RecentAppointments { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int CompletedAppointments { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUser = await _authService.GetCurrentUserAsync();
            if (CurrentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            PatientInfo = await _userService.GetPatientByUserIdAsync(CurrentUser.UserId);
            if (PatientInfo == null)
            {
                return RedirectToPage("/Patient/CompleteProfile");
            }

            var allAppointments = await _appointmentService.GetAppointmentsByPatientAsync(PatientInfo.PatientId);

            TotalAppointments = allAppointments.Count;
            PendingAppointments = allAppointments.Count(a => a.Status == "Pending");
            ApprovedAppointments = allAppointments.Count(a => a.Status == "Approved");
            CompletedAppointments = allAppointments.Count(a => a.Status == "Completed");

            RecentAppointments = allAppointments.Take(5).ToList();

            return Page();
        }
    }
}
