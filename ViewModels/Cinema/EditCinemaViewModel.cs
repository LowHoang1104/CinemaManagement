namespace CinemaManagement.ViewModels.Cinema
{
    public class EditCinemaViewModel
    {
        public Guid CinemaId { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;
    }
}
