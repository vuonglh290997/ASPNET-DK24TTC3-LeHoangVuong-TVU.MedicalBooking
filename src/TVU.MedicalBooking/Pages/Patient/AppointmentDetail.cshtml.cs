using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Patient
{
    [Authorize(Roles = "Patient,Doctor,Admin")]
    public class AppointmentDetailModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;

        public AppointmentDetailModel(
            IAuthService authService,
            IUserService userService,
            IAppointmentService appointmentService)
        {
            _authService = authService;
            _userService = userService;
            _appointmentService = appointmentService;
        }

        public Appointment Appointment { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Lấy thông tin lịch hẹn
            Appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (Appointment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            var currentUser = await _authService.GetCurrentUserAsync();

            if (currentUser.Role.RoleName == "Patient")
            {
                // Patient chỉ xem được lịch hẹn của mình
                var patient = await _userService.GetPatientByUserIdAsync(currentUser.UserId);

                if (patient == null || Appointment.PatientId != patient.PatientId)
                {
                    return Forbid(); // 403 Forbidden
                }
            }
            else if (currentUser.Role.RoleName == "Doctor")
            {
                // Doctor chỉ xem được lịch hẹn của mình
                var doctor = await _userService.GetDoctorByUserIdAsync(currentUser.UserId);

                if (doctor == null || Appointment.DoctorId != doctor.DoctorId)
                {
                    return Forbid();
                }
            }
            // Admin có thể xem tất cả

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do hủy";
                return RedirectToPage(new { id });
            }

            // Kiểm tra quyền hủy
            var currentUser = await _authService.GetCurrentUserAsync();
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Chỉ cho phép hủy nếu là Patient của lịch hẹn đó
            if (currentUser.Role.RoleName == "Patient")
            {
                var patient = await _userService.GetPatientByUserIdAsync(currentUser.UserId);

                if (patient == null || appointment.PatientId != patient.PatientId)
                {
                    return Forbid();
                }
            }

            // Chỉ cho phép hủy nếu status là Pending hoặc Approved
            if (appointment.Status != "Pending" && appointment.Status != "Approved")
            {
                TempData["ErrorMessage"] = "Không thể hủy lịch hẹn ở trạng thái này";
                return RedirectToPage(new { id });
            }

            var success = await _appointmentService.CancelAppointmentAsync(id, reason);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã hủy lịch hẹn thành công";
                return RedirectToPage("/Patient/MyAppointments");
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy lịch hẹn";
                return RedirectToPage(new { id });
            }
        }
    }
}
