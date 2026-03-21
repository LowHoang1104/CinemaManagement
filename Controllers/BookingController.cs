using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    [Route("Booking")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly CinemaManagementContext _context;
        private readonly ILogger<BookingController> _logger;

        public BookingController(CinemaManagementContext context, ILogger<BookingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST /Booking/CreateTicket
        [HttpPost("CreateTicket")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not logged in" });
                }

                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return BadRequest(new { error = "Invalid user ID" });
                }

                _logger.LogInformation("[CreateTicket] userId={userId} showTimeId={showTimeId} seatIds={seatIds} orderId={orderId}",
                    userId, request.ShowTimeId, string.Join(",", request.SeatIds), request.OrderId);

                // Verify showtime exists
                var showTime = await _context.ShowTimes.FirstOrDefaultAsync(s => s.ShowTimeId == request.ShowTimeId);
                if (showTime == null)
                {
                    return NotFound(new { error = "ShowTime not found" });
                }

                // Create booking record
                var bookingCode = $"BK{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
                var booking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    UserId = userGuid,
                    ShowTimeId = request.ShowTimeId,
                    BookingCode = bookingCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddMinutes(10),
                    Status = 1, // 1 = Completed/Confirmed
                    TotalAmount = request.TotalPrice
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create tickets and update seat status
                var tickets = new List<Ticket>();
                var pricePerSeat = request.TotalPrice / request.SeatIds.Count;

                foreach (var seatId in request.SeatIds)
                {
                    var seat = await _context.Seats.FirstOrDefaultAsync(s => s.SeatId == seatId);
                    if (seat == null)
                    {
                        _logger.LogWarning("[CreateTicket] Seat not found: {seatId}", seatId);
                        continue;
                    }

                    // Create ticket
                    var ticketCode = $"TK{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(100000, 999999)}";
                    var ticket = new Ticket
                    {
                        TicketId = Guid.NewGuid(),
                        BookingId = booking.BookingId,
                        ShowTimeId = request.ShowTimeId,
                        SeatId = seatId,
                        UnitPrice = pricePerSeat,
                        TicketCode = ticketCode,
                        Status = true // true = Active/Valid
                    };

                    tickets.Add(ticket);
                    _context.Tickets.Add(ticket);

                    // Update ShowTimeSeats to mark as booked (status = 1)
                    var showTimeSeat = await _context.ShowTimeSeats
                        .FirstOrDefaultAsync(sts => sts.ShowTimeId == request.ShowTimeId && sts.SeatId == seatId);

                    if (showTimeSeat != null)
                    {
                        showTimeSeat.Status = 1; // 1 = Booked
                    }
                    else
                    {
                        // Create new ShowTimeSeat if doesn't exist
                        showTimeSeat = new ShowTimeSeat
                        {
                            ShowTimeId = request.ShowTimeId,
                            SeatId = seatId,
                            Status = 1 // 1 = Booked
                        };
                        _context.ShowTimeSeats.Add(showTimeSeat);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("[CreateTicket] Successfully created {ticketCount} tickets for booking {bookingId}",
                    tickets.Count, booking.BookingId);

                return Ok(new
                {
                    success = true,
                    bookingId = booking.BookingId,
                    bookingCode = booking.BookingCode,
                    ticketCount = tickets.Count,
                    totalPrice = booking.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreateTicket] Error creating ticket");
                return BadRequest(new { error = "Failed to create ticket: " + ex.Message });
            }
        }

        public class CreateTicketRequest
        {
            public Guid ShowTimeId { get; set; }
            public List<Guid> SeatIds { get; set; } = new();
            public string OrderId { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
