namespace InsuranceSystem2._0.DTOs.Auth
{
    public class OtpSentResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime OtpExpiresAt { get; set; }
        public DateTime ResendAvailableAt { get; set; }
    }
}
