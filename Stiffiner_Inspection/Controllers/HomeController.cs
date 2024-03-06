using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models;
using System.ComponentModel;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<HomeHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, IHubContext<HomeHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            Global.controlPLC.Connect();
            return View();
        }

        public async void Setdata()
        {
            /*Random rnd = new Random();
            int test = rnd.Next(2, 3);
            await _hubContext.Clients.All.SendAsync("connect-plc", test);

            Random random = new Random();
            int index = random.Next(0, 40);
            int data = random.Next(1, 4);
            Global.controlPLC.WriteSampleStatusByIndex((Global.eSampleStatus)data, index);

            Console.WriteLine("alarm message: " + Global.controlPLC.AlarmMessage);

            Console.WriteLine(string.Format("Set data to register {0} = {1}", index, data));
            //Console.WriteLine("Global:" + Global.controlPLC.GetData());
            //Console.WriteLine("*****************");
            //return Global.controlPLC.GetData();
            return 0;*/
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
