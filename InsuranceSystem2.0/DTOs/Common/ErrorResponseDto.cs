namespace InsuranceSystem2._0.DTOs.Common
{
    public class ErrorResponseDto
    {
        public DateTime Timestamp { get; set; }

        public int StatusCode { get; set; }

        public string ErrorType { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string RequestPath { get; set; } = string.Empty;
    }
}
