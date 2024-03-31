using log4net;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Models.Entity;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;
using System.Net;

namespace Stiffiner_Inspection.Hubs
{
    public class HistoryHub : Hub
    {
        private readonly DataService _dataService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeHub));

        public HistoryHub (DataService dataService)
        {
            _dataService = dataService;
        }

        //public async Task<List<ImageResponse>> DownloadFile(List<Image> images)
        //{
        //    try
        //    {
        //        return await _dataService.DownloadFile(images);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error cannot download file: " + ex.ToString());
        //        throw;
        //    }
        //}

        public async Task<string> DownloadFile(List<Image> images)
        {
            try
            {
                return await _dataService.DownloadFile(images);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cannot download file: " + ex.ToString());
                throw;
            }
        }
    }
}
