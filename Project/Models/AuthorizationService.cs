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


        
    }
}
