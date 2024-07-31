using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Security.Claims;

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
                .AsNoTracking() //AsNoTracking() fonksiyon kullanımı
                .Include(u => u.UserPermissions) // Eager Loading
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            return user.UserPermissions.Any(up => up.PermissionName == permissionName);
        }

    }
}
