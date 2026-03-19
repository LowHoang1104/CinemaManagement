using System;

namespace CinemaManagement.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public Func<int, string> GenerateUrl { get; set; }
    }
}
