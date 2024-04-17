using Microsoft.AspNetCore.Mvc;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Controllers
{
    public class HistoryController : Controller
    {
        private readonly DataService _dataService;

        public HistoryController(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _dataService.GetHistory();
            ViewBag.GroupedData = data?.GroupBy(x => x.Tray).OrderByDescending(g => g.Key).Where(g => g.Count() >= 40).Take(10).ToList();

            return View();
        }
    }
}
