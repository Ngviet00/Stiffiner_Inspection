using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Commons;
using Stiffiner_Inspection.Models.Response;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Hubs
{
    public class HomeHub : Hub
    {
        private readonly DataService _dataService;

        public HomeHub(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<StatisticalCalculationsResponse?> UpdateStatistical(string message)
        {
            try
            {
                StatisticalCalculationsResponse result = new StatisticalCalculationsResponse();

                double total = await _dataService.GetTotal();
                int totalOK = await _dataService.GettotalOK();
                int totalEmpty = await _dataService.GetTotalEmpty();
                int totalNG = (int)(total - totalOK - totalEmpty);

                double percentChartOk = _dataService.CalculateChartOK(totalOK, total, totalEmpty);
                double percentChartNG = _dataService.CalculateChartNG(totalNG, total, totalEmpty);
                double percentChartEmpty = total == 0 ? 0 : Math.Round(Constants.PERCENT - percentChartNG - percentChartOk, 2);

                result.TotalTray = (int)(total > 0 ? total/40 : 0);
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
                Log.Error($"Update statistical calculations failed: {ex.Message}");
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
                Log.Error($"Error can not change status vision busy: {ex.Message}");
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
                Log.Error($"Error can not change status vision busy: {ex.Message}");
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
                Log.Error($"Error can not change status vision busy: {ex.Message}");
                throw;
            }
        }

        public void ChangeModel(string model)
        {
            try
            {
                Global._currentSelectedModel = model;
                Global.WriteFileToTxt(Global.PathFileSetting, new Dictionary<string, string> { { "current_model", model } });
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not change model: {ex.Message}");
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
                Log.Error($"Error can not get list data: {ex.Message}");
                throw;
            }
        }

        public async Task ReloadModels()
        {
            try
            {
                Global.Client1IsPostModel = Constants.ACTIVE;
                Global.Client2IsPostModel = Constants.ACTIVE;
                Global.Client3IsPostModel = Constants.ACTIVE;
                Global.Client4IsPostModel = Constants.ACTIVE;

                Global.strModels = string.Empty;
                Global._currentSelectedModel = string.Empty;
                Global.ListModels.Clear();

                Global.WriteFileToTxt(Global.PathFileSetting, new Dictionary<string, string> { { "current_model", string.Empty} });
                await _dataService.WriteOneLine(Global.PathFileListModel, string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not change model: {ex.Message}");
                throw;
            }
        }

        public void ChangeModeRun(string mode)
        {
            try
            {
                Global.Mode = int.Parse(mode);
                Global.WriteFileToTxt(Global.PathFileSetting, new Dictionary<string, string>
                {
                    { "mode", mode }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not change model: {ex.Message}");
                throw;
            }
        }

        public void ResetCamClient(int client)
        {
            try
            {
                if (client == Constants.CLIENT_1)
                {
                    Global.ResetCamClient1 = Constants.ACTIVE;
                }

                if (client == Constants.CLIENT_2)
                {
                    Global.ResetCamClient2 = Constants.ACTIVE;
                }

                if (client == Constants.CLIENT_3)
                {
                    Global.ResetCamClient3 = Constants.ACTIVE;
                }

                if (client == Constants.CLIENT_4)
                {
                    Global.ResetCamClient4 = Constants.ACTIVE;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not save file log: {ex.Message}");
                throw;
            }
        }
    }
}
