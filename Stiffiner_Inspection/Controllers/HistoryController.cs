using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Controllers
{
    public class HistoryController : Controller
    {
        private readonly DataService _dataService;

        readonly int numberItemOneTray = 20;

        public HistoryController(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> data = Global.ReadValueFileTxt(Global.PathFileSetting, ["timeline"]);
            Global.TimeLine = data["timeline"];

            var dataLeft = await _dataService.GetHistoryBySide("left");

            var dataRight = await _dataService.GetHistoryBySide("right");

            ViewBag.GroupedDataLeft = dataLeft?
               .Select((value, index) => new { CountIndex = index, Value = value })
               .GroupBy(x => x.CountIndex / numberItemOneTray)
               .Select(g => g.Select(x => x.Value).ToList())
               .ToList();

            ViewBag.GroupedDataRight = dataRight?
               .Select((value, index) => new { CountIndex = index, Value = value })
               .GroupBy(x => x.CountIndex / numberItemOneTray)
               .Select(g => g.Select(x => x.Value).ToList())
               .ToList();

            ViewBag.ListErrors = JObject.Parse(System.IO.File.ReadAllText(Global.PathValueErrors)).Properties().Select(p => new ErrorTypeResponse
            {
                Id = int.Parse(p.Name),
                Name = (string)p.Value["Name"],
                Qty = (int)p.Value["Qty"]
            }).ToList();

            return View();
        }
    }
}
