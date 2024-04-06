using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;
using System.Net;
using static System.Net.WebRequestMethods;

namespace Stiffiner_Inspection.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class DataController : Controller
    {
        private readonly DataService _dataService;
        private readonly IHubContext<HomeHub> _hubContext;
        const int PERCENT = 100;

        const int CLIENT_1 = 1;
        const int CLIENT_2 = 2;
        const int CLIENT_3 = 3;
        const int CLIENT_4 = 4;

        const int INACTIVE = 0;

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

                //event realtime result log
                await _hubContext.Clients.All.SendAsync("ReceiveData", dataDTO);

                //event realtime timelog
                await _hubContext.Clients.All.SendAsync("ReceiveTimeLog", dataDTO.time, "Program", "Send signals from Server to PLC");

                //send to PLC
                await _dataService.SendToPLC(dataDTO);

                //save db
                var result = await _dataService.Save(dataDTO);

                return Ok(result);
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
                _dataService.ChangeStatusCamVisionBusy(client_id, 1);
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
        public async Task<IActionResult> ChangeStatusSystemClient(int clientId, int status, string? message) //1:running, 2: pause, 3: error - with message
        {
            try
            { 

                if (clientId == CLIENT_1 && status == 1)
                {
                    Global.StatusVisionChangeModel1 = INACTIVE;
                }

                if (clientId == CLIENT_2 && status == 1)
                {
                    Global.StatusVisionChangeModel2 = INACTIVE;
                }

                if (clientId == CLIENT_3 && status == 1)
                {
                    Global.StatusVisionChangeModel3 = INACTIVE;
                }

                if (clientId == CLIENT_4 && status == 1)
                {
                    Global.StatusVisionChangeModel4 = INACTIVE;
                }

                if (Global.StatusVisionChangeModel1 == INACTIVE && Global.StatusVisionChangeModel2 == INACTIVE && Global.StatusVisionChangeModel3 == INACTIVE && Global.StatusVisionChangeModel4 == INACTIVE)
                {
                    await _hubContext.Clients.All.SendAsync("ChangeStatusSystemClient", 1, message);
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync("ChangeStatusSystemClient", 2, message);
                }

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

                _dataService.ChangeConnectVisionBusy(clientId, 1);
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
        public async Task<IActionResult> DeepCore(int client_id, int status)
        {
            try
            {
                _dataService.ChangeDeepLearningVisionBusy(client_id, 1);
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

        [Route("get-model")]
        [HttpGet]
        public IActionResult GetModel()
        {
            try
            {
                return Ok(new
                {
                    status = 200,
                    message = "success",
                    type_model = Global.currentSelectedModel,
                    name_model = Global.currentSelectedModel == 1 ? "Stiffener Inspection" : "Stiffener Filler"
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
