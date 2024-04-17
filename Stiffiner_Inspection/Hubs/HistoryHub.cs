using log4net;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Models.Entity;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Hubs
{
    public class HistoryHub : Hub
    {
        private readonly DataService _dataService;

        public HistoryHub (DataService dataService)
        {
            _dataService = dataService;
        }

        public List<ImageResponse>? DownloadFile(List<Image> images)
        {
            try
            {
                return _dataService.DownloadFile(images);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cannot download file: " + ex.ToString());
                throw;
            }
        }
    }
}
