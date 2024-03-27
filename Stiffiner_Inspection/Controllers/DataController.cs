using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class DataController : Controller
    {
        private readonly DataService _dataService;
        private readonly IHubContext<HomeHub> _hubContext;
        const int PERCENT = 100;

        public DataController(DataService dataService, IHubContext<HomeHub> hubContext)
        {
            _dataService = dataService;
            _hubContext = hubContext;
        }

        [Route("save-data")]
        [HttpPost]
        public async Task<IActionResult> SaveData(DataDTO dataDTO)
        {
            try
            {
                //get current target id
                dataDTO.tray = Global.currentTray;

                //gửi lên web realtime sự kiện result log
                await _hubContext.Clients.All.SendAsync("ReceiveData", dataDTO);

                //gửi lên web realtime timelog
                await _hubContext.Clients.All.SendAsync("ReceiveTimeLog", dataDTO.time, "Program", "Send signals from Server to PLC");

                //send to PLC
                await _dataService.SendToPLC(dataDTO);

                //lưu db
                var result = await _dataService.Save(dataDTO);

                //update count total, OK, NG
                if (result.ResultArea is not null && result.ResultLine is not null)
                {
                    int totalTray = await _dataService.GetTotalTray(Global.currentTargetId);
                    double total = await _dataService.GetTotal(Global.currentTargetId);
                    int totalOK = await _dataService.GettotalOK(Global.currentTargetId);
                    int totalNG = await _dataService.GettotalNG(Global.currentTargetId);
                    int totalEmpty = await _dataService.GetTotalEmpty(Global.currentTargetId);

                    double percentOK = Math.Round((totalOK / total) * PERCENT, 2);
                    double percentNG = Math.Round((totalNG / total) * PERCENT, 2);

                    double percentChartOk = _dataService.CalculateChartOK(totalOK, total, totalEmpty);
                    double percentChartNG = _dataService.CalculateChartNG(totalNG, total, totalEmpty);
                    double percentChartEmpty = total == 0 ? 0 : Math.Round(PERCENT - percentChartNG - percentChartOk, 1);

                    //gửi lên client
                    await _hubContext.Clients.All.SendAsync("UpdateQuantity", totalTray, total, totalOK, totalNG, totalEmpty, percentOK, percentNG, percentChartOk, percentChartNG, percentChartEmpty);

                    //nếu lớn hơn target => gửi cho client hiển thị thông báo
                    if (total >= 2000)
                    {
                        await _hubContext.Clients.All.SendAsync("AlertEnoughQuantity");
                        Global.controlPLC.AlertEnoughQuantity(true);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Status = 500,
                    Message = ex.Message
                });
            }
        }

        [Route("change-cam")]
        [HttpPost]
        public async Task<IActionResult> ChangeCAM(int client_id, int status = 1)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ChangeCAM", client_id, status);

                return Ok(new
                {
                    status = 200,
                    message = "Change CAM successfully!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Status = 500,
                    Message = ex.Message
                });
            }
        }

        [Route("change-status-system-client")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatusSystemClient(int status, string? message) //1:running, 2: pause, 3: error - with message
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ChangeStatusSystemClient", status, message);

                return Ok(new
                {
                    status = 200,
                    message = "Change system status successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Status = 500,
                    Message = ex.Message
                });
            }
        }

        [Route("get-reset-plc")]
        [HttpGet]
        public async Task<IActionResult> ResetPLC(int clientId)
        {
            try
            {
                int result = 0;

                if (clientId == 1)
                {
                    result = Global.resetPLC1;
                } 

                if (clientId == 2)
                {
                    result = Global.resetPLC2;
                }

                if (clientId == 3)
                {
                    result = Global.resetPLC3;
                }

                if (clientId == 4)
                {
                    result = Global.resetPLC4;
                }

                await _hubContext.Clients.All.SendAsync("ChangeClientConnect", clientId);

                return Ok(new
                {
                    status = 200,
                    message = "Send API Success",
                    result = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Status = 500,
                    Message = ex.Message
                });
            }
        }

        [Route("post-reset-plc")]
        [HttpPost]
        public IActionResult SaveResetPLC(int clientId)
        {
            try 
            {
                if (clientId == 1)
                {
                    Global.resetPLC1 = 0;
                }

                if (clientId == 2)
                {
                    Global.resetPLC2 = 0;
                }

                if (clientId == 3)
                {
                    Global.resetPLC3 = 0;
                }

                if (clientId == 4)
                {
                    Global.resetPLC4 = 0;
                }

                return Ok(new
                {
                    status = 200,
                    message = "Send API Success",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Status = 500,
                    Message = ex.Message
                });
            }
        }

        [Route("deep-core")]
        [HttpPost]
        public async Task<IActionResult> DeepCore(int client_id, int status = 1)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("deepcore", client_id, status);

                return Ok(new
                {
                    status = 200,
                    message = "Change deep core successfully!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Status = 500,
                    Message = ex.Message
                });
            }
        }

        [Route("config")]
        [HttpGet]
        public IActionResult GetConfig()
        {
            try
            {
                return Ok(new
                {
                    status = 200,
                    message = "Send API Success",
                    type_model = 1,
                    model = "Stiffiner"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Status = 500,
                    Message = ex.Message
                });
            }
        }
    }
}
