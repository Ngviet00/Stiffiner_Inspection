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
        const int PERCENT = 100;

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
            long currtarget = await _dataService.GetCurrentTargetID();
            Global.currentTargetId = (int) currtarget;

            //Global.controlPLC.Connect();

            //Thread read value plc
            Thread threadValuePLC = new Thread(GetValuePLC);
            threadValuePLC.IsBackground = true;
            threadValuePLC.Name = "GET_CURRENT_STATUS_PLC";
            threadValuePLC.Start();

            //reset client
            Thread resetClient = new Thread(ResetClient);
            resetClient.IsBackground = true;
            resetClient.Name = "RESET_CLIENT";
            resetClient.Start();

            double total = await _dataService.GetTotal(currtarget);

            int allOK = await _dataService.GettotalOK(currtarget);
            int allNG = await _dataService.GettotalNG(currtarget);
            int allEMPTY = await _dataService.GetTotalEmpty(currtarget);

            double PercentOK = total > 0 ? Math.Round((allOK / total) * PERCENT, 2) : 0;
            double PercentNG = total > 0 ? Math.Round((allNG / total) * PERCENT, 2) : 0;

            ViewBag.TotalTray = await _dataService.GetTotalTray(currtarget);
            ViewBag.Total = total;
            ViewBag.TotalOK = allOK;
            ViewBag.TotalNG = allNG;
            ViewBag.TotalEmpty = allEMPTY;

            ViewBag.PercentOK = PercentOK;
            ViewBag.PercentNG = PercentNG;

            ViewBag.CurrTargetQty = await _dataService.GetCurrentTargetQty(currtarget);

            ViewBag.PercentChartOK = _dataService.CalculateChartOK(allOK, total, allEMPTY);
            ViewBag.PercentChartNG = _dataService.CalculateChartOK(allNG, total, allEMPTY);
            ViewBag.PercentChartEmpty = total == 0 ? 0 : Math.Round(PERCENT - ViewBag.PercentChartNG - ViewBag.PercentChartOK, 1);

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
                await _hubContext.Clients.All.SendAsync("ChangeStatusPLC", Global.valuePLC);
                Thread.Sleep(timeSleep);
            }
        }
    }
}
