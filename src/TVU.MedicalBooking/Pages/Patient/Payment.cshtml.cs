using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Pages.Patient
{
    [Authorize(Roles = "Patient")]
    public class PaymentModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public PaymentModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public Payment Payment { get; set; }

        [BindProperty]
        public int PaymentId { get; set; }

        [BindProperty]
        public string TransactionId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Payment = await _paymentService.GetPaymentByIdAsync(id);

            if (Payment == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var success = await _paymentService.ConfirmPaymentAsync(
                PaymentId,
                TransactionId ?? $"TXN{PaymentId}{DateTime.Now.Ticks}");

            if (success)
            {
                TempData["SuccessMessage"] = "Xác nhận thanh toán thành công!";
                return RedirectToPage("/Patient/MyAppointments");
            }

            TempData["ErrorMessage"] = "Không thể xác nhận thanh toán. Vui lòng thử lại.";
            return RedirectToPage(new { id = PaymentId });
        }
    }
}
