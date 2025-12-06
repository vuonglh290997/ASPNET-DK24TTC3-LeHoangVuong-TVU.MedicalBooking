namespace TVU.MedicalBooking.Interfaces
{
    public interface IReportService
    {
        Task<Dictionary<string, int>> GetAppointmentStatsByStatusAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, decimal>> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<List<dynamic>> GetTopDoctorsByAppointmentsAsync(int topCount = 10);
        Task<List<dynamic>> GetTopServicesAsync(int topCount = 10);
        Task<Dictionary<string, int>> GetAppointmentsByDepartmentAsync();
        Task<int> GetTotalPatientsAsync();
        Task<int> GetTotalAppointmentsAsync(DateTime? date = null);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
