using log4net;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Hubs
{
    public class HomeHub : Hub
    {
        private readonly TargetService _targetService;
        private readonly DataService _dataService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeHub));
        private readonly IHubContext<HomeHub> _hubContext;
        const int PERCENT = 100;

        public HomeHub(TargetService targetService, DataService dataService, IHubContext<HomeHub> hubContext)
        {
            _targetService = targetService;
            _dataService = dataService;
            _hubContext = hubContext;
        }

        public async Task AddNewTarget(int targetValue)
        {
            try
            {
                await _targetService.InsertTargetQty(Global.currentTargetId, targetValue);
            }
            catch (Exception ex)
            {
                _logger.Error("Add new target failed: " + ex);
            }
        }
        public async Task UpdateTarget(int targetValue)
        {
            try
            {
                await _targetService.UpdateTargetQty(Global.currentTargetId, targetValue);
            }
            catch (Exception ex)
            {
                _logger.Error("Update target failed: " + ex);
            }
        }

        public async Task<StatisticalCalculationsResponse?> UpdateStatistical(string message)
        {
            try
            {
                StatisticalCalculationsResponse result = new StatisticalCalculationsResponse();

                int totalTray = await _dataService.GetTotalTray(Global.currentTargetId);
                double total = await _dataService.GetTotal(Global.currentTargetId);
                int totalOK = await _dataService.GettotalOK(Global.currentTargetId);
                int totalNG = await _dataService.GettotalNG(Global.currentTargetId);
                int totalEmpty = await _dataService.GetTotalEmpty(Global.currentTargetId);

                double percentOK = total == 0 ? 0 : Math.Round((totalOK / total) * PERCENT, 2);
                double percentNG = total == 0 ? 0 : Math.Round((totalNG / total) * PERCENT, 2);

                double percentChartOk = _dataService.CalculateChartOK(totalOK, total, totalEmpty);
                double percentChartNG = _dataService.CalculateChartNG(totalNG, total, totalEmpty);
                double percentChartEmpty = total == 0 ? 0 : Math.Round(PERCENT - percentChartNG - percentChartOk, 2);

                result.TotalTray = totalTray;
                result.Total = total;
                result.TotalOK = totalOK;
                result.TotalNG = totalNG;
                result.TotalEmpty = totalEmpty;

                result.PercentOK = percentOK;
                result.PercentNG = percentNG;

                result.PercentChartOk = percentChartOk;
                result.percentChartNG = percentChartNG;
                result.percentChartEmpty = percentChartEmpty;

                //alert set enough target
                //if (total >= Global.NumberTarget)
                //{
                //    //send alert to client is enough quantity
                //    await _hubContext.Clients.All.SendAsync("AlertEnoughQuantity");

                //    //send signal to PLC alert enough quantity
                //    Global.controlPLC.AlertEnoughQuantity(true);

                //    //set global is check is true
                //    Global.IsEnoughTarget = true;
                //}
                //return result;

                return result;

            } catch (Exception ex)
            {
                _logger.Error("Update statistical calculations failed: " + ex.Message);
                return null;
            }
        }
    }
}
