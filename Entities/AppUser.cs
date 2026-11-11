namespace SporSalonuTakip.Entities
{
    public class AppUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsAdmin { get { return Role == "Admin"; } }
    }
}
