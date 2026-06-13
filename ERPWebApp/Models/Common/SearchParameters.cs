#nullable enable
namespace ERPWebApp.Models.Common
{
    public class SearchParameters
    {
        public string? SearchValue { get; set; }
        public List<string>? SearchColumns { get; set; }
        public List<QueryFilter>? Filters { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
        public int? Start { get; set; } = 0;
        public int? PageSize { get; set; } = 10;
        public TimeZoneInfo? UserTimeZone { get; set; }
    }
}
