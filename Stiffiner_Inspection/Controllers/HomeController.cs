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
            Global.currentTargetId = (int) await _dataService.GetCurrentTargetID();

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

            long currtarget = await _dataService.GetCurrentTargetID();
            //ViewBag.Total = await _dataService.GetTotal(currtarget);
            ViewBag.TotalTray = await _dataService.GetTotalTray(currtarget);
            int allEMPTY = await _dataService.GetTotalEmpty(currtarget);
            ViewBag.TotalEmpty = allEMPTY;

            int allOK = await _dataService.GettotalOK(currtarget);
            ViewBag.TotalOK = allOK;
            Double total = await _dataService.GetTotal(currtarget);
            ViewBag.Total = total;

            int allNG = await _dataService.GettotalNG(currtarget);
            ViewBag.TotalNG = allNG;
            Double PercentOK = Math.Round((allOK / total) * 100, 2);
            ViewBag.PercentOK = PercentOK;
            Double PercentNG = Math.Round((allNG / total) * 100, 2);
            ViewBag.PercentNG = PercentNG;
            ViewBag.CurrTargetQty = await _dataService.GetCurrentTargetQty(currtarget);
            //ViewBag.PercentChartEmpty = Math.Round(allEMPTY / (total + allEMPTY) * 100, 2);

            ViewBag.PercentChartOK = total == 0 ? 0 : Math.Round(allOK / (total + allEMPTY) * 100, 1);
            ViewBag.PercentChartNG = total == 0 ? 0 : Math.Round(allNG / (total + allEMPTY) * 100, 1);
            ViewBag.PercentChartEmpty = total == 0 ? 0 : Math.Round(100 - ViewBag.PercentChartNG - ViewBag.PercentChartOK, 1);

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
