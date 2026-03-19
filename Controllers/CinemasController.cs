using CinemaManagement.Extensions;
using CinemaManagement.Requests;
using CinemaManagement.Services;
using CinemaManagement.ViewModels.Cinema;
using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers
{
    public class CinemasController : Controller
    {
        private readonly ICinemaService _cinemaService;

        public CinemasController(ICinemaService cinemaService)
        {
            _cinemaService = cinemaService;
        }

        public async Task<IActionResult> Index(string? search, int? status, string? sortBy, string? sortDir, int page = 1, int pageSize = 2)
        {
            var result = await _cinemaService.GetAllAsync(search, status, sortBy, sortDir, page, pageSize);
            var stats = await _cinemaService.GetStatsAsync();

            var vm = new CinemaIndexViewModel
            {
                Cinemas = result.Items,
                Stats = stats,
                SearchKeyword = search ?? "",
                StatusFilter = status,
                SortBy = sortBy ?? "",
                SortDir = sortDir ?? "asc",
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize)
            };

            // Kiểm tra nếu là AJAX request thì chỉ trả về Partial View chứa các Card
            if (Request.IsAjaxRequest())
            {
                return PartialView("_CinemaGridPartial", vm);
            }

            return View(vm);
        }

        // GET: Để hiển thị form trong Modal
        public IActionResult Create()
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction(nameof(Index));

            return PartialView("_CreateCinemaPartial", new CreateCinemaRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, Address")] CreateCinemaRequest request)
        {
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                    return PartialView("_CreateCinemaPartial", request);
                return View(request);
            }

            try
            {
                // TODO: Khi có hệ thống Auth, thay null bằng ID của User đang đăng nhập
                // Guid? currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                Guid? currentUserId = null; 

                await _cinemaService.CreateAsync(request, currentUserId);

                // 3. UX on Success: Nếu là Modal AJAX, trả về JSON để FE tự đóng Modal và hiện Toast
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = $"Đã tạo rạp {request.Name} thành công!" });
                }

                TempData["SuccessMessage"] = $"Đã tạo rạp {request.Name} thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // 2. Bắt lỗi trùng tên từ Service và hiển thị đỏ lòm trên field "Name"
                ModelState.AddModelError("Name", ex.Message);
                
                if (Request.IsAjaxRequest())
                    return PartialView("_CreateCinemaPartial", request);
                    
                return View(request);
            }
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction(nameof(Index));

            if (id == null) return NotFound();
            var cinemaVm = await _cinemaService.GetEditByIdAsync(id.Value);
            if (cinemaVm == null) return NotFound();

            return PartialView("_EditCinemaPartial", cinemaVm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var cinema = await _cinemaService.GetCinemaDetailsAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }
            return View(cinema);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyName(string name, Guid? currentId = null)
        {
            bool exists = await _cinemaService.IsCinemaNameExistsAsync(name, currentId);
            return Json(new { isUnique = !exists });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CinemaId,Name,Address")] EditCinemaViewModel vm)
        {
            if (id != vm.CinemaId) return BadRequest();

            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                    return PartialView("_EditCinemaPartial", vm);
                return View(vm);
            }

            try
            {
                var request = new UpdateCinemaRequest
                {
                    CinemaId = vm.CinemaId,
                    Name = vm.Name,
                    Address = vm.Address,
                };

                await _cinemaService.UpdateAsync(request, null);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = $"Cập nhật rạp {vm.Name} thành công!" });
                }

                TempData["SuccessMessage"] = $"Cập nhật rạp {vm.Name} thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Name", ex.Message);
                if (Request.IsAjaxRequest())
                    return PartialView("_EditCinemaPartial", vm);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                var cinema = await _cinemaService.GetByIdAsync(id);
                // TODO: Lấy User ID từ người dùng đang đăng nhập
                await _cinemaService.ActivateAsync(id, null);
                return Json(new { success = true, message = $"Kích hoạt rạp {cinema.Name} thành công!" });
            }
            catch (Exception ex)
            {
                // Bắt lỗi (ví dụ: "Cinema must have at least one room") để đẩy về Toast/Alert
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                var cinema = await _cinemaService.GetByIdAsync(id);
                await _cinemaService.DeactivateAsync(id, null);
                return Json(new { success = true, message = $"Ngừng hoạt động rạp {cinema.Name} thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
