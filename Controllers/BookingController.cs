using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.Services;
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
        private readonly CinemaManagement.Services.ISeatNotifier _seatNotifier;

        public BookingController(CinemaManagementContext context, ILogger<BookingController> logger, 
            CinemaManagement.Services.ISeatNotifier seatNotifier)
        {
            _context = context;
            _logger = logger;
            _seatNotifier = seatNotifier;
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

                    // Check if ticket already exists for this ShowTime-Seat combination
                    var existingTicket = await _context.Tickets
                        .FirstOrDefaultAsync(t => t.ShowTimeId == request.ShowTimeId && t.SeatId == seatId);

                    if (existingTicket != null)
                    {
                        _logger.LogWarning("[CreateTicket] Ticket already exists for ShowTime {showTimeId} and Seat {seatId}. " +
                            "TicketId={ticketId}, Status={status}", request.ShowTimeId, seatId, existingTicket.TicketId, existingTicket.Status);
                        
                        // If status is false (cancelled), reuse it
                        if (!existingTicket.Status)
                        {
                            existingTicket.Status = true;
                            existingTicket.BookingId = booking.BookingId;
                            existingTicket.UnitPrice = pricePerSeat;
                            tickets.Add(existingTicket);
                        }
                        else
                        {
                            // Skip this seat if ticket is already active
                            _logger.LogWarning("[CreateTicket] Ticket is already active, skipping seat {seatId}", seatId);
                            continue;
                        }
                    }
                    else
                    {
                        // Create new ticket
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
                    }

                    // Update ShowTimeSeats to mark as booked (Status = 2)
                    var showTimeSeat = await _context.ShowTimeSeats
                        .FirstOrDefaultAsync(sts => sts.ShowTimeId == request.ShowTimeId && sts.SeatId == seatId);

                    if (showTimeSeat != null)
                    {
                        showTimeSeat.Status = 2; // 2 = Booked
                        showTimeSeat.HoldSessionId = null; // Clear hold
                        showTimeSeat.HoldUntil = null;
                    }
                    else
                    {
                        // Create new ShowTimeSeat if doesn't exist
                        showTimeSeat = new ShowTimeSeat
                        {
                            ShowTimeId = request.ShowTimeId,
                            SeatId = seatId,
                            Status = 2 // 2 = Booked
                        };
                        _context.ShowTimeSeats.Add(showTimeSeat);
                    }

                    // Broadcast via ISeatNotifier
                    await _seatNotifier.NotifySeatBooked(request.ShowTimeId.ToString(), seatId.ToString(), seat.SeatCode);
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
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[CreateTicket] Database error creating ticket. Inner: {innerException}", dbEx.InnerException?.Message);
                return BadRequest(new { error = $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreateTicket] Error creating ticket");
                return BadRequest(new { error = "Failed to create ticket: " + ex.Message });
            }
        }

        // POST /Booking/ReleaseSeats - API endpoint for releasing seats (used when payment fails)
        [HttpPost("ReleaseSeats")]
        public async Task<IActionResult> ReleaseSeatsApi([FromBody] ReleaseSeatsRequest request)
        {
            try
            {
                var showTimeId = request.ShowTimeId;
                var seatIds = request.SeatIds;

                _logger.LogInformation("[ReleaseSeats] START - showTimeId={showTimeId}, seatCount={seatCount}", 
                    showTimeId, seatIds?.Count ?? 0);

                if (seatIds == null || seatIds.Count == 0)
                {
                    _logger.LogWarning("[ReleaseSeats] No seat IDs provided");
                    return BadRequest(new { error = "No seat IDs provided" });
                }

                // Find all ShowTimeSeats for these seats in this showtime
                var sts = await _context.ShowTimeSeats
                    .Where(s => s.ShowTimeId == showTimeId && seatIds.Contains(s.SeatId))
                    .ToListAsync();

                _logger.LogInformation("[ReleaseSeats] Found {stCount} ShowTimeSeats to release", sts.Count);

                int updatedCount = 0;
                var releasedSeatIds = new List<string>();

                foreach (var s in sts)
                {
                    // Reset ShowTimeSeats to available (Status = 0)
                    s.Status = 0;
                    s.HoldUntil = null;
                    s.HoldSessionId = null;
                    updatedCount++;
                    releasedSeatIds.Add(s.SeatId.ToString());
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("[ReleaseSeats] Successfully released {updatedCount} ShowTimeSeats", updatedCount);

                // Broadcast to all clients that seats have been released (thanh toán th?t b?i)
                await _seatNotifier.NotifySeatsReleased(showTimeId.ToString(), releasedSeatIds);

                return Ok(new { success = true, message = $"Seats released successfully ({updatedCount} ShowTimeSeats reset)", updatedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReleaseSeats] Error releasing seats");
                return BadRequest(new { error = "Failed to release seats: " + ex.Message });
            }
        }

        public class CreateTicketRequest
        {
            public Guid ShowTimeId { get; set; }
            public List<Guid> SeatIds { get; set; } = new();
            public string OrderId { get; set; }
            public decimal TotalPrice { get; set; }
        }

        public class ReleaseSeatsRequest
        {
            public Guid ShowTimeId { get; set; }
            public List<Guid> SeatIds { get; set; } = new();
        }
    }
}
