using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;
using System.IO;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stiffiner_Inspection.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class DataController : Controller
    {
        protected readonly DataService _dataService;
        private readonly IHubContext<HomeHub> _hubContext;

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
                //save to db
                var result = await _dataService.Save(dataDTO);

                //event to client
                await _hubContext.Clients.All.SendAsync("ReceiveData", dataDTO);

                //send to status to PLC
                //Global.controlPLC.WriteDataToRegister(dataDTO.result, dataDTO.index);

                //event to client log
                await _hubContext.Clients.All.SendAsync("ReceiveTimeLog", "2024-03-12T05:37:37.723Z", "Program", "Send from server to PLC");

                //write log to file
                //await _dataService.SaveTimeLog(dataDTO.time, "Program", "Send from server to PLC");

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

        [Route("change-status-cam-client")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatusCamClient(int client_id, int status = 1) //1 active, 2 inactive
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ChangeStatusCamClient", client_id, status);

                return Ok(new
                {
                    status = 200,
                    message = "Send API Success"
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
                    message = "Success"
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

        [Route("change-status-trigger-cam")]
        [HttpPost]
        public async Task<IActionResult> ChangeStatusTriggerCam(int client_id) //1:running, 2: pause, 3: error - with message
        {
            try
            {
                //get from PLC
                await _hubContext.Clients.All.SendAsync("ChangeStatusTriggerCam", client_id);

                return Ok(new
                {
                    status = 200,
                    message = "Success"
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

        [Route("change-client-connect")]
        [HttpPost]
        public async Task<IActionResult> ChangeClientConnect(int client_id, int status = 1) //1 active, 2 inactive
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ChangeClientConnect", client_id, status);

                return Ok(new
                {
                    status = 200,
                    message = "Send API Success"
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

        [Route("plc-reset")]
        [HttpGet]
        public IActionResult PLCReset()
        {
            try
            {
                return Ok(new
                {
                    status = 200,
                    message = "Send API Success",
                    result = Global.plcReset,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok(new
                {
                    status = 200,
                    message = "Send API Success",
                    result = -1,
                });
            }
        }


        [Route("test-api")]
        [HttpGet]
        public IActionResult TestAPI() 
        {
            return Ok(new
            {
                status = 200,
                message = "API Success"
            });
        }
    }
}
