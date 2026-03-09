namespace CinemaManagement.ViewModels.Cinema
{
    public class EditCinemaVm
    {
        public Guid CinemaId { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;
    }
}
