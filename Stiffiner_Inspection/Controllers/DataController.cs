using log4net;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;
using System.Dynamic;

namespace Stiffiner_Inspection.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class DataController : Controller
    {
        private readonly DataService _dataService;
        private readonly IHubContext<HomeHub> _hubContext;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataController));

        const int CLIENT_1 = 1;
        const int CLIENT_2 = 2;
        const int CLIENT_3 = 3;
        const int CLIENT_4 = 4;

        const int INACTIVE = 0;

        const int CLIENT_RUNNING = 1;
        const int CLIENT_PAUSE = 2;

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
                //set current tray
                dataDTO.tray = Global.currentTray;

                //set model
                dataDTO.model = Global._currentSelectedModel;

                dataDTO.TimeLine = Global.TimeLine;

                //event realtime result log
                await _hubContext.Clients.All.SendAsync("ReceiveData", dataDTO);

                //send to PLC
                await _dataService.SendToPLCV2(dataDTO);

                return Ok(new
                {
                    status = 200,
                    message = "Save data successfully!"
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
        public IActionResult ChangeStatusSystemClient(int clientId, int status, string? message) //1:running, 2: pause, 3: error - with message
        {
            try
            {
                if (clientId == CLIENT_1)
                {
                    Global.DeepLearningCam1 = status;
                }

                if (clientId == CLIENT_2)
                {
                    Global.DeepLearningCam2 = status;
                }

                if (clientId == CLIENT_3)
                {
                    Global.DeepLearningCam3 = status;
                }

                if (clientId == CLIENT_4)
                {
                    Global.DeepLearningCam4 = status;
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

        [Route("post-reset-plc")]
        [HttpPost]
        public IActionResult SaveResetPLC(int clientId)
        {
            try
            {
                if (clientId == CLIENT_1)
                {
                    Global.resetPLC1 = 0;
                }

                if (clientId == CLIENT_2)
                {
                    Global.resetPLC2 = 0;
                }

                if (clientId == CLIENT_3)
                {
                    Global.resetPLC3 = 0;
                }

                if (clientId == CLIENT_4)
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

        [Route("client-get-data-server")]
        [HttpGet]
        public async Task<IActionResult> ClientGetDataServer(int clientId)
        {
            try
            {
                dynamic data = new ExpandoObject();
                data.status = 200;
                data.message = "success";
                data.name_model = Global._currentSelectedModel;
                data.mode_run = Global.Mode;

                int resultPLC = 0;

                if (clientId == CLIENT_1)
                {
                    resultPLC = Global.resetPLC1;
                    data.is_send_model = Global.Client1IsPostModel;
                    data.client_clear_data = Global.ClearClient1;
                    data.reset_cam = Global.ResetCamClient1;
                }

                if (clientId == CLIENT_2)
                {
                    resultPLC = Global.resetPLC2;
                    data.is_send_model = Global.Client2IsPostModel;
                    data.client_clear_data = Global.ClearClient2;
                    data.reset_cam = Global.ResetCamClient2;
                }

                if (clientId == CLIENT_3)
                {
                    resultPLC = Global.resetPLC3;
                    data.is_send_model = Global.Client3IsPostModel;
                    data.client_clear_data = Global.ClearClient3;
                    data.reset_cam = Global.ResetCamClient3;
                }

                if (clientId == CLIENT_4)
                {
                    resultPLC = Global.resetPLC4;
                    data.is_send_model = Global.Client4IsPostModel;
                    data.client_clear_data = Global.ClearClient4;
                    data.reset_cam = Global.ResetCamClient4;
                }

                data.result_plc = resultPLC;

                _dataService.ChangeConnectVisionBusy(clientId, 1);
                await _hubContext.Clients.All.SendAsync("ChangeClientConnect", clientId);

                return Ok(data);
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

        [Route("check-client-is-send-model")]
        [HttpPost]
        public IActionResult CheckClientIsSendModel(int clientId)
        {
            try
            {
                if (clientId == CLIENT_1)
                {
                    Global.Client1IsPostModel = 0;
                }

                if (clientId == CLIENT_2)
                {
                    Global.Client2IsPostModel = 0;
                }

                if (clientId == CLIENT_3)
                {
                    Global.Client3IsPostModel = 0;
                }

                if (clientId == CLIENT_4)
                {
                    Global.Client4IsPostModel = 0;
                }

                return Ok(new
                {
                    status = 200,
                    message = "success",
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

        [Route("client-post-model")]
        [HttpPost]
        public async Task<IActionResult> PostModel(int clientId, string listModels)
        {
            try
            {
                string models = Global.strModels;

                if (!listModels.IsNullOrWhiteSpace())
                {
                    models += "," + listModels;
                }

                models = models.Trim(',');
                Global.strModels = models;

                Global.ListModels = _dataService.GetListModelsAppearFourTime(models);

                if (Global.ListModels.Count > 0)
                {
                    await _dataService.WriteManyLine(Global.PathFileListModel, Global.ListModels);
                    await _hubContext.Clients.All.SendAsync("ListModels", Global.ListModels);
                }

                return Ok(new
                {
                    status = 200,
                    message = "success",
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

        [Route("client-clear-data")]
        [HttpPost]
        public IActionResult UpdateStatusClientClearData(int clientId)
        {
            try
            {
                if (clientId == CLIENT_1)
                {
                    Global.ClearClient1 = 0;
                }

                if (clientId == CLIENT_2)
                {
                    Global.ClearClient2 = 0;
                }

                if (clientId == CLIENT_3)
                {
                    Global.ClearClient3 = 0;
                }

                if (clientId == CLIENT_4)
                {
                    Global.ClearClient4 = 0;
                }

                return Ok(new
                {
                    status = 200,
                    message = "success",
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

        [Route("post-reset-cam")]
        [HttpPost]
        public IActionResult SaveResetCam(int clientId)
        {
            try
            {
                if (clientId == CLIENT_1)
                {
                    Global.ResetCamClient1 = 0;
                }

                if (clientId == CLIENT_2)
                {
                    Global.ResetCamClient2 = 0;
                }

                if (clientId == CLIENT_3)
                {
                    Global.ResetCamClient3 = 0;
                }

                if (clientId == CLIENT_4)
                {
                    Global.ResetCamClient4 = 0;
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
    }
}
