using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Services;
using System;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly DataService _dataService;
        private readonly ErrorCodeService _errorCodeService;
        private readonly StatusCAMService _statusCAMService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeController));

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
            _logger.Error("Home Index");
            Global.controlPLC.Connect();

            //Thread read value plc
            Thread threadValuePLC = new Thread(GetValuePLC);
            threadValuePLC.IsBackground = true;
            threadValuePLC.Name = "GET_CURRENT_STATUS_PLC";
            threadValuePLC.Start();

            //Thread reset client

            Thread resetClient = new Thread(ResetClient);
            resetClient.IsBackground = true;
            resetClient.Name = "RESET_CLIENT";
            resetClient.Start();

            //reset plc
            //Thread threadPLCReset = new Thread(PLCReset);
            //threadPLCReset.IsBackground = true;
            //threadPLCReset.Name = "PLC_RESET";
            //threadPLCReset.Start();

            //trigger cam 1
            //Thread triggercam1 = new Thread(TriggerCam1);
            //triggercam1.IsBackground = true;
            //triggercam1.Name = "trigger_cam_1";
            //triggercam1.Start();

            ////trigger cam 2
            //Thread triggerCam2 = new Thread(TriggerCam2);
            //triggerCam2.IsBackground = true;
            //triggerCam2.Name = "TRIGGER_CAM_2";
            //triggerCam2.Start();

            ////trigger cam3
            //Thread triggerCam3 = new Thread(TriggerCam3);
            //triggerCam3.IsBackground = true;
            //triggerCam3.Name = "TRIGGER_CAM_3";
            //triggerCam3.Start();

            ////trigger cam 4
            //Thread triggerCam4 = new Thread(TriggerCam4);
            //triggerCam4.IsBackground = true;
            //triggerCam4.Name = "TRIGGER_CAM_4";
            //triggerCam4.Start();

            ViewBag.errorCodes = await _errorCodeService.GetAll();
            ViewBag.statusCams = await _statusCAMService.GetAll();

            return View();
        }

        public async void ResetClient()
        {
            while (true)
            {
                if (Global.resetClient == 1)
                {
                    Console.WriteLine("reset client");
                    await _hubContext.Clients.All.SendAsync("PLCReset", Global.resetClient);
                }
                
                Thread.Sleep(100);
            }
        }

        public async void GetValuePLC()
        {
            while (true)
            {
                _logger.Error("Value-PLC:" + Global.valuePLC);
                //_logger.Error("Current Value PLC: " + Global.valuePLC);
                await _hubContext.Clients.All.SendAsync("ChangeStatusPLC", Global.valuePLC);
                Thread.Sleep(100);
            }
        }

        //public async void PLCReset()
        //{
        //    while (true)
        //    {
        //        //Console.WriteLine("plc-reset:" + Global.resetPLC);
        //        //await _hubContext.Clients.All.SendAsync("PLCReset", Global.resetPLC);
        //        //Thread.Sleep(200);
        //    }
        //}

        public async void TriggerCam1()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("TriggerCam1", Global.triggerCAM1);
                Console.WriteLine("Trigger cam 1: " + Global.triggerCAM1);
                Thread.Sleep(1000);
            }
        }

        public async void TriggerCam2()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("TriggerCam2", Global.triggerCAM2);
                Console.WriteLine("Trigger cam 2: " + Global.triggerCAM2);
                Thread.Sleep(1000);
            }
        }

        public async void TriggerCam3()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("TriggerCam3", Global.triggerCAM3);
                Console.WriteLine("Trigger cam 3: " + Global.triggerCAM3);
                Thread.Sleep(1000);
            }
        }

        public async void TriggerCam4()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("TriggerCam4", Global.triggerCAM4);
                Console.WriteLine("Trigger cam 4: " + Global.triggerCAM4);
                Thread.Sleep(1000);
            }
        }
    }
}
