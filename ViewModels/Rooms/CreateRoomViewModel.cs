using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels.Rooms
{
    public class CreateRoomViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn rạp chiếu.")]
        public Guid CinemaId { get; set; }

        [Required(ErrorMessage = "Tên phòng chiếu không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên phòng chiếu không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số hàng không được để trống.")]
        [Range(1, 26, ErrorMessage = "Hàng ghế phải từ 1 đến 26 (A-Z).")]
        public int TotalRows { get; set; } = 10;

        [Required(ErrorMessage = "Số ghế/hàng không được để trống.")]
        [Range(1, 50, ErrorMessage = "Số ghế mỗi hàng phải từ 1 đến 50.")]
        public int TotalCols { get; set; } = 12;
    }
}
