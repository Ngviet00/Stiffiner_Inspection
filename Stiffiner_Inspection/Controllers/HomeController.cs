using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Commons;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly IHubContext<HistoryHub> _historyContext;
        private readonly DataService _dataService;
        const int timeSleep = 100;

        public HomeController(
            IHubContext<HomeHub> hubContext,
            IHubContext<HistoryHub> historyContext,
            DataService dataService
        )
        {
            _hubContext = hubContext;
            _historyContext = historyContext;
            _dataService = dataService;
        }

        public async Task<IActionResult> Index()
        {
            Dictionary<string, string> currentData = Global.ReadValueFileTxt(Global.PathFileSetting, ["total", "ok", "ng", "empty", "current_tray", "hidden_setting", "mode", "timeline", "current_model"]);

            Global.Total = int.Parse(currentData["total"]);
            Global.TotalOK = int.Parse(currentData["ok"]);
            Global.TotalNG = int.Parse(currentData["ng"]);
            Global.TotalEmpty = int.Parse(currentData["empty"]);
            Global.currentTray = int.Parse(currentData["current_tray"]);
            Global.HiddenSetting = int.Parse(currentData["hidden_setting"]);
            Global.Mode = int.Parse(currentData["mode"]);
            Global.TimeLine = currentData["timeline"];
            Global._currentSelectedModel = currentData["current_model"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Global.TimeLine))
            {
                Global.TimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");
                Global.WriteFileToTxt(Global.PathFileSetting, new Dictionary<string, string> 
                {
                    { "timeline", Global.TimeLine }
                });
            }

            Global.ListModels = await _dataService.ReadManyLine(Global.PathFileListModel);

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

            ViewBag.TotalTray = Global.Total > 0 ? Global.Total / 40 : 0;
            ViewBag.Total = Global.Total;
            ViewBag.TotalOK = Global.TotalOK;
            ViewBag.TotalNG = Global.TotalNG;
            ViewBag.TotalEmpty = Global.TotalEmpty;

            ViewBag.PercentChartOK = _dataService.CalculateChartOK(Global.TotalOK, Global.Total, Global.TotalEmpty);
            ViewBag.PercentChartNG = _dataService.CalculateChartNG(Global.TotalNG, Global.Total, Global.TotalEmpty);
            ViewBag.PercentChartEmpty = Global.Total == 0 ? 0 : Math.Round(Constants.PERCENT - ViewBag.PercentChartNG - ViewBag.PercentChartOK, 2);

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
                    await _hubContext.Clients.All.SendAsync("ChangeStatusSystemClient", Constants.PAUSE, "");
                } 
                else
                {
                    Global.controlPLC.VisionBusy(false);
                    await _hubContext.Clients.All.SendAsync("ChangeStatusSystemClient", Constants.RUNNING, "");
                }

                Thread.Sleep(2000);
            }
        }

        //true is not busy, false is busy
        public bool CheckConditionVisionBusy()
        {
            if (Global.StatusCam1 != Constants.ACTIVE || Global.StatusCam2 != Constants.ACTIVE || Global.StatusCam3 != Constants.ACTIVE || Global.StatusCam4 != Constants.ACTIVE)
            {
                return false;
            }

            if (Global.ConnectCam1 != Constants.ACTIVE || Global.ConnectCam2 != Constants.ACTIVE || Global.ConnectCam3 != Constants.ACTIVE || Global.ConnectCam4 != Constants.ACTIVE)
            {
                return false;
            }

            if (Global.DeepLearningCam1 != Constants.ACTIVE || Global.DeepLearningCam2 != Constants.ACTIVE || Global.DeepLearningCam3 != Constants.ACTIVE || Global.DeepLearningCam4 != Constants.ACTIVE)
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
            Global.TimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");
            Global.WriteFileToTxt(Global.PathFileSetting, new Dictionary<string, string> {
                { "total", "0" },
                { "ok", "0"},
                { "ng", "0" },
                { "empty", "0" },
                { "current_tray", "0" },
                { "timeline", Global.TimeLine }
            });

            await _historyContext.Clients.All.SendAsync("RefreshData");

            Global.ClearClient1 = Constants.ACTIVE;
            Global.ClearClient2 = Constants.ACTIVE;
            Global.ClearClient3 = Constants.ACTIVE;
            Global.ClearClient4 = Constants.ACTIVE;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllData()
        {
            Global.TimeLine = DateTime.Now.ToString("yyyyMMddHHmmss");
            Global.WriteFileToTxt(Global.PathFileSetting, new Dictionary<string, string> {
                { "total", "0" },
                { "ok", "0"},
                { "ng", "0" },
                { "empty", "0" },
                { "current_tray", "0" },
                { "timeline", Global.TimeLine }
            });

            await _dataService.DeleteAllData();
            await _historyContext.Clients.All.SendAsync("RefreshData");

            Global.ClearClient1 = Constants.ACTIVE;
            Global.ClearClient2 = Constants.ACTIVE;
            Global.ClearClient3 = Constants.ACTIVE;
            Global.ClearClient4 = Constants.ACTIVE;

            return RedirectToAction("Index");
        }
    }
}
