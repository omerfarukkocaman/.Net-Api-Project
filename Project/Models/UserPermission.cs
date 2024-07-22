using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class UserPermission
    {
        public int Id { get; set; } 
        public string PermissionName { get; set; }
        [ForeignKey("user")]
        public int UserId { get; set; }
        public User user { get; set; }
    }
}
