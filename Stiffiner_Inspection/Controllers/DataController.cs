using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Stiffiner_Inspection.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class DataController : Controller
    {
        private readonly DataService _dataService;
        private readonly StatusCAMService _statusCAMService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataController));

        private SerialPort _lightControl1;
        private SerialPort _lightControl2;

        private readonly IHubContext<HomeHub> _hubContext;

        public DataController(DataService dataService, StatusCAMService statusCAMService, IHubContext<HomeHub> hubContext)
        {
            _dataService = dataService;
            _hubContext = hubContext;
            _statusCAMService = statusCAMService;

            _lightControl1 = new SerialPort("COM4", 115200);
            _lightControl2 = new SerialPort("COM5", 115200);

            if (!_lightControl1.IsOpen)
            {
                _lightControl1.Open();
            }

            if (_lightControl2.IsOpen)
            {
                _lightControl2.Open();
            }

            //_lightControl1.Close();
            //_lightControl2.Close();
        }

        [Route("save-data")]
        [HttpPost]
        public async Task<IActionResult> SaveData(DataDTO dataDTO)
        {
            try
            {
                //event to client
                await _hubContext.Clients.All.SendAsync("ReceiveData", dataDTO);

                //event to client log
                await _hubContext.Clients.All.SendAsync("ReceiveTimeLog", dataDTO.time, "Program", "Send signals from Server to PLC");

                //save to db
                var result = await _dataService.Save(dataDTO);

                //find pair
                var findPair = await _dataService.FindPair(result);

                if (findPair is not null)
                {
                    int position = _dataService.GetPosition(findPair.Index, findPair.ClientId);
                    int rs = _dataService.GetResult(result.Result, findPair.Result);
                    
                    Global.controlPLC.WriteDataToRegister(rs, position);
                    _logger.Error("Send to PLc result: " + rs + ", position: "+position);
                }

                _logger.Error("Time Log: " + dataDTO.time + "-Program-Send from server to PLC");

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
        public async Task<IActionResult> SaveResetPLC(int clientId)
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

        [Route("open-test-light")]
        [HttpGet]
        public IActionResult TestLight()
        {
            try
            {
                //toggle light
                Console.WriteLine("test-toggle light");
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

        [Route("close-light")]
        [HttpGet]
        public IActionResult CloseLight()
        {
            try
            {
                //toggle light
                Console.WriteLine("open-test-toggle light");
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
    }
}
