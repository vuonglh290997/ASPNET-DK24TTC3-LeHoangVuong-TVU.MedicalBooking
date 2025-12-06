using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TVU.MedicalBooking.Interfaces;

namespace TVU.MedicalBooking.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Username, Input.Password);

            if (result.Success)
            {
                // Redirect based on role
                if (result.User.Role.RoleName == "Admin")
                {
                    return RedirectToPage("/Admin/Dashboard");
                }
                else if (result.User.Role.RoleName == "Doctor")
                {
                    return RedirectToPage("/Doctors/Dashboard");
                }
                else if (result.User.Role.RoleName == "Patient")
                {
                    return RedirectToPage("/Patient/Dashboard");
                }

                return RedirectToPage("/Index");
            }

            ErrorMessage = result.Message;
            return Page();
        }
    }
}
