using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TVU.MedicalBooking.Interfaces;

namespace TVU.MedicalBooking.Pages.Patient
{
    [Authorize(Roles = "Patient")]
    public class CompleteProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public CompleteProfileModel(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ tên đầy đủ")]
            [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số CMND/CCCD")]
            [StringLength(20, MinimumLength = 9, ErrorMessage = "Số CMND/CCCD từ 9-20 ký tự")]
            [Display(Name = "Số CMND/CCCD")]
            public string IdentityCard { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
            [Display(Name = "Ngày sinh")]
            public DateTime DateOfBirth { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn giới tính")]
            [Display(Name = "Giới tính")]
            public string Gender { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
            [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
            [Display(Name = "Địa chỉ")]
            public string Address { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [StringLength(100, ErrorMessage = "Tên người liên hệ tối đa 100 ký tự")]
            [Display(Name = "Người liên hệ khẩn cấp")]
            public string EmergencyContact { get; set; }

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại khẩn cấp")]
            public string EmergencyPhone { get; set; }

            [StringLength(500, ErrorMessage = "Tiền sử bệnh tối đa 500 ký tự")]
            [Display(Name = "Tiền sử bệnh")]
            public string MedicalHistory { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Kiểm tra xem patient đã có thông tin chưa
            var patient = await _userService.GetPatientByUserIdAsync(currentUser.UserId);

            if (patient != null && !string.IsNullOrEmpty(patient.IdentityCard))
            {
                // Đã có thông tin, load để edit
                Input = new InputModel
                {
                    FullName = currentUser.FullName,
                    IdentityCard = patient.IdentityCard,
                    DateOfBirth = patient.DateOfBirth ?? DateTime.MinValue,
                    Gender = patient.Gender,
                    Address = patient.Address,
                    PhoneNumber = currentUser.PhoneNumber,
                    Email = currentUser.Email,
                    EmergencyContact = patient.EmergencyContact,
                    EmergencyPhone = patient.EmergencyPhone,
                    MedicalHistory = patient.MedicalHistory
                };
            }
            else
            {
                // Chưa có thông tin, pre-fill từ user
                Input = new InputModel
                {
                    FullName = currentUser.FullName,
                    Email = currentUser.Email,
                    PhoneNumber = currentUser.PhoneNumber,
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    Gender = "Nam"
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                // Validate tuổi hợp lệ
                var age = DateTime.Now.Year - Input.DateOfBirth.Year;
                if (age < 0 || age > 150)
                {
                    ErrorMessage = "Ngày sinh không hợp lệ";
                    return Page();
                }

                // Update thông tin User
                currentUser.FullName = Input.FullName;
                currentUser.PhoneNumber = Input.PhoneNumber;
                currentUser.Email = Input.Email;

                var updateUserSuccess = await _userService.UpdateUserAsync(currentUser);
                if (!updateUserSuccess)
                {
                    ErrorMessage = "Không thể cập nhật thông tin người dùng";
                    return Page();
                }

                // Update hoặc tạo mới Patient
                var patient = await _userService.GetPatientByUserIdAsync(currentUser.UserId);

                if (patient == null)
                {
                    // Tạo mới Patient record
                    patient = new MedicalBooking.Models.Patient
                    {
                        UserId = currentUser.UserId
                    };
                }

                // Kiểm tra CMND đã được sử dụng chưa (trừ chính user này)
                var existingPatient = await _userService.GetPatientByUserIdAsync(currentUser.UserId);
                if (existingPatient != null &&
                    existingPatient.IdentityCard != Input.IdentityCard &&
                    await IsIdentityCardUsedAsync(Input.IdentityCard))
                {
                    ErrorMessage = "Số CMND/CCCD này đã được sử dụng bởi tài khoản khác";
                    return Page();
                }

                // Update patient info
                patient.IdentityCard = Input.IdentityCard;
                patient.DateOfBirth = Input.DateOfBirth;
                patient.Gender = Input.Gender;
                patient.Address = Input.Address;
                patient.EmergencyContact = Input.EmergencyContact;
                patient.EmergencyPhone = Input.EmergencyPhone;
                patient.MedicalHistory = Input.MedicalHistory;

                var updatePatientSuccess = await _userService.UpdatePatientInfoAsync(patient);

                if (updatePatientSuccess)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToPage("/Patient/Dashboard");
                }
                else
                {
                    ErrorMessage = "Không thể cập nhật thông tin bệnh nhân";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
                return Page();
            }
        }

        private async Task<bool> IsIdentityCardUsedAsync(string identityCard)
        {
            // TODO: Implement check if identity card is already used
            // Cần thêm method vào UserService
            return false;
        }
    }
}
