using Microsoft.EntityFrameworkCore;
using TVU.MedicalBooking.Data;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;

namespace TVU.MedicalBooking.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public UserService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == roleName)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActivateUserAsync(int userId, bool isActive)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.IsActive = isActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AssignUserToGroupAsync(int userId, int groupId)
        {
            try
            {
                var userGroup = new UserGroup
                {
                    UserId = userId,
                    GroupId = groupId
                };

                _context.UserGroups.Add(userGroup);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveUserFromGroupAsync(int userId, int groupId)
        {
            try
            {
                var userGroup = await _context.UserGroups
                    .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

                if (userGroup == null) return false;

                _context.UserGroups.Remove(userGroup);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Group>> GetUserGroupsAsync(int userId)
        {
            return await _context.UserGroups
                .Include(ug => ug.Group)
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.Group)
                .ToListAsync();
        }

        public async Task<Patient> GetPatientByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
            }
            catch(Exception ex) 
            {
                return null;

            }
        }

        public async Task<Doctor> GetDoctorByUserIdAsync(int userId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<bool> UpdatePatientInfoAsync(Patient patient)
        {
            try
            {
                _context.Patients.Update(patient);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDoctorInfoAsync(Doctor doctor)
        {
            try
            {
                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
