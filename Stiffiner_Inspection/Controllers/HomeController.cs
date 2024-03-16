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

            Thread threadPLCReset = new Thread(PLCReset);
            threadPLCReset.IsBackground = true;
            threadPLCReset.Name = "PLC_RESET";
            threadPLCReset.Start();

            ViewBag.errorCodes = await _errorCodeService.GetAll();
            ViewBag.statusCams = await _statusCAMService.GetAll();

            return View();
        }

        public async void GetValuePLC()
        {
            while (true)
            {
                _logger.Error("Current Value PLC: " + Global.valuePLC);
                await _hubContext.Clients.All.SendAsync("ChangeStatusPLC", Global.valuePLC);
                Thread.Sleep(1000);
            }
        }

        public async void PLCReset()
        {
            while (true)
            {
                //read data status reset PLC from PLC => send to client
                await _hubContext.Clients.All.SendAsync("PLCReset", Global.resetPLC);
                Thread.Sleep(1000);
            }
        }
    }
}
