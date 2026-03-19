using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Requests
{
    public class UpdateCinemaRequest
    {
        [Required]
        public Guid CinemaId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = null!;
    }
}
