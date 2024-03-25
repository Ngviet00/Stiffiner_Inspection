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
        const int timeSleep = 100;

        public HomeController(
            IHubContext<HomeHub> hubContext,
            DataService dataService,
            ErrorCodeService errorCodeService
        )
        {
            _hubContext = hubContext;
            _dataService = dataService;
            _errorCodeService = errorCodeService;
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


            //ViewBag.TotalTray = Math.Floor(totalTray / 80.0);
            //ViewBag.TotalItem = totalTray / 2;
            //ViewBag.Total = await _dataService.GetTotal();
            //ViewBag.TotalTray = await _dataService.GetTotalTray();
            //int allEMPTY = await _dataService.GetTotalEmpty();
            //ViewBag.TotalEmpty = allEMPTY;
            //int allOK = await _dataService.GettotalOK();
            //ViewBag.TotalOK = allOK;
            //Double total = await _dataService.GetTotal();
            //ViewBag.Total = total;
            //int allNG = await _dataService.GettotalNG(); ;
            //ViewBag.TotalNG = allNG;
            //Double PercentOK = Math.Round((allOK / total) * 100, 2);
            //ViewBag.PercentOK = PercentOK;
            //Double PercentNG = Math.Round((allNG / total) * 100, 2);
            //ViewBag.PercentNG = PercentNG;
            //ViewBag.CurrTargetQty = await _dataService.GetCurrentTargetQty();
            ////ViewBag.PercentChartEmpty = Math.Round(allEMPTY / (total + allEMPTY) * 100, 2);

            //ViewBag.PercentChartOK = total == 0 ? 0 : Math.Round(allOK / (total + allEMPTY) * 100, 1);
            //ViewBag.PercentChartNG = total == 0 ? 0 : Math.Round(allNG / (total + allEMPTY) * 100, 1);
            //ViewBag.PercentChartEmpty = total == 0 ? 0 : Math.Round(100 - ViewBag.PercentChartNG - ViewBag.PercentChartOK, 1);


            //Console.WriteLine(Global.TrayUnique);

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
