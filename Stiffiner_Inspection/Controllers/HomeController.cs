using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly IHubContext<HistoryHub> _historyContext;
        private readonly DataService _dataService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeController));
        private readonly ApplicationDbContext _context;
        const int timeSleep = 100;
        const int PERCENT = 100;
        const int ACTIVE = 1;

        public HomeController(
            IHubContext<HomeHub> hubContext,
            IHubContext<HistoryHub> historyContext,
            DataService dataService,
            ApplicationDbContext context
        )
        {
            _hubContext = hubContext;
            _historyContext = historyContext;
            _dataService = dataService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int currentTrayId = await _dataService.GetcurrTray();
            ViewBag.currentTray = currentTrayId;
            Global.currentTray = currentTrayId;

            Global._currentSelectedModel = await _dataService.ReadOneLine(Global.PathFileCurrentModel);
            Global.ListModels = await _dataService.ReadManyLine(Global.PathFileListModel);

            string timeLine = await _dataService.ReadOneLine(Global.PathFileTimeLine);

            if (string.IsNullOrWhiteSpace(timeLine))
            {
                string currentTimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");
                Global.TimeLine = currentTimeLine;
                await _dataService.WriteOneLine(Global.TimeLine, currentTimeLine);
            }
            else
            {
                Global.TimeLine = timeLine;
            }

            string mode = await _dataService.ReadOneLine(Global.PathFileMode);

            if (string.IsNullOrWhiteSpace(mode))
            {
                Global.Mode = 1;
                await _dataService.WriteOneLine(Global.PathFileMode, "1");
            }
            else
            {
                Global.Mode = int.Parse(mode);
            }

            Global.controlPLC.Connect();

            //Thread read value PLC
            Thread threadValuePLC = new Thread(GetValuePLC);
            threadValuePLC.IsBackground = true;
            threadValuePLC.Name = "GET_CURRENT_STATUS_PLC";
            threadValuePLC.Start();

            //Thread check PLC reset
            Thread resetClient = new Thread(ResetClient);
            resetClient.IsBackground = true;
            resetClient.Name = "RESET_CLIENT";
            resetClient.Start();

            //Thread check vision busy
            Thread visionBusy = new Thread(VisionBusy);
            visionBusy.IsBackground = true;
            visionBusy.Name = "VISION_BUSY";
            visionBusy.Start();

            //Get info ok, ng, percent chart
            double total = await _dataService.GetTotal();

            int allOK = await _dataService.GettotalOK();
            int allNG = await _dataService.GettotalNG();
            int allEMPTY = await _dataService.GetTotalEmpty();

            ViewBag.TotalTray = await _dataService.GetTotalTray();
            ViewBag.Total = total;
            ViewBag.TotalOK = allOK;
            ViewBag.TotalNG = allNG;
            ViewBag.TotalEmpty = allEMPTY;

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

        public async void VisionBusy()
        {
            while (true)
            {
                if (CheckConditionVisionBusy() == false)
                {
                    Global.controlPLC.VisionBusy(true);
                    await _hubContext.Clients.All.SendAsync("ChangeStatusSystemClient", 2, "");
                } 
                else
                {
                    Global.controlPLC.VisionBusy(false);
                    await _hubContext.Clients.All.SendAsync("ChangeStatusSystemClient", 1, "");
                }

                Thread.Sleep(2000);
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

            if (string.IsNullOrWhiteSpace(Global._currentSelectedModel))
            {
                return false;
            }

            return true;
        }

        [HttpPost]
        public async Task<IActionResult> ClearData()
        {
            string currentTimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");
            Global.TimeLine = currentTimeLine;
            
            await _dataService.WriteOneLine(Global.PathFileTimeLine, currentTimeLine);
            await _historyContext.Clients.All.SendAsync("RefreshData");

            Global.ClearClient1 = 1;
            Global.ClearClient2 = 1;
            Global.ClearClient3 = 1;
            Global.ClearClient4 = 1;
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllData()
        {
            string currentTimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");
            Global.TimeLine = currentTimeLine;
           
            await _dataService.WriteOneLine(Global.PathFileTimeLine, currentTimeLine);
            await _dataService.DeleteAllData();
            await _historyContext.Clients.All.SendAsync("RefreshData");

            Global.ClearClient1 = 1;
            Global.ClearClient2 = 1;
            Global.ClearClient3 = 1;
            Global.ClearClient4 = 1;

            return RedirectToAction("Index");
        }
    }
}
