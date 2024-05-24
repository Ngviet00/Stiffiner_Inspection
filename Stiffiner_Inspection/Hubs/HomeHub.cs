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
        const int PERCENT = 100;

        public HomeHub(DataService dataService)
        {
            _dataService = dataService;
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

            }
            catch (Exception ex)
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
            }
            catch (Exception ex)
            {
                _logger.Error("Error can not change status vision busy:" + ex.Message);
                throw;
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
                _logger.Error("Error can not change status vision busy:" + ex.Message);
                throw;
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
                _logger.Error("Error can not change status vision busy:" + ex.Message);
                throw;
            }
        }

        public async Task ChangeModel(string model)
        {
            try
            {
                Global._currentSelectedModel = model;
                await _dataService.WriteOneLine(Global.PathFileCurrentModel, model);
            }
            catch (Exception ex)
            {
                _logger.Error("Error can not change model:" + ex.Message);
                throw;
            }
        }

        public async Task<SearchDataResponse> SearchData(string fromDate, string toDate, int page, string model)
        {
            try
            {
                return await _dataService.SearchData(fromDate, toDate, page, model);
            }
            catch (Exception ex)
            {
                _logger.Error("Error can not get list data:" + ex.Message);
                throw;
            }
        }

        public async Task ReloadModels()
        {
            try
            {
                Global.Client1IsPostModel = 1;
                Global.Client2IsPostModel = 1;
                Global.Client3IsPostModel = 1;
                Global.Client4IsPostModel = 1;

                Global.strModels = string.Empty;
                Global._currentSelectedModel = string.Empty;
                Global.ListModels.Clear();

                await _dataService.WriteOneLine(Global.PathFileCurrentModel, string.Empty);
                await _dataService.WriteOneLine(Global.PathFileListModel, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.Error("Error can not change model:" + ex.Message);
                throw;
            }
        }

        public async Task ChangeModeRun(string mode)
        {
            try
            {
                Global.Mode = int.Parse(mode);
                await _dataService.WriteOneLine(Global.PathFileMode, mode);
            }
            catch (Exception ex)
            {
                _logger.Error("Error can not change model:" + ex.Message);
                throw;
            }
        }

        public void ResetCamClient(int client)
        {
            try
            {
                if (client == 1)
                {
                    Global.ResetCamClient1 = 1;
                }

                if (client == 2)
                {
                    Global.ResetCamClient2 = 1;
                }

                if (client == 3)
                {
                    Global.ResetCamClient3 = 1;
                }

                if (client == 4)
                {
                    Global.ResetCamClient4 = 1;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error can not save file log: " + ex.Message);
                throw;
            }
        }
    }
}
