using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;

namespace TVU.MedicalBooking.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }
        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _authService.LogoutAsync();

            if (result)
            {
                    return RedirectToPage("/Index");
            }
            return Page();
        }
    }
}
