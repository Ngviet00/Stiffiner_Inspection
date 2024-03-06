using Microsoft.AspNetCore.Mvc;
using Stiffiner_Inspection.Models;
using System.Diagnostics;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Global.controlPLC.Connect();
            return View();
        }

        public int Setdata()
        {
            Random random = new Random();
            int index = random.Next(0, 40);
            int data = random.Next(1, 4);
            Global.controlPLC.WriteSampleStatusByIndex((Global.eSampleStatus)data, index);

            Console.WriteLine("alarm message: " + Global.controlPLC.AlarmMessage);

            Console.WriteLine(string.Format("Set data to register {0} = {1}", index, data));
            //Console.WriteLine("Global:" + Global.controlPLC.GetData());
            //Console.WriteLine("*****************");
            //return Global.controlPLC.GetData();
            return 0;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
