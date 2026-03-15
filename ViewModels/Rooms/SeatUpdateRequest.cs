using CinemaManagement.Models;

namespace CinemaManagement.ViewModels.Rooms
{
    public class SeatUpdateRequest
    {
        public Guid Id { get; set; }
        public SeatTypeEnum SeatType { get; set; }
        public Guid SeatStatusId { get; set; }
    }
}
