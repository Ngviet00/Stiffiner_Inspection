using Microsoft.AspNetCore.Mvc;
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
            Dictionary<string, string> data = Global.ReadValueFileTxt(Global.PathFileSetting, ["timeline", "total_error", "err_particle", "err_ng_tape_position", "err_deform", "err_scratch", "err_dirty"]);
            Global.TimeLine = data["timeline"];
            double totalError = double.Parse(data["total_error"]);
            double errParticle = double.Parse(data["err_particle"]);
            double errNgTapePosition = double.Parse(data["err_ng_tape_position"]);
            double errDeform = double.Parse(data["err_deform"]);
            double errScratch = double.Parse(data["err_scratch"]);
            double errDirty = double.Parse(data["err_dirty"]);

            var dataLeft = await _dataService.GetHistoryBySide("left");

            var dataRight = await _dataService.GetHistoryBySide("right");

            ViewBag.perErrParticle = totalError > 0 ? Math.Round(errParticle/totalError * 100, 2) : 0;
            ViewBag.perErrNGTapePosition = totalError > 0 ? Math.Round(errNgTapePosition / totalError * 100, 2) : 0;
            ViewBag.perErrDeform = totalError > 0 ? Math.Round(errDeform / totalError * 100, 2) : 0;
            ViewBag.perErrScratch = totalError > 0 ? Math.Round(errScratch / totalError * 100, 2) : 0;
            ViewBag.perErrDirty = totalError > 0 ? Math.Round(errDirty / totalError * 100, 2) : 0;

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

            return View();
        }
    }
}
