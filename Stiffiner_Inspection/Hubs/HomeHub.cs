using log4net;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Hubs
{
    public class HomeHub : Hub
    {
        private readonly DataService _dataService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeHub));
        private readonly IHubContext<HomeHub> _hubContext;
        const int PERCENT = 100;

        public HomeHub(DataService dataService, IHubContext<HomeHub> hubContext)
        {
            _dataService = dataService;
            _hubContext = hubContext;
        }

        public async Task<StatisticalCalculationsResponse?> UpdateStatistical(string message)
        {
            try
            {
                StatisticalCalculationsResponse result = new StatisticalCalculationsResponse();

                int totalTray = await _dataService.GetTotalTray();
                double total = await _dataService.GetTotal();
                int totalOK = await _dataService.GettotalOK();
                int totalNG = await _dataService.GettotalNG();
                int totalEmpty = await _dataService.GetTotalEmpty();

                double percentChartOk = _dataService.CalculateChartOK(totalOK, total, totalEmpty);
                double percentChartNG = _dataService.CalculateChartNG(totalNG, total, totalEmpty);
                double percentChartEmpty = total == 0 ? 0 : Math.Round(PERCENT - percentChartNG - percentChartOk, 2);

                result.TotalTray = totalTray;
                result.Total = total;
                result.TotalOK = totalOK;
                result.TotalNG = totalNG;
                result.TotalEmpty = totalEmpty;

                result.PercentChartOk = percentChartOk;
                result.percentChartNG = percentChartNG;
                result.percentChartEmpty = percentChartEmpty;

                return result;

            } catch (Exception ex)
            {
                _logger.Error("Update statistical calculations failed: " + ex.Message);
                return null;
            }
        }

        public void ChangeDeepCoreVisionBusy(int clientId, int status)
        {
            try
            {
                _dataService.ChangeDeepLearningVisionBusy(clientId, status);
            } catch (Exception ex)
            {
                Console.WriteLine("Error can not change status vision busy:" + ex.Message);
            }
        }

        public void ChangeStatusCamVisionBusy(int clientId, int status)
        {
            try
            {
                _dataService.ChangeStatusCamVisionBusy(clientId, status);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error can not change status vision busy:" + ex.Message);
            }
        }

        public void ChangeConnectVisionBusy(int clientId, int status)
        {
            try
            {
                _dataService.ChangeConnectVisionBusy(clientId, status);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error can not change status vision busy:" + ex.Message);
            }
        }

        public void ChangeModel(string model)
        {
            try
            {
                Global._currentSelectedModel= model;

                Global.StatusVisionChangeModel1 = 1;
                Global.StatusVisionChangeModel2 = 1;
                Global.StatusVisionChangeModel3 = 1;
                Global.StatusVisionChangeModel4 = 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error can not change model:" + ex.Message);
            }
        }

        public async Task<SearchDataResponse> SearchData(string fromDate, string toDate, int page)
        {
            try
            {
                return await _dataService.SearchData(fromDate, toDate, page);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error can not get list data:" + ex.Message);
                throw;
            }
        }
    }
}
