using CinemaManagement.Models;

namespace CinemaManagement.ViewModels.Cinema
{
    /// <summary>
    /// DTO nhận từ AJAX – chỉ chứa những field cần cập nhật
    /// </summary>
    public class SeatUpdateRequest
    {
        public Guid Id { get; set; }
        public SeatTypeEnum SeatType { get; set; }
        public Guid SeatStatusId { get; set; }
    }
}