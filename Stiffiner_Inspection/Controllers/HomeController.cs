using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models.DTO.Data;
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
            Global.controlPLC.Connect();

            //Thread read value plc
            Thread threadGetCurrentPLC = new Thread(GetValuePLC);
            threadGetCurrentPLC.IsBackground = true;
            threadGetCurrentPLC.Name = "GET_CURRENT_STATUS_PLC";
            threadGetCurrentPLC.Start();

            //Global.controlPLC.WriteDataToRegister(1, 0);

            //Thread threadGetSignReset = new Thread(GetCurrentValueResetPLC);
            //threadGetSignReset.IsBackground = true;
            //threadGetSignReset.Name = "GET_SIGN_RESET_PLC";
            //threadGetSignReset.Start();
            return View();
        }

        public async void GetValuePLC()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("ChangeStatusPLC", Global.valuePLC);
                Thread.Sleep(1000);
            }
        }

        //public async void GetCurrentValueResetPLC()
        //{
        //    while(true) 
        //    {
        //        if (Global.plcReset != -1)
        //        {
        //            await _hubContext.Clients.All.SendAsync("PLCReset", Global.plcReset);
        //        }
                
        //        Thread.Sleep(1000);
        //    }
        //}
    }
}
