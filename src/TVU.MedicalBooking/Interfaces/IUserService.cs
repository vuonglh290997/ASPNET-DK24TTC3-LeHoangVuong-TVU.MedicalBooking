using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(string roleName);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId, bool isActive);
        Task<bool> AssignUserToGroupAsync(int userId, int groupId);
        Task<bool> RemoveUserFromGroupAsync(int userId, int groupId);
        Task<List<Group>> GetUserGroupsAsync(int userId);
        Task<Patient> GetPatientByUserIdAsync(int userId);
        Task<Doctor> GetDoctorByUserIdAsync(int userId);
        Task<bool> UpdatePatientInfoAsync(Patient patient);
        Task<bool> UpdateDoctorInfoAsync(Doctor doctor);
    }
}
