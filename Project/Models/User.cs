namespace Project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
        public int? Age { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public UserType? UserType { get; set; }
        public List<UserPermission>? UserPermissions { get; set; }
    }
    public enum UserType
    {
        Admin,
        User
    }
}