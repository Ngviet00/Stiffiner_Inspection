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
        const int ACTIVE = 1;

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
            ViewBag.currentTargetId = Global.currentTargetId;

            int currentTrayId = await _dataService.GetcurrTray(Global.currentTargetId);
            ViewBag.currentTray = currentTrayId;
            Global.currentTray = currentTrayId;

            Global.controlPLC.Connect();

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

            //vision busy
            Thread visionBusy = new Thread(VisionBusy);
            visionBusy.IsBackground = true;
            visionBusy.Name = "VISION_BUSY";
            visionBusy.Start();

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
            ViewBag.PercentChartNG = _dataService.CalculateChartNG(allNG, total, allEMPTY);
            ViewBag.PercentChartEmpty = total == 0 ? 0 : Math.Round(PERCENT - ViewBag.PercentChartNG - ViewBag.PercentChartOK, 2);

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

        public void VisionBusy()
        {
            while (true)
            {
                if (CheckConditionVisionBusy())
                {
                    Global.controlPLC.VisionBusy(false);
                }
                else
                {
                    Global.controlPLC.VisionBusy(true);
                }

                Thread.Sleep(1000);
            }
        }

        //true is not busy, false is busy
        public bool CheckConditionVisionBusy()
        {
            if (Global.StatusCam1 != ACTIVE || Global.StatusCam2 != ACTIVE || Global.StatusCam3 != ACTIVE || Global.StatusCam4 != ACTIVE)
            {
                return false;
            }

            if (Global.ConnectCam1 != ACTIVE || Global.ConnectCam2 != ACTIVE || Global.ConnectCam3 != ACTIVE || Global.ConnectCam4 != ACTIVE)
            {
                return false;
            }

            if (Global.DeepLearningCam1 != ACTIVE || Global.DeepLearningCam2 != ACTIVE || Global.DeepLearningCam3 != ACTIVE || Global.DeepLearningCam4 != ACTIVE)
            {
                return false;
            }

            return true;
        }
    }
}
