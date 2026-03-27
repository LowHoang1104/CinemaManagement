using CinemaManagement.Models;

namespace CinemaManagement.ViewModels.Cinema
{
    public class CinemaIndexViewModel
    {
        public IEnumerable<Models.Cinema> Cinemas { get; set; } = new List<Models.Cinema>();
        public CinemaStatsViewModel Stats { get; set; } = new CinemaStatsViewModel();
        
        // Filter & Paging State
        public string SearchKeyword { get; set; } = string.Empty;
        public int? StatusFilter { get; set; }
        public string SortBy { get; set; } = string.Empty;
        public string SortDir { get; set; } = "asc";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
