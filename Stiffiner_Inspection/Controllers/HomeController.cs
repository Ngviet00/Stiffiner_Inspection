using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models;
using Stiffiner_Inspection.Services;
using System.Diagnostics;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly DataService _dataService;
        private readonly ErrorCodeService _errorCodeService;

        public HomeController(IHubContext<HomeHub> hubContext, DataService dataService, ErrorCodeService errorCodeService)
        {
            _hubContext = hubContext;
            _dataService = dataService;
            _errorCodeService = errorCodeService;
        }

        public async Task<IActionResult> Index()
        {
            /*Global.controlPLC.Connect();*/
            ViewBag.countOk = await _dataService.CountItemByResult(1);
            ViewBag.countNG = await _dataService.CountItemByResult(2);
            ViewBag.countEmpty = await _dataService.CountItemByResult(3);
            ViewBag.errorCodes = await _errorCodeService.GetAll();

            return View();
        }

        public async void Setdata()
        {
            Random random = new Random();
            int index = random.Next(0, 40);
            int data = random.Next(1, 4);
            /*Global.controlPLC.WriteSampleStatusByIndex((Global.eSampleStatus)data, index);*/

            /*Console.WriteLine("alarm message: " + Global.controlPLC.AlarmMessage);*/

            Console.WriteLine(string.Format("Set data to register {0} = {1}", index, data));
            //Console.WriteLine("Global:" + Global.controlPLC.GetData());
            //Console.WriteLine("*****************");
            //return Global.controlPLC.GetData();
            /*return 0;*/
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
