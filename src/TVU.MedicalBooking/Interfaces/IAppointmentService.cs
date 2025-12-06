using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Interfaces
{
    public interface IAppointmentService
    {
        Task<(bool Success, string Message, int AppointmentId)> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
        Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId);
        Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId, DateTime? date = null);
        Task<List<Appointment>> GetPendingAppointmentsAsync();
        Task<List<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> ApproveAppointmentAsync(int appointmentId, int approvedBy);
        Task<bool> RejectAppointmentAsync(int appointmentId, int rejectedBy, string reason);
        Task<bool> CompleteAppointmentAsync(int appointmentId);
        Task<bool> CancelAppointmentAsync(int appointmentId, string reason);
        Task<List<string>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date);
        Task<List<Service>> GetAllServicesAsync();
        Task<Service> GetServiceByIdAsync(int serviceId);
    }
}
