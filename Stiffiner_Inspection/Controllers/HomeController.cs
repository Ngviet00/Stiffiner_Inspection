using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Services;
using System;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO.Ports;
using static Stiffiner_Inspection.Global;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly DataService _dataService;
        private readonly ErrorCodeService _errorCodeService;
        private readonly StatusCAMService _statusCAMService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeController));

        private static SerialPort _lightControl1;
        private static SerialPort _lightControl2;

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
            //Global.controlPLC.PLCPushStart += PLCPushStartEvent;

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

            Thread toggleLight = new Thread(ToggleLightControl);
            toggleLight.IsBackground = true;
            toggleLight.Name = "RESET_CLIENT";
            toggleLight.Start();

            ViewBag.errorCodes = await _errorCodeService.GetAll();
            ViewBag.statusCams = await _statusCAMService.GetAll();

            _lightControl1 = new SerialPort("COM4", 115200);
            _lightControl2 = new SerialPort("COM5", 115200);

            string[] availablePorts = SerialPort.GetPortNames();

            if (!Array.Exists(availablePorts, p => p == "COM4"))
            {

            }

            if (!Array.Exists(availablePorts, p => p == "COM5"))
            {

            }


            if (!_lightControl1.IsOpen )
            {
                lightControl1.Open();
            }

            if (!_lightControl2.IsOpen)
            {
                _lightControl2.Open();
            }

            return View();
        }

        //private void PLCPushStartEvent(object sender, EventArgs e)
        //{
        //    Console.WriteLine("den sang");
        //}

        public void ToggleLightControl()
        {

        }

        //private void SwitchLights(bool v)
        //{
        //    if (v)
        //    {
        //        _lightControl1.WriteLine("@SI00/255/255/255/255");
        //        _lightControl2.WriteLine("@SI00/255/255/255/255");
        //    }
        //    else
        //    {
        //        _lightControl1.WriteLine("@SI00/0/0/0/0");
        //        _lightControl2.WriteLine("@SI00/0/0/0/0");
        //    }
        //}

        public void ToggleLight()
        {
            //while (true)
            //{
            //    if (Global.toggleLight)
            //    {
            //        SwitchLights(true);
            //    }
            //    else
            //    {
            //        SwitchLights(false);
            //    }
            //    Thread.Sleep(100);
            //}
        }

        public async void ResetClient()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("PLCReset", Global.resetClient);
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
