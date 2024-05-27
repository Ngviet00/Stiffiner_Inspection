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

            var dataLeft = await _dataService.GetHistoryBySide("left");

            var dataRight = await _dataService.GetHistoryBySide("right");

            ViewBag.GroupedDataLeft = dataLeft?
               .Select((value, index) => new { CountIndex = index, Value = value })
               .GroupBy(x => x.CountIndex / 20)
               .Select(g => g.Select(x => x.Value).ToList())
               .ToList();

            ViewBag.GroupedDataRight = dataRight?
               .Select((value, index) => new { CountIndex = index, Value = value })
               .GroupBy(x => x.CountIndex / 20)
               .Select(g => g.Select(x => x.Value).ToList())
               .ToList();

            return View();
        }
    }
}
