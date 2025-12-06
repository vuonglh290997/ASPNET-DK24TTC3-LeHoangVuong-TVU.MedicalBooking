using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Interfaces
{
    public interface IPaymentService
    {
        Task<(bool Success, string Message, Payment Payment)> CreatePaymentAsync(int appointmentId);
        Task<string> GenerateQRCodeAsync(decimal amount, string content);
        Task<bool> ConfirmPaymentAsync(int paymentId, string transactionId);
        Task<Payment> GetPaymentByIdAsync(int paymentId);
        Task<Payment> GetPaymentByAppointmentIdAsync(int appointmentId);
        Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
