using CinemaManagement.Models;

namespace CinemaManagement.ViewModels.Cinema;

public class CinemaDetailsViewModel
{
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int Status { get; set; }
    public List<RoomVm> Rooms { get; set; } = new();
}

public class RoomVm
{
    public Guid RoomId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public int Status { get; set; }
}
