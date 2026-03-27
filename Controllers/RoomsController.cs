using CinemaManagement.Extensions;
using CinemaManagement.Models;
using CinemaManagement.Services;
using CinemaManagement.ViewModels.Rooms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CinemaManagement.Controllers
{
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET: Rooms
        public async Task<IActionResult> Index(Guid? cinemaId, string? search, RoomStatus? status,
                                               string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            var viewModel = await _roomService.GetAllAsync(cinemaId, search, status, sortBy, sortDir, page, pageSize);

            var cinemas = await _roomService.GetCinemasForDropdownAsync();
            ViewData["CinemaSelectList"] = new SelectList(cinemas, "CinemaId", "Name", cinemaId);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_RoomListPartial", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, int status)
        {
            var result = await _roomService.ToggleStatusAsync(id, status);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                message = result.Message,
                newStatus = result.NewStatus
            });
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction(nameof(Index));

            if (id == null) return NotFound();

            var room = await _roomService.GetByIdAsync(id.Value);
            if (room == null) return NotFound();

            var model = new EditRoomViewModel
            {
                RoomId = room.RoomId,
                CinemaId = room.CinemaId,
                CinemaName = room.Cinema?.Name,
                Name = room.Name,
                TotalRows = room.TotalRows,
                TotalCols = room.TotalCols
            };

            return PartialView("_EditModalPartial", model);
        }

        // POST: Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditRoomViewModel model)
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction(nameof(Index));

            if (id != model.RoomId) return NotFound();

            if (!ModelState.IsValid)
            {
                var existingRoom = await _roomService.GetByIdAsync(id);
                if (existingRoom != null)
                {
                    model.CinemaName = existingRoom.Cinema?.Name;
                    model.TotalRows = existingRoom.TotalRows;
                    model.TotalCols = existingRoom.TotalCols;
                }
                return PartialView("_EditModalPartial", model);
            }

            var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? adminId = Guid.TryParse(adminIdStr, out var g) ? g : null;

            var result = await _roomService.EditAsync(id, model.Name, adminId);
            if (result.Success)
            {
                return Json(new { success = true, message = result.Message, updatedName = result.UpdatedName, roomId = id });
            }

            if (result.Message == "Không tìm thấy phòng chiếu.") return NotFound();

            return BadRequest(new { success = false, message = result.Message });
        }

        // GET: Rooms/Create
        public async Task<IActionResult> Create()
        {
            var cinemas = await _roomService.GetCinemasForDropdownAsync();
            ViewData["CinemaId"] = new SelectList(cinemas, "CinemaId", "Name");
            return View(new CreateRoomViewModel());
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await HandleCreateError(model);
            }

            var room = new Room
            {
                CinemaId = model.CinemaId,
                Name = model.Name,
                TotalRows = model.TotalRows,
                TotalCols = model.TotalCols
            };

            var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? adminId = Guid.TryParse(adminIdStr, out var g) ? g : null;

            var result = await _roomService.CreateAsync(room, adminId);
            if (result.Success)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = result.Message, redirectUrl = Url.Action(nameof(Index)) });
                }
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.Message);
            return await HandleCreateError(model);
        }

        private async Task<IActionResult> HandleCreateError(CreateRoomViewModel model)
        {
            if (Request.IsAjaxRequest())
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = string.Join(" ", errors) });
            }

            var cinemas = await _roomService.GetCinemasForDropdownAsync();
            ViewData["CinemaId"] = new SelectList(cinemas, "CinemaId", "Name", model.CinemaId);
            return View(model);
        }

        // GET: /Rooms/SeatMap/{roomId}
        [HttpGet]
        public async Task<IActionResult> SeatMap(Guid id, bool editMode = false)
        {
            ViewBag.EditMode = editMode;
            var room = await _roomService.GetRoomWithSeatsAsync(id);
            if (room is null) return NotFound();

            return View(room);
        }

        // POST: /Rooms/UpdateSeats
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSeats([FromBody] List<SeatUpdateRequest> seats)
        {
            var result = await _roomService.UpdateSeatsAsync(seats);
            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message, updated = result.UpdatedCount });
            }

            if (result.Message.Contains("Không tìm thấy dữ liệu ghế")) return NotFound(new { success = false, message = result.Message });
            if (result.Message.Contains("Không thể hủy kích hoạt") || result.Message.Contains("Không có dữ liệu")) return BadRequest(new { success = false, message = result.Message });

            return StatusCode(500, new { success = false, message = result.Message });
        }

        // GET: /Rooms/CheckRoomNameExists
        [HttpGet]
        public async Task<JsonResult> CheckRoomNameExists(Guid cinemaId, string roomName, Guid? excludeRoomId = null)
        {
            bool exists = await _roomService.IsRoomNameExistsAsync(cinemaId, roomName, excludeRoomId);
            return Json(new { exists });
        }
    }
}