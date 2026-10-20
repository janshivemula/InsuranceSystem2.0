namespace InsuranceSystem2._0.DTOs.Common
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Records { get; set; } = new List<T>();

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public bool IsLastPage { get; set; }

        public string SortField { get; set; } = string.Empty;

        public string SortDirection { get; set; } = string.Empty;
    }
}
