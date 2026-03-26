using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels.Rooms
{
    public class EditRoomViewModel
    {
        public Guid RoomId { get; set; }
        
        public Guid CinemaId { get; set; }

        public string? CinemaName { get; set; }

        [Required(ErrorMessage = "Tên phòng chiếu không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên phòng chiếu không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        public int TotalRows { get; set; }
        public int TotalCols { get; set; }
    }
}
