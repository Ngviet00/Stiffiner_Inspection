using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models;
using Stiffiner_Inspection.Services;
//using System.Diagnostics;

namespace Stiffiner_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly DataService _dataService;
        private readonly ErrorCodeService _errorCodeService;

        public HomeController(IHubContext<HomeHub> hubContext, DataService dataService, ErrorCodeService errorCodeService)
        {
            _hubContext = hubContext;
            _dataService = dataService;
            _errorCodeService = errorCodeService;
        }

        public async Task<IActionResult> Index()
        {
            Global.controlPLC.Connect();

            Thread threadGetCurrentPLC = new Thread(GetCurrentValuePLC);
            threadGetCurrentPLC.IsBackground = true;
            threadGetCurrentPLC.Name = "GET_CURRENT_STATUS_PLC";
            threadGetCurrentPLC.Start();

            //ViewBag.countOk = await _dataService.CountItemByResult(1);
            //ViewBag.countNG = await _dataService.CountItemByResult(2);
            //ViewBag.countEmpty = await _dataService.CountItemByResult(3);
            //ViewBag.errorCodes = await _errorCodeService.GetAll();

            return View();
        }

        public async void GetCurrentValuePLC()
        {
            while (true)
            {
                await _hubContext.Clients.All.SendAsync("ChangeStatusPLC", Global.tempValuePLC);
                Thread.Sleep(1000);
            }
        }
    }
}
