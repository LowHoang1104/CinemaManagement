using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels.AdminMovies;

public class AdminMovieViewModel
{
    public Guid MovieId { get; set; }

    [Required(ErrorMessage = "Tên phim là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên phim tối đa 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Range(1, 600, ErrorMessage = "Thời lượng phải lớn hơn 0")]
    [Display(Name = "Thời lượng (phút)")]
    public int DurationMin { get; set; } = 90;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "Poster URL tối đa 500 ký tự")]
    [Display(Name = "Poster URL")]
    public string? PosterUrl { get; set; }

    [Range(0, 21, ErrorMessage = "Độ tuổi phải từ 0 đến 21")]
    [Display(Name = "Độ tuổi")]
    public int? AgeRating { get; set; }

    [StringLength(200)]
    public string? Director { get; set; }

    [StringLength(500)]
    public string? Actors { get; set; }

    [StringLength(200)]
    public string? Genre { get; set; }

    [StringLength(100)]
    public string? Language { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày phát hành")]
    public DateTime? ReleaseDate { get; set; }

    [Range(0, 1, ErrorMessage = "Trạng thái chỉ nhận 0 hoặc 1")]
    public int Status { get; set; } = 1;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdatedAt { get; set; }
}
