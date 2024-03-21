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
        private readonly StatusCAMService _statusCAMService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeController));
        const int timeSleep = 100;

        public HomeController(
            IHubContext<HomeHub> hubContext,
            DataService dataService,
            ErrorCodeService errorCodeService,
            StatusCAMService statusCAMService
        )
        {
            _hubContext = hubContext;
            _dataService = dataService;
            _errorCodeService = errorCodeService;
            _statusCAMService = statusCAMService;
        }

        public async Task<IActionResult> Index()
        {
            //Global.controlPLC.Connect();

            ////Thread read value plc
            //Thread threadValuePLC = new Thread(GetValuePLC);
            //threadValuePLC.IsBackground = true;
            //threadValuePLC.Name = "GET_CURRENT_STATUS_PLC";
            //threadValuePLC.Start();

            ////reset client
            //Thread resetClient = new Thread(ResetClient);
            //resetClient.IsBackground = true;
            //resetClient.Name = "RESET_CLIENT";
            //resetClient.Start();

            double totalTray = await _dataService.CountTotal();
            ViewBag.TotalTray = Math.Floor(totalTray / 80.0);
            ViewBag.TotalItem = totalTray / 2;

            Console.WriteLine(Global.TrayUnique);

            //ViewBag.errorCodes = await _errorCodeService.GetAll();
            //ViewBag.statusCams = await _statusCAMService.GetAll();

            return View();
        }

        public async void ResetClient()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("PLCReset", Global.resetClient);
                Thread.Sleep(timeSleep);
            }
        }

        public async void GetValuePLC()
        {
            while (true)
            {
                _logger.Error("Value-PLC:" + Global.valuePLC);
                await _hubContext.Clients.All.SendAsync("ChangeStatusPLC", Global.valuePLC);
                Thread.Sleep(timeSleep);
            }
        }
    }
}
