namespace SporSalonuTakip.Entities
{
    internal class UserRecord : AppUser
    {
        public string PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int PasswordIterations { get; set; }
        public bool IsLegacyHash { get; set; }
        public bool IsActive { get; set; }
    }
}
