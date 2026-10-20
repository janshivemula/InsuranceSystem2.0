namespace InsuranceSystem2._0.DTOs.Claim
{
    public class ClaimDocumentResponseDto
    {
        public int DocumentId { get; set; }
        public int ClaimId { get; set; }
        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }
    }
}
