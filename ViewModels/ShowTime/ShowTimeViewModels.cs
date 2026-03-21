using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaManagement.ViewModels;

public class ShowTimeIndexViewModel
{
    public Guid ShowTimeId { get; set; }
    public string MovieTitle { get; set; } = null!;
    public string CinemaName { get; set; } = null!;
    public string RoomName { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime CleaningEndsAt { get; set; }
    public decimal BasePrice { get; set; }
    public int Status { get; set; }
    public int DisplayStatus { get; set; }
    public int? AgeRating { get; set; }
}

public class ShowTimeListViewModel
{
    public List<ShowTimeIndexViewModel> ShowTimes { get; set; } = new();
    public int UpcomingCount { get; set; }
    public int NowShowingCount { get; set; }
    public int CancelledCount { get; set; }
    public int TodayCount { get; set; }
    
    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 10;

    // Active filter values (for pre-filling the filter bar on reload)
    public string? SearchTerm { get; set; }
    public DateTime? DateFilter { get; set; }
    public Guid? CinemaIdFilter { get; set; }
    public int? StatusFilter { get; set; }
    public int? DisplayStatusFilter { get; set; }

    // For integrated Create Modal
    public ShowTimeCreateViewModel CreateForm { get; set; } = new();
    public Dictionary<string, int> MovieDurations { get; set; } = new();
    public Dictionary<string, string> RoomLocations { get; set; } = new();
}

public class ShowTimeCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn phim")]
    public Guid MovieId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phòng")]
    public Guid RoomId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
    [Display(Name = "Thời gian bắt đầu")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá vé")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá vé phải lớn hơn hoặc bằng 0")]
    public decimal BasePrice { get; set; }

    public SelectList? Movies { get; set; }
    public SelectList? Rooms { get; set; }
}

public class ShowTimeEditViewModel
{
    [Required]
    public Guid ShowTimeId { get; set; }

    public Guid MovieId { get; set; }
    public Guid RoomId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
    [Display(Name = "Thời gian bắt đầu")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá vé")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá vé phải lớn hơn hoặc bằng 0")]
    public decimal BasePrice { get; set; }

    // Display-only properties (made nullable to avoid validation errors on POST)
    public string? MovieTitle { get; set; }
    public int MovieDuration { get; set; }
    public string? RoomName { get; set; }
    public string? CinemaName { get; set; }
}

public class ShowTimeDetailViewModel
{
    public Guid ShowTimeId { get; set; }

    // Movie info
    public string MovieTitle { get; set; } = null!;
    public int MovieDuration { get; set; }
    public int? AgeRating { get; set; }

    // Location
    public string RoomName { get; set; } = null!;
    public string CinemaName { get; set; } = null!;

    // Time
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime CleaningEndsAt { get; set; }

    // Created
    public DateTime CreatedAt { get; set; }

    // Pricing
    public decimal BasePrice { get; set; }
    public decimal VipPrice => BasePrice + 20_000m;
    public decimal CouplePrice => BasePrice + 50_000m;

    // Status
    public int Status { get; set; }
    public int DisplayStatus { get; set; }
    public bool IsActive => Status == 1;
}
