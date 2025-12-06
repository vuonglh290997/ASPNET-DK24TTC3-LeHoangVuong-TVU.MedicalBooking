using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Patient
{
    [Authorize(Roles = "Patient")]
    public class BookAppointmentModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;
        private readonly IPaymentService _paymentService;

        public BookAppointmentModel(
            IAuthService authService,
            IUserService userService,
            IAppointmentService appointmentService,
            IPaymentService paymentService)
        {
            _authService = authService;
            _userService = userService;
            _appointmentService = appointmentService;
            _paymentService = paymentService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<Service> Services { get; set; }
        public List<Doctor> Doctors { get; set; }
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng chọn dịch vụ")]
            [Display(Name = "Dịch vụ")]
            public int ServiceId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
            [Display(Name = "Bác sĩ")]
            public int DoctorId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
            [Display(Name = "Ngày khám")]
            public DateTime AppointmentDate { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn giờ khám")]
            [Display(Name = "Giờ khám")]
            public string TimeSlot { get; set; }

            [Display(Name = "Lý do khám")]
            [StringLength(500, ErrorMessage = "Lý do khám tối đa 500 ký tự")]
            public string Reason { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadData();
                return Page();
            }

            var currentUser = await _authService.GetCurrentUserAsync();
            var patient = await _userService.GetPatientByUserIdAsync(currentUser.UserId);

            if (patient == null)
            {
                ErrorMessage = "Vui lòng hoàn thiện thông tin bệnh nhân trước";
                await LoadData();
                return Page();
            }

            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = Input.DoctorId,
                ServiceId = Input.ServiceId,
                AppointmentDate = Input.AppointmentDate,
                TimeSlot = Input.TimeSlot,
                Reason = Input.Reason
            };

            var result = await _appointmentService.CreateAppointmentAsync(appointment);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                await LoadData();
                return Page();
            }

            // Create payment
            var paymentResult = await _paymentService.CreatePaymentAsync(result.AppointmentId);

            if (paymentResult.Success)
            {
                return RedirectToPage("/Patient/Payment", new { id = paymentResult.Payment.PaymentId });
            }

            return RedirectToPage("/Patient/MyAppointments");
        }

        // AJAX handler for available time slots
        public async Task<JsonResult> OnGetAvailableTimeSlotsAsync(int doctorId, DateTime date)
        {
            var timeSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, date);
            return new JsonResult(timeSlots);
        }

        private async Task LoadData()
        {
            Services = await _appointmentService.GetAllServicesAsync();

            // Get all doctors - you might want to filter by service/specialty
            var allUsers = await _userService.GetUsersByRoleAsync("Doctor");
            Doctors = new List<Doctor>();

            foreach (var user in allUsers)
            {
                var doctor = await _userService.GetDoctorByUserIdAsync(user.UserId);
                if (doctor != null)
                {
                    Doctors.Add(doctor);
                }
            }
        }
    }
}
