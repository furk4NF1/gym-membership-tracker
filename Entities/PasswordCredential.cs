namespace SporSalonuTakip.Entities
{
    internal class PasswordCredential
    {
        public string Hash { get; set; }
        public byte[] Salt { get; set; }
        public int Iterations { get; set; }
    }
}
