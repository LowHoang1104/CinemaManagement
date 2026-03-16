using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CinemaManagement.Controllers
{
    public class BaseController : Controller
    {
        protected readonly CinemaManagementContext _context;

        public BaseController(CinemaManagementContext context)
        {
  _context = context;
    }

 public override void OnActionExecuting(ActionExecutingContext context)
        {
    // Always load cinemas for the layout dropdown
    LoadCinemasForLayout();
      base.OnActionExecuting(context);
        }

        protected void LoadCinemasForLayout()
        {
       var allCinemas = _context.Cinemas
       .Where(x => x.Status == 1)
         .OrderBy(x => x.Name)
        .ToList();

    ViewData["Cinemas"] = allCinemas;

         // Ensure a cinema is selected
          EnsureCinemaSelected(allCinemas);
        }

        protected void EnsureCinemaSelected(List<Cinema> allCinemas)
        {
            var selectedCinemaIdStr = HttpContext.Session.GetString("SelectedCinemaId");

   if (string.IsNullOrEmpty(selectedCinemaIdStr))
       {
      // Select the first cinema as default
          var defaultCinema = allCinemas.FirstOrDefault();
    if (defaultCinema != null)
       {
       HttpContext.Session.SetString("SelectedCinemaId", defaultCinema.CinemaId.ToString());
    HttpContext.Session.SetString("SelectedCinemaName", defaultCinema.Name);
                }
      }
    }

        protected Guid GetSelectedCinemaId()
        {
   var selectedCinemaIdStr = HttpContext.Session.GetString("SelectedCinemaId");
            if (!string.IsNullOrEmpty(selectedCinemaIdStr) && Guid.TryParse(selectedCinemaIdStr, out var cinemaId))
            {
  return cinemaId;
            }
     return Guid.Empty;
        }
    }
}