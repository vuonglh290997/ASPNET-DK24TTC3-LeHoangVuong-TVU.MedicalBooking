using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
            [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập từ 4-50 ký tự")]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            [Display(Name = "Xác nhận mật khẩu")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new User
            {
                Username = Input.Username,
                FullName = Input.FullName,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber
            };

            var result = await _authService.RegisterAsync(user, Input.Password, "Patient");

            if (result.Success)
            {
                return RedirectToPage("/Account/Login", new { message = "Đăng ký thành công! Vui lòng đăng nhập." });
            }

            ErrorMessage = result.Message;
            return Page();
        }
    }
}
