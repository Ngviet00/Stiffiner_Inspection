using Microsoft.AspNetCore.Mvc;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class DataController : Controller
    {
        public DataService _dataService;

        public DataController(DataService dataService)
        {
            _dataService = dataService;
        }

        [Route("save-data")]
        [HttpPost]
        public async Task<IActionResult> SaveData(DataDTO dataDTO)
        {
            try
            {
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

        [Route("check-index")]
        [HttpPost]
        public IActionResult CheckIndex()
        {
            var data = new
            {
                status = 200,
                message = "success"
            };

            return Ok(data);
        }

        [Route("get-model")]
        [HttpGet]
        public IActionResult GetModel(int threshold, int min_area)
        {
            var result = new
            {
                status = 200,
                message = "success",
                threshold = threshold,
                min_area = min_area
            };

            return Ok(result);
        }
    }
}
