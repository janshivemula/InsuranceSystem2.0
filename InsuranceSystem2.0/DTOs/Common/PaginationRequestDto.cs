using InsuranceSystem2._0.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Common
{
    public class PaginationRequestDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "PaymentDate";

        public string SortDirection { get; set; } = "desc";

        // Filtering
        public PaymentStatus? PaymentStatus { get; set; }

        public PaymentMode? PaymentMode { get; set; }
    }
}
