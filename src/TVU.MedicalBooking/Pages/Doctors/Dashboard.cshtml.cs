using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Doctors
{
    [Authorize(Roles = "Doctor,Admin")]
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
        public MedicalBooking.Models.Doctor DoctorInfo { get; set; }
        public int PendingAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int ThisWeekAppointments { get; set; }
        public int CompletedAppointments { get; set; }

        public List<Appointment> PendingList { get; set; }
        public List<Appointment> TodayList { get; set; }
        public List<Appointment> UpcomingList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUser = await _authService.GetCurrentUserAsync();
            if (CurrentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            DoctorInfo = await _userService.GetDoctorByUserIdAsync(CurrentUser.UserId);
            if (DoctorInfo == null)
            {
                return RedirectToPage("/Error");
            }

            // Get all appointments for this doctor
            var allAppointments = await _appointmentService.GetAppointmentsByDoctorAsync(DoctorInfo.DoctorId);

            // Calculate stats
            PendingAppointments = allAppointments.Count(a => a.Status == "Pending");

            var today = DateTime.Today;
            TodayAppointments = allAppointments.Count(a =>
                a.AppointmentDate.Date == today &&
                (a.Status == "Approved" || a.Status == "Pending"));

            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            ThisWeekAppointments = allAppointments.Count(a =>
                a.AppointmentDate >= startOfWeek &&
                a.AppointmentDate < endOfWeek &&
                (a.Status == "Approved" || a.Status == "Pending"));

            CompletedAppointments = allAppointments.Count(a => a.Status == "Completed");

            // Get lists for tabs
            PendingList = allAppointments
                .Where(a => a.Status == "Pending")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToList();

            TodayList = allAppointments
                .Where(a => a.AppointmentDate.Date == today && a.Status != "Cancelled")
                .OrderBy(a => a.TimeSlot)
                .ToList();

            UpcomingList = allAppointments
                .Where(a => a.AppointmentDate.Date > today && a.Status == "Approved")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .Take(20)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int appointmentId)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            var success = await _appointmentService.ApproveAppointmentAsync(appointmentId, currentUser.UserId);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã duyệt lịch hẹn thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể duyệt lịch hẹn";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int appointmentId, string reason)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            var success = await _appointmentService.RejectAppointmentAsync(appointmentId, currentUser.UserId, reason);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã từ chối lịch hẹn";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể từ chối lịch hẹn";
            }

            return RedirectToPage();
        }
    }
}
