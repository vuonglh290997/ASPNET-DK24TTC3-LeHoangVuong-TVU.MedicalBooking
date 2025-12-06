using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UsersModel(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        public List<User> Users { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FilterRole { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FilterStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            // Get all users
            var allUsers = await _userService.GetAllUsersAsync();

            // Apply filters
            Users = allUsers;

            if (!string.IsNullOrEmpty(FilterRole))
            {
                Users = Users.Where(u => u.Role.RoleName == FilterRole).ToList();
            }

            if (!string.IsNullOrEmpty(FilterStatus))
            {
                bool isActive = FilterStatus == "active";
                Users = Users.Where(u => u.IsActive == isActive).ToList();
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Users = Users.Where(u =>
                    u.Username.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(SearchTerm))
                ).ToList();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string Username,
            string FullName,
            string Email,
            string PhoneNumber,
            string Password,
            string RoleName)
        {
            try
            {
                var user = new User
                {
                    Username = Username,
                    FullName = FullName,
                    Email = Email,
                    PhoneNumber = PhoneNumber
                };

                var result = await _authService.RegisterAsync(user, Password, RoleName);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int userId, bool isActive)
        {
            try
            {
                var success = await _userService.ActivateUserAsync(userId, isActive);

                if (success)
                {
                    return new JsonResult(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }

            return new JsonResult(new { success = false });
        }
    }
}
