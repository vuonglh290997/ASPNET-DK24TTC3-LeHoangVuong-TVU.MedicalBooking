using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User User)> LoginAsync(string username, string password);
        Task<(bool Success, string Message)> RegisterAsync(User user, string password, string roleName = "Patient");
        Task<bool> LogoutAsync();
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<User> GetCurrentUserAsync();
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permissionName);
    }
}
