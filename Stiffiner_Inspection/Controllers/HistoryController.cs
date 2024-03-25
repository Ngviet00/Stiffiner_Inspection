using Microsoft.AspNetCore.Mvc;

namespace Stiffiner_Inspection.Controllers
{
    public class HistoryController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
