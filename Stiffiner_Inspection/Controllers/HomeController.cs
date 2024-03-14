using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly DataService _dataService;
        private readonly ErrorCodeService _errorCodeService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeController));

        public HomeController(IHubContext<HomeHub> hubContext, DataService dataService, ErrorCodeService errorCodeService)
        {
            _hubContext = hubContext;
            _dataService = dataService;
            _errorCodeService = errorCodeService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.Error("dsadsa");


            //_logger.LogError("test publish log:" + 1);

            //Thread test = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        Console.WriteLine(11);
            //        _logger.LogError("duynk");
            //        Thread.Sleep(2200);
            //    }
            //});
            //test.Name = "11";
            //test.IsBackground = true;
            //test.Start();

            Global.controlPLC.Connect();

            //Thread threadGetCurrentPLC = new Thread(GetCurrentValuePLC);
            //threadGetCurrentPLC.IsBackground = true;
            //threadGetCurrentPLC.Name = "GET_CURRENT_STATUS_PLC";
            //threadGetCurrentPLC.Start();

            //Thread threadGetSignReset = new Thread(GetCurrentValueResetPLC);
            //threadGetSignReset.IsBackground = true;
            //threadGetSignReset.Name = "GET_SIGN_RESET_PLC";
            //threadGetSignReset.Start();

            //ViewBag.countOk = await _dataService.CountItemByResult(1);
            //ViewBag.countNG = await _dataService.CountItemByResult(2);
            //ViewBag.countEmpty = await _dataService.CountItemByResult(3);
            //ViewBag.errorCodes = await _errorCodeService.GetAll();

            return View();
        }

        public async void GetCurrentValuePLC()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("ChangeStatusPLC", Global.tempValuePLC);
                Thread.Sleep(1000);
            }
        }

        public async void GetCurrentValueResetPLC()
        {
            while(true) 
            {
                if (Global.plcReset != -1)
                {
                    await _hubContext.Clients.All.SendAsync("PLCReset", Global.plcReset);
                }
                
                Thread.Sleep(1000);
            }
        }
    }
}
