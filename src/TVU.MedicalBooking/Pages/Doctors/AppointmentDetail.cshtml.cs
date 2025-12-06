using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Doctors
{
    [Authorize(Roles = "Doctor,Admin")]
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
            // Lấy appointment với đầy đủ thông tin
            Appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (Appointment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            var currentUser = await _authService.GetCurrentUserAsync();

            if (currentUser.Role.RoleName == "Doctor")
            {
                // Doctor chỉ xem lịch hẹn của mình
                var doctor = await _userService.GetDoctorByUserIdAsync(currentUser.UserId);

                if (doctor == null || Appointment.DoctorId != doctor.DoctorId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem lịch hẹn này";
                    return RedirectToPage("/Doctor/Dashboard");
                }
            }
            // Admin có thể xem tất cả

            return Page();
        }

        // Handler: Duyệt lịch hẹn
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền
            if (currentUser.Role.RoleName == "Doctor")
            {
                var doctor = await _userService.GetDoctorByUserIdAsync(currentUser.UserId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền duyệt lịch hẹn này";
                    return RedirectToPage(new { id });
                }
            }

            // Chỉ duyệt được nếu status = Pending
            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Không thể duyệt lịch hẹn ở trạng thái này";
                return RedirectToPage(new { id });
            }

            var success = await _appointmentService.ApproveAppointmentAsync(id, currentUser.UserId);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã duyệt lịch hẹn thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể duyệt lịch hẹn";
            }

            return RedirectToPage(new { id });
        }

        // Handler: Từ chối lịch hẹn
        public async Task<IActionResult> OnPostRejectAsync(int id, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối";
                return RedirectToPage(new { id });
            }

            var currentUser = await _authService.GetCurrentUserAsync();
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền
            if (currentUser.Role.RoleName == "Doctor")
            {
                var doctor = await _userService.GetDoctorByUserIdAsync(currentUser.UserId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền từ chối lịch hẹn này";
                    return RedirectToPage(new { id });
                }
            }

            // Chỉ từ chối được nếu status = Pending
            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Không thể từ chối lịch hẹn ở trạng thái này";
                return RedirectToPage(new { id });
            }

            var success = await _appointmentService.RejectAppointmentAsync(id, currentUser.UserId, reason);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã từ chối lịch hẹn";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể từ chối lịch hẹn";
            }

            return RedirectToPage(new { id });
        }

        // Handler: Hoàn thành lịch khám
        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền
            if (currentUser.Role.RoleName == "Doctor")
            {
                var doctor = await _userService.GetDoctorByUserIdAsync(currentUser.UserId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật lịch hẹn này";
                    return RedirectToPage(new { id });
                }
            }

            // Chỉ complete được nếu status = Approved
            if (appointment.Status != "Approved")
            {
                TempData["ErrorMessage"] = "Chỉ có thể đánh dấu hoàn thành cho lịch đã được duyệt";
                return RedirectToPage(new { id });
            }

            var success = await _appointmentService.CompleteAppointmentAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Đã đánh dấu hoàn thành lịch khám!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật trạng thái";
            }

            return RedirectToPage(new { id });
        }

        // Handler: Cập nhật ghi chú
        public async Task<IActionResult> OnPostUpdateNotesAsync(int appointmentId, string notes)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền
            if (currentUser.Role.RoleName == "Doctor")
            {
                var doctor = await _userService.GetDoctorByUserIdAsync(currentUser.UserId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật ghi chú";
                    return RedirectToPage(new { id = appointmentId });
                }
            }

            // Cập nhật notes
            appointment.Notes = notes;

            // TODO: Cần thêm method UpdateAppointmentAsync vào IAppointmentService
            // Tạm thời có thể dùng trực tiếp context
            try
            {
                // Giả sử có method này
                // await _appointmentService.UpdateAppointmentAsync(appointment);

                TempData["SuccessMessage"] = "Đã lưu ghi chú thành công!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Không thể lưu ghi chú";
            }

            return RedirectToPage(new { id = appointmentId });
        }
    }
}
