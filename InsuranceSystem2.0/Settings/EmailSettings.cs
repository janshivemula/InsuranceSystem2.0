namespace InsuranceSystem2._0.Settings
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 2525;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "noreply@insurancesystem.com";
        public string FromName { get; set; } = "Insurance System";
        public bool EnableSsl { get; set; } = true;
    }
}
