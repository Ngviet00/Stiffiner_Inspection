using Microsoft.AspNetCore.Mvc;

namespace Stiffiner_Inspection.Controllers
{
    [ApiController]
    public class DataController : Controller
    {
        [Route("api/data/get-data")]
        [HttpGet]
        public IActionResult GetData()
        {
            var dataList = new List<object>
            {
                new {
                    id = 1,
                    status = 200,
                    title = "test",
                    result = "OK",
                    error_code = "100",
                    client_id = 1,
                    time = "13:10:00",
                },
                new {
                    id = 2,
                    status = 200,
                    title = "test",
                    result = "OK",
                    error_code = "100",
                    client_id = 1,
                    time = "13:10:00",
                },
            };

            return Ok(dataList);
        }

        [Route("api/data/post-data")]
        [HttpPost]
        public IActionResult PostData()
        {
            var data = new
            {
                status = 200,
                message = "success"
            };

            return Ok(data);
        }
    }
}
