using Microsoft.EntityFrameworkCore;
using System.Text;
using TVU.MedicalBooking.Data;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;

        public PaymentService(
            ApplicationDbContext context,
            IAuditService auditService,
            IConfiguration configuration)
        {
            _context = context;
            _auditService = auditService;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, Payment Payment)> CreatePaymentAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Service)
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                {
                    return (false, "Không tìm thấy lịch hẹn", null);
                }

                // Check if payment already exists
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

                if (existingPayment != null)
                {
                    return (false, "Thanh toán đã tồn tại", existingPayment);
                }

                var payment = new Payment
                {
                    AppointmentId = appointmentId,
                    Amount = appointment.Service.Price,
                    PaymentMethod = "QRCode",
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                // Generate QR code content
                var qrContent = $"Thanh toan dich vu {appointment.Service.ServiceName} - Ma lich hen: {appointmentId}";
                payment.QRCodeData = await GenerateQRCodeAsync(payment.Amount ?? 1000, qrContent);

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    appointment.Patient.UserId,
                    "CreatePayment",
                    "Payment",
                    payment.PaymentId,
                    $"Created payment for appointment #{appointmentId}");

                return (true, "Tạo thanh toán thành công", payment);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        public async Task<string> GenerateQRCodeAsync(decimal amount, string content)
        {
            try
            {
                // Get bank config from appsettings
                var bankId = _configuration["QRCodePayment:BankId"];
                var accountNo = _configuration["QRCodePayment:AccountNo"];
                var accountName = _configuration["QRCodePayment:AccountName"];
                var template = _configuration["QRCodePayment:Template"] ?? "compact";

                // Generate VietQR URL (using VietQR API standard)
                // Format: https://img.vietqr.io/image/[BANK_ID]-[ACCOUNT_NO]-[TEMPLATE].png?amount=[AMOUNT]&addInfo=[CONTENT]

                var qrUrl = new StringBuilder();
                qrUrl.Append($"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png");
                qrUrl.Append($"?amount={amount}");
                qrUrl.Append($"&addInfo={Uri.EscapeDataString(content)}");
                qrUrl.Append($"&accountName={Uri.EscapeDataString(accountName)}");

                return qrUrl.ToString();
            }
            catch (Exception ex)
            {
                return $"Error generating QR: {ex.Message}";
            }
        }

        public async Task<bool> ConfirmPaymentAsync(int paymentId, string transactionId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Appointment)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null || payment.Status == "Completed")
                    return false;

                payment.Status = "Completed";
                payment.TransactionId = transactionId;
                payment.PaidAt = DateTime.Now;

                // Update appointment status if pending
                if (payment.Appointment.Status == "Pending")
                {
                    payment.Appointment.Status = "Approved";
                    payment.Appointment.ApprovedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    payment.Appointment.PatientId,
                    "ConfirmPayment",
                    "Payment",
                    paymentId,
                    $"Payment confirmed with transaction ID: {transactionId}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Payment> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(p => p.Appointment.Patient)
                    .ThenInclude(pt => pt.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            }catch(Exception ex)
            {
                return null;
            }
            
        }

        public async Task<Payment> GetPaymentByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        }

        public async Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(p => p.Appointment.Patient)
                    .ThenInclude(pt => pt.User)
                .Where(p => p.CreatedAt.Date >= startDate.Date &&
                           p.CreatedAt.Date <= endDate.Date)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
