
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TVU.MedicalBooking.Data;
using TVU.MedicalBooking.Interfaces;
using TVU.MedicalBooking.Models;
namespace TVU.MedicalBooking.Services
{
        public class AuthService : IAuthService
        {
            private readonly ApplicationDbContext _context;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IAuditService _auditService;

            public AuthService(
                ApplicationDbContext context,
                IHttpContextAccessor httpContextAccessor,
                IAuditService auditService)
            {
                _context = context;
                _httpContextAccessor = httpContextAccessor;
                _auditService = auditService;
            }

            // ============================================
            // METHOD: Login
            // Xác thực người dùng và tạo cookie authentication
            // ============================================
            public async Task<(bool Success, string Message, User User)> LoginAsync(string username, string password)
            {
                try
                {
                    // Tìm user trong database, include Role và Permissions
                    var user = await _context.Users
                        .Include(u => u.Role)
                            .ThenInclude(r => r.RolePermissions)
                                .ThenInclude(rp => rp.Permission)
                        .FirstOrDefaultAsync(u => u.Username == username);

                    // Kiểm tra user có tồn tại không
                    if (user == null)
                    {
                        return (false, "Tên đăng nhập không tồn tại", null);
                    }

                    // Kiểm tra user có bị khóa không
                    if (!user.IsActive)
                    {
                        return (false, "Tài khoản đã bị khóa", null);
                    }

                    // Verify password
                    if (!VerifyPassword(password, user.PasswordHash))
                    {
                        return (false, "Mật khẩu không đúng", null);
                    }

                    // ============================================
                    // TẠO CLAIMS - Thông tin người dùng trong cookie
                    // ============================================
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName),
                    new Claim("FullName", user.FullName)
                };

                    // Thêm permissions vào claims
                    foreach (var rolePermission in user.Role.RolePermissions)
                    {
                        claims.Add(new Claim("Permission", rolePermission.Permission.PermissionName));
                    }

                    // ============================================
                    // TẠO COOKIE AUTHENTICATION
                    // ============================================
                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,                           // Remember me
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // Cookie expires in 7 days
                    };

                    // Sign in user (tạo cookie)
                    await _httpContextAccessor.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Ghi log audit
                    await _auditService.LogActionAsync(
                        user.UserId,
                        "Login",
                        "User",
                        user.UserId,
                        $"User {user.Username} logged in successfully");

                    return (true, "Đăng nhập thành công", user);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi: {ex.Message}", null);
                }
            }
        // ============================================
        // METHOD: Register
        // Đăng ký tài khoản mới
        // ============================================
        public async Task<(bool Success, string Message)> RegisterAsync(
                User user,
                string password,
                string roleName = "Patient")
            {
                try
                {
                    // Kiểm tra username đã tồn tại chưa
                    if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                    {
                        return (false, "Tên đăng nhập đã tồn tại");
                    }

                    // Kiểm tra email đã được sử dụng chưa
                    if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                    {
                        return (false, "Email đã được sử dụng");
                    }

                    // Tìm role theo tên
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                    if (role == null)
                    {
                        return (false, "Vai trò không hợp lệ");
                    }

                    // Hash password và set thông tin user
                    user.PasswordHash = HashPassword(password);
                    user.RoleId = role.RoleId;
                    user.IsActive = true;
                    user.CreatedAt = DateTime.Now;

                    // Lưu user vào database
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Nếu đăng ký với role Patient, tự động tạo record Patient
                    if (roleName == "Patient")
                    {
                        var patient = new Patient
                        {
                            UserId = user.UserId,
                            IdentityCard = "",
                            DateOfBirth = DateTime.Now.AddYears(-20),
                            Gender = "Khác",
                            Address = ""
                        };
                        _context.Patients.Add(patient);
                        await _context.SaveChangesAsync();
                    }

                    // Ghi log audit
                    await _auditService.LogActionAsync(
                        user.UserId,
                        "Register",
                        "User",
                        user.UserId,
                        $"New user {user.Username} registered");

                    return (true, "Đăng ký thành công");
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi: {ex.Message}");
                }
            }

            // ============================================
            // METHOD: Logout
            // Đăng xuất và xóa cookie
            // ============================================
            public async Task<bool> LogoutAsync()
            {
                try
                {
                    var userId = GetCurrentUserId();

                    // Sign out (xóa cookie)
                    await _httpContextAccessor.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    // Ghi log audit
                    if (userId.HasValue)
                    {
                        await _auditService.LogActionAsync(
                            userId.Value,
                            "Logout",
                            "User",
                            userId.Value,
                            "User logged out");
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // ============================================
            // METHOD: Change Password
            // Đổi mật khẩu người dùng
            // ============================================
            public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
            {
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) return false;

                    // Verify mật khẩu cũ
                    if (!VerifyPassword(oldPassword, user.PasswordHash))
                    {
                        return false;
                    }

                    // Hash và lưu mật khẩu mới
                    user.PasswordHash = HashPassword(newPassword);
                    await _context.SaveChangesAsync();

                    // Ghi log audit
                    await _auditService.LogActionAsync(
                        userId,
                        "ChangePassword",
                        "User",
                        userId,
                        "Password changed successfully");

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // ============================================
            // METHOD: Get Current User
            // Lấy thông tin user đang đăng nhập
            // ============================================
            public async Task<User> GetCurrentUserAsync()
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return null;

                return await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId.Value);
            }

            // ============================================
            // METHOD: Get User Permissions
            // Lấy danh sách permissions của user
            // ============================================
            public async Task<List<string>> GetUserPermissionsAsync(int userId)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return new List<string>();

                return user.Role.RolePermissions
                    .Select(rp => rp.Permission.PermissionName)
                    .ToList();
            }

            // ============================================
            // METHOD: Has Permission
            // Kiểm tra user có permission cụ thể không
            // ============================================
            public async Task<bool> HasPermissionAsync(int userId, string permissionName)
            {
                var permissions = await GetUserPermissionsAsync(userId);
                return permissions.Contains(permissionName);
            }

            // ============================================
            // PRIVATE HELPER METHODS
            // ============================================

            // Lấy UserId từ claims trong HttpContext
            private int? GetCurrentUserId()
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                return null;
            }

            // Hash password bằng SHA256
            private string HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(hashedBytes);
                }
            }

            // Verify password với hash đã lưu
            private bool VerifyPassword(string password, string hashedPassword)
            {
                var hashOfInput = HashPassword(password);
                return hashOfInput == hashedPassword;
            }
        }
}
