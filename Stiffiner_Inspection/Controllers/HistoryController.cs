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
            Global.TimeLine = await _dataService.ReadOneLine(Global.PathFileTimeLine);

            var data = await _dataService.GetHistory();

            ViewBag.GroupedData = data?
                .Select((value, index) => new { CountIndex = index, Value = value })
                .GroupBy(x => x.CountIndex / 40)
                .Select(g => g.Select(x => x.Value).ToList())
                .ToList();

            return View();
        }
    }
}
