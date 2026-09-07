CinemaManagement
Problem

Quản lý rạp chiếu phim bằng phương pháp thủ công gây khó khăn trong việc quản lý phim, phòng chiếu, suất chiếu, ghế và đặt vé. Đặc biệt, khi nhiều khách hàng cùng chọn một suất chiếu, việc cập nhật trạng thái ghế có thể xảy ra tình trạng trùng ghế hoặc sai trạng thái.

Solution

Xây dựng hệ thống quản lý rạp chiếu phim và booking vé, giúp số hóa các nghiệp vụ quản lý phim, phòng chiếu, suất chiếu, ghế và vé.

Hệ thống sử dụng SignalR để cập nhật trạng thái ghế theo thời gian thực giữa các người dùng, giúp hạn chế tình trạng nhiều người cùng đặt một ghế.

Project Description

CinemaManagement là hệ thống quản lý rạp chiếu phim và đặt vé xem phim được xây dựng với ASP.NET Core MVC.

Các chức năng chính:

Quản lý phim, thể loại.
Quản lý rạp, phòng chiếu và ghế.
Quản lý suất chiếu.
Đặt vé và thanh toán.
Quản lý người dùng và phân quyền.
Cập nhật trạng thái ghế realtime với SignalR.
Thống kê và báo cáo doanh thu.
Technologies
C# / ASP.NET Core MVC
Entity Framework Core
PostgreSQL
SignalR
FluentValidation
HTML / CSS / JavaScript
