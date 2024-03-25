using Microsoft.AspNetCore.Mvc;

namespace Stiffiner_Inspection.Controllers
{
    public class HistoryController : Controller
    {
        public HistoryController()
        {

        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
