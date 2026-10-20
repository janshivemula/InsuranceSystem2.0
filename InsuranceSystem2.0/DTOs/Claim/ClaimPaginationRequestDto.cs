using InsuranceSystem2._0.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Claim
{
    public class ClaimPaginationRequestDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "CreatedDate";

        public string SortDirection { get; set; } = "desc";

        // Filtering
        public ClaimStatus? ClaimStatus { get; set; }

        public int? CustomerId { get; set; }

        public int? PolicyId { get; set; }
    }
}
