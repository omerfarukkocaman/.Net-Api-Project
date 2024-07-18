using Microsoft.EntityFrameworkCore;
using Project.Data;

namespace Project.Models
{
    public class AuthorizationService
    {
        private readonly ApplicationDbContext _context;

        public AuthorizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermission(int userId, string permissionName)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            return user.UserPermissions.Any(up => up.PermissionName == permissionName);
        }
    }
}
