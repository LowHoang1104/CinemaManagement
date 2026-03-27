using System;
using System.Collections.Generic;
using CinemaManagement.Models;

namespace CinemaManagement.ViewModels.Rooms
{
    public class RoomListViewModel
    {
        public List<Room> Rooms { get; set; } = new();
        
        // Stats
        public int TotalRooms { get; set; }
        public int TotalActiveRooms { get; set; }
        public int TotalInactiveRooms { get; set; }
        public int TotalSeats { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;

        // Filter/Sort State
        public string? SearchTerm { get; set; }
        public Guid? CinemaIdFilter { get; set; }
        public RoomStatus? StatusFilter { get; set; }
        public string? SortBy { get; set; }
        public string? SortDir { get; set; }
    }
}
