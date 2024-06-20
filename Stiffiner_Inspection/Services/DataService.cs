using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using System.Globalization;
using CsvHelper;
using System.Net;
using Stiffiner_Inspection.Models.Response;
using System.Text;
using Stiffiner_Inspection.Commons;

namespace Stiffiner_Inspection.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<HistoryHub> _historyContext;
        private readonly IHubContext<HomeHub> _hubContext;

        public DataService(ApplicationDbContext dbContext, IHubContext<HomeHub> hubContext, IHubContext<HistoryHub> historyContext)
        {
            _dbContext = dbContext;
            _historyContext = historyContext;
            _hubContext = hubContext;
        }

        public int GetIndex(DataDTO dataDTO)
        {
            return dataDTO.client_id == Constants.CLIENT_1 || dataDTO.client_id == Constants.CLIENT_2 ? dataDTO.index : dataDTO.index + 20;
        }

        private async Task SaveImageV2(Data data, DataDTO dataArea, DataDTO dataLine)
        {
            try
            {
                List<Image> listImages = new List<Image>();

                string[]? imgArea = dataArea?.image?.Split(',');
                string[]? imgLine = dataLine?.image?.Split(',');

                foreach (string item in imgArea)
                {
                    if (!string.IsNullOrWhiteSpace(item) && item.Trim() != "No_save")
                    {
                        listImages.Add(new Image
                        {
                            DataId = data.Id,
                            Path = item,
                            ClientId = (int)dataArea.client_id
                        });
                    }
                }

                foreach (string item in imgLine)
                {
                    if (!string.IsNullOrWhiteSpace(item) && item.Trim() != "No_save")
                    {
                        listImages.Add(new Image
                        {
                            DataId = data.Id,
                            Path = item,
                            ClientId = (int)dataLine.client_id
                        });
                    }
                }

                await _dbContext.Images.AddRangeAsync(listImages);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Can not save images: {ex.Message}");
                throw;
            }
        }

        public async Task SaveErrorV2(Data data, DataDTO dataArea, DataDTO dataLine)
        {
            try
            {
                List<Error> listErrs = new List<Error>();

                string[]? errorsArea = dataArea?.error?.Trim(',')?.Split(',');
                string[]? errorsLine = dataLine?.error?.Trim(',')?.Split(',');

                foreach (string item in errorsArea)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        listErrs.Add(new Error
                        {
                            DataId = data.Id,
                            Description = item,
                            Type = (int)dataArea.client_id, //(1,3 type area, 2,4 type line)
                        });
                    }
                    
                }

                foreach (string item in errorsLine)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        listErrs.Add(new Error
                        {
                            DataId = data.Id,
                            Description = item,
                            Type = (int)dataLine.client_id, //(1,3 type area, 2,4 type line)
                        });
                    }
                }

                await _dbContext.Errors.AddRangeAsync(listErrs);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Can not save errors: {ex.Message}");
                throw;
            }
        }

        public int GetPosition(int index, int? clientId)
        {
            if (clientId == Constants.CLIENT_3 || clientId == Constants.CLIENT_4)
            {
                return index - 1;
            }

            return index + 19;
        }

        public int GetResult(int? result1, int? result2)
        {
            if (result1 == Constants.OK && result2 == Constants.OK)
            {
                return Constants.OK;
            }

            if (result1 == Constants.EMPTY && result2 == Constants.EMPTY)
            {
                return 0;
            }

            return Constants.NG;
        }

        public async Task SendToPLCV2(DataDTO dataDTO)
        {
            Global.CurrentTrayDataV2.Enqueue(dataDTO);

            if (Global.CurrentTrayDataV2.Count == 80)
            {
                List<DataCSV> dataCSV = [];

                int ok = 0;
                int ng = 0;
                int empty = 0;

                for (int i = 1; i <= 20; i++)
                {
                    //pair left
                    var leftArea = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == Constants.CLIENT_1 && e.tray == Global.currentTray);
                    var leftLine = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == Constants.CLIENT_2 && e.tray == Global.currentTray);

                    var rsLeft = GetResult(leftArea?.result, leftLine?.result);

                    switch (rsLeft)
                    {
                        case 1:
                            ok += 1;
                            break;
                        case 2:
                            ng += 1;
                            break;
                        case 0:
                            empty += 1;
                            break;
                    }

                    //add to list to save excel
                    AddListPrepareSaveExcel(dataCSV, leftArea, leftLine);

                    //write register PLC
                    Global.controlPLC.WriteDataToRegister(rsLeft, i - 1);

                    //save to db
                    await SaveToDB(leftArea, leftLine);

                    //pair right 
                    var rightArea = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == Constants.CLIENT_3 && e.tray == Global.currentTray);
                    var rightLine = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == Constants.CLIENT_4 && e.tray == Global.currentTray);

                    var rsRight = GetResult(rightArea?.result, rightLine?.result);

                    switch (rsRight)
                    {
                        case 1:
                            ok += 1;
                            break;
                        case 2:
                            ng += 1;
                            break;
                        case 0:
                            empty += 1;
                            break;
                    }

                    //add to list to save excel
                    AddListPrepareSaveExcel(dataCSV, rightArea, rightLine);

                    //write register PLC
                    Global.controlPLC.WriteDataToRegister(rsRight, i + 19);

                    //save to db
                    await SaveToDB(rightArea, rightLine);
                }

                //ater vision done, send signal
                Global.controlPLC.VisionDoneIns();

                Global.Total += 40;
                Global.TotalOK += ok;
                Global.TotalNG += ng;
                Global.TotalEmpty += empty;

                await _hubContext.Clients.All.SendAsync("RefreshData", Global.Total, Global.TotalOK, Global.TotalNG, Global.TotalEmpty);

                if (dataCSV.Count == 40)
                {
                    await SaveToExcel(dataCSV);
                }

                //after vision done, call method refresh data in history page
                await _historyContext.Clients.All.SendAsync("RefreshData");
            }
        }

        public void AddListPrepareSaveExcel(List<DataCSV> dataCSV, DataDTO? dataArea, DataDTO? dataLine)
        {
            dataCSV.Add(new DataCSV
            {
                model = Global._currentSelectedModel,
                time = dataArea?.time,
                index = dataArea?.client_id == Constants.CLIENT_1 || dataArea?.client_id == Constants.CLIENT_2 ? dataArea.index : dataArea.index + 20,
                result_area = dataArea?.result == Constants.OK ? "OK" : (dataArea?.result == Constants.NG ? "NG" : "Empty"),
                result_line = dataLine?.result == Constants.OK ? "OK" : (dataLine?.result == Constants.NG ? "NG" : "Empty"),
                image = dataArea?.image + "," + dataLine?.image,
                errors = dataArea?.error + "," + dataLine?.error
            });
        }

        private async Task SaveToExcel(List<DataCSV> dataCSV)
        {
            string directoryPath = @"D:\Export_Result\" + DateTime.Now.ToString(@"yyyy_MM_dd");
            string model = "_" + Global._currentSelectedModel + ".csv";
            string fileNameCSV = "MAY_1_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + model;
            string filePath = Path.Combine(directoryPath, fileNameCSV);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(dataCSV);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Can not save to file CSV: {ex.Message}");
            }
        }

        public async Task SaveToDB(DataDTO? dataArea, DataDTO? dataLine)
        {
            var data = new Data
            {
                Time = dataArea?.time,
                Model = dataArea?.model,
                Tray = dataArea.tray,
                ClientId = dataArea.client_id,
                Side = dataArea.side,
                Camera = dataArea.camera,
                TargetId = 0,
                ResultArea = dataArea.result,
                ResultLine = dataLine?.result,
                Index = GetIndex(dataArea),
                TimeLine = Global.TimeLine,
            };

            await _dbContext.Data.AddAsync(data);
            await _dbContext.SaveChangesAsync();

            if (dataArea.result == Constants.NG || dataLine?.result == Constants.NG || (dataArea.result == Constants.OK && dataLine?.result == Constants.EMPTY) || (dataArea.result == Constants.EMPTY && dataLine?.result == Constants.OK))
            {
                await SaveImageV2(data, dataArea, dataLine);
                await SaveErrorV2(data, dataArea, dataLine);
            }
        }

        public async Task<int> GetTotal()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking().Where(d => d.ResultArea != null && d.ResultLine != null && d.TimeLine == Global.TimeLine).CountAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Error Get Total: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalTray()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking().Where(e => e.TimeLine == Global.TimeLine).Select(d => d.Tray).Distinct().CountAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Error Get Total Tray: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalEmpty()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking().Where(d => d.ResultArea == Constants.EMPTY && d.ResultLine == Constants.EMPTY && d.TimeLine == Global.TimeLine).CountAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Error Get Total Empty: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GettotalOK()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking().Where(d => d.ResultArea == Constants.OK && d.ResultLine == Constants.OK && d.TimeLine == Global.TimeLine).CountAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Error Get Total OK: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GettotalNG()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking()
                .Where(d => d.TimeLine == Global.TimeLine && (
                    (d.ResultArea == Constants.NG && d.ResultLine == Constants.NG) ||
                    (d.ResultArea == Constants.NG && d.ResultLine == Constants.EMPTY) ||
                    (d.ResultArea == Constants.EMPTY && d.ResultLine == Constants.NG) ||
                    (d.ResultArea == Constants.OK && d.ResultLine == Constants.EMPTY) ||
                    (d.ResultArea == Constants.EMPTY && d.ResultLine == Constants.OK) ||
                    d.ResultLine == Constants.NG || d.ResultArea == Constants.NG)).CountAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Error Get Total NG: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetcurrTray()
        {
            var maxTray = await _dbContext.Data.Where(d => d.TimeLine == Global.TimeLine).MaxAsync(d => (int?)d.Tray);

            if (maxTray != null)
            {
                return (int)maxTray;
            }

            return 0;
        }


        public double CalculateChartOK(int totalOK, double total, int totalEmpty)
        {
            return total == 0 ? 0 : Math.Round(totalOK / (total + totalEmpty) * Constants.PERCENT, 2);
        }

        public double CalculateChartNG(int totalNG, double total, int totalEmpty)
        {
            return total == 0 ? 0 : Math.Round(totalNG / (total + totalEmpty) * Constants.PERCENT, 2);
        }

        public double CalculateChartEmpty(double total, double percentNG, double percentOK)
        {
            return total == 0 ? 0 : Math.Round(Constants.PERCENT - percentNG - percentOK, 2);
        }

        public List<ImageResponse>? DownloadFile(List<Image> images)
        {
            List<ImageResponse> imgsResponse = new List<ImageResponse>();

            try
            {
                string rootPath = @"D:\publish_image\images\";

                using (WebClient client = new WebClient())
                {
                    if (!Directory.Exists(rootPath))
                    {
                        Directory.CreateDirectory(rootPath);
                    }

                    foreach (var item in images)
                    {
                        if (item?.Path?.Trim() != "No_save")
                        {
                            string imageUrl = GetImageRemote(item);

                            string fileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".bmp";

                            try
                            {
                                client.DownloadFile(imageUrl, rootPath + fileName);

                                imgsResponse.Add(new ImageResponse
                                {
                                    client_id = item.ClientId,
                                    path = "https://localhost:8089/images/" + fileName
                                });
                            }
                            catch (WebException webException)
                            {
                                throw;
                            }
                        }
                    }
                }

                return imgsResponse;
            }
            catch (Exception ex)
            {
                Log.Error($"Error cannot download file: {ex.Message}");
            }

            return imgsResponse;
           
        }

        public string GetImageRemote(Image? img)
        {
            return FormatClient(img.ClientId) + ConvertPathImage(img?.Path);
        }

        public string FormatClient(int? clientId)
        {
            switch (clientId)
            {
                case 1:
                    return "http://192.168.1.11:8881/";
                case 2:
                    return "http://192.168.1.22:8881/";
                case 3:
                    return "http://192.168.1.33:8881/";
                default:
                    return "http://192.168.1.44:8881/";
            }
        }

        static string ConvertPathImage(string? fullPath)
        {
            fullPath = fullPath?.Trim();
            int index = fullPath.IndexOf("ScreenCapture");

            if (index != -1)
            {
                string relativePath = fullPath.Substring(index);
                relativePath = relativePath.Replace('\\', '/');
                return relativePath;
            }
            else
            {
                return fullPath;
            }
        }

        public async Task RefreshHistoryWhenClearData()
        {
            await _historyContext.Clients.All.SendAsync("RefreshData");
        }

        public void ChangeStatusCamVisionBusy(int clientId, int status)
        {
            if (clientId == Constants.CLIENT_1)
            {
                Global.StatusCam1 = status;
                return;
            }

            if (clientId == Constants.CLIENT_2)
            {
                Global.StatusCam2 = status;
                return;
            }

            if (clientId == Constants.CLIENT_3)
            {
                Global.StatusCam3 = status;
                return;
            }

            if (clientId == Constants.CLIENT_4)
            {
                Global.StatusCam4 = status;
                return;
            }
        }

        public void ChangeConnectVisionBusy(int clientId, int status)
        {
            if (clientId == Constants.CLIENT_1)
            {
                Global.ConnectCam1 = status;
                return;
            }

            if (clientId == Constants.CLIENT_2)
            {
                Global.ConnectCam2 = status;
                return;
            }

            if (clientId == Constants.CLIENT_3)
            {
                Global.ConnectCam3 = status;
                return;
            }

            if (clientId == Constants.CLIENT_4)
            {
                Global.ConnectCam4 = status;
                return;
            }
        }

        public void ChangeDeepLearningVisionBusy(int clientId, int status)
        {
            if (clientId == Constants.CLIENT_1)
            {
                Global.DeepLearningCam1 = status;
                return;
            }

            if (clientId == Constants.CLIENT_2)
            {
                Global.DeepLearningCam2 = status;
                return;
            }

            if (clientId == Constants.CLIENT_3)
            {
                Global.DeepLearningCam3 = status;
                return;
            }

            if (clientId == Constants.CLIENT_4)
            {
                Global.DeepLearningCam4 = status;
                return;
            }
        }

        public async Task<SearchDataResponse> SearchData(string fromDate, string toDate, int page, string model)
        {
            try
            {
                int pageSize = 20;

                SearchDataResponse response = new SearchDataResponse();

                StringBuilder baseSql = new StringBuilder();

                baseSql.Append(@"SELECT * FROM data WHERE 1 = 1");

                if (!string.IsNullOrWhiteSpace(model))
                {
                    baseSql.Append($" AND model = '{model}' ");
                }

                baseSql.Append(" AND CONVERT(VARCHAR(16), time, 120) >= {0} and CONVERT(VARCHAR(16), time, 120) <= {1} and result_area is not null and result_line is not null ");

                var total = await _dbContext.Data
                    .FromSqlRaw(baseSql.ToString(), fromDate, toDate)
                    .CountAsync();

                var countOK = await _dbContext.Data
                    .FromSqlRaw(baseSql.ToString() + "AND result_area = 1 and result_line = 1 ", fromDate, toDate)
                    .CountAsync();

                var countNG = await _dbContext.Data
                    .FromSqlRaw(@baseSql.ToString() + "AND ((result_area = 2 or result_line = 2) or (result_area = 1 and result_line = 3) or (result_area = 3 and result_line = 1)) ", fromDate, toDate)
                    .CountAsync();

                var countEmpty = await _dbContext.Data
                    .FromSqlRaw(baseSql.ToString() + "AND result_area = 3 and result_line = 3 ", fromDate, toDate)
                    .CountAsync();

                var totalTray = (int)Math.Floor((double)total / 40);

                var data = await _dbContext.Data
                    .FromSqlRaw(baseSql.ToString(), fromDate, toDate)
                    .Include(p => p.Errors)
                    .Include(p => p.Images)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Total = total;
                response.TotalOK = countOK;

                response.TotalNG = countNG;
                response.TotalEmpty = countEmpty;

                response.TotalTray = totalTray;

                response.PercentOK = CalculateChartOK(countOK, total, countEmpty);
                response.PercentNG = CalculateChartNG(countNG, total, countEmpty);
                response.PercentEmpty = CalculateChartEmpty(total, response.PercentNG, response.PercentOK);

                response.results = data;

                return response;
            }
            catch (Exception ex)
            {
                Log.Error($"Error cannot get data: {ex.Message}");
                throw;
            }
        }

        public List<string> GetListModelsAppearFourTime(string models)
        {
            string[] elements = models.Split(',');

            //get item appear four time and push to list
            return elements.GroupBy(x => x).Where(g => g.Count() == 4).Select(g => g.Key).ToList();
        }

        public async Task WriteOneLine(string path, string? content)
        {
            try
            {
                await File.WriteAllTextAsync(path, content);
            }
            catch (Exception ex)
            {
                Log.Error($"Can not write line: {ex.Message}");
                throw;
            }
        }

        public async Task WriteManyLine(string path, List<string> content)
        {
            try
            {
                await File.WriteAllLinesAsync(path, content);
            }
            catch (Exception ex)
            {
                Log.Error($"Can not write line: {ex.Message}");
                throw;
            }
        }

        public async Task<string> ReadOneLine(string path)
        {
            try
            {
                return await File.ReadAllTextAsync(path);
            }
            catch (Exception ex)
            {
                Log.Error($"Can not read line: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> ReadManyLine(string path)
        {
            try
            {
                List<string> result = new List<string>();

                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        result.Add(line);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Can not read many line: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAllData()
        {
            try
            {
                _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE errors");
                _dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE images");
                _dbContext.Database.ExecuteSqlRaw("DELETE FROM data");
                _dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('stiffiner_inspection.dbo.data', RESEED, 0)");

                string folderPath = @"D:\publish_image\images";

                // Check if exist folder => delete => create new folder
                if (Directory.Exists(folderPath))
                {
                    await Task.Run(() => Directory.Delete(folderPath, true));
                    Directory.CreateDirectory(folderPath);
                }
                else
                {
                    Directory.CreateDirectory(folderPath);
                }

                await RefreshHistoryWhenClearData();
            }
            catch (Exception ex)
            {
                Log.Error($"Error can not delete all data: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Data>?> GetHistoryBySide(string side)
        {
            try
            {
                return await _dbContext.Data
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(e => e.TimeLine == Global.TimeLine && e.Side == side && ((e.ResultLine == 2 || e.ResultArea == 2) || (e.ResultArea == 1 && e.ResultLine == 3) || (e.ResultArea == 3 && e.ResultLine == 1)))
                    .OrderByDescending(x => x.Id)
                    .OrderByDescending(x => x.Tray)
                    .Include(p => p.Errors)
                    .Include(p => p.Images)
                    .Take(200)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                Log.Error($"Error Get List History: {ex.Message}");
                return null;
            }
        }
    }
}
