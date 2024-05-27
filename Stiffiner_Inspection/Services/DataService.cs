using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using System.Globalization;
using CsvHelper;
using log4net;
using System.Net;
using Stiffiner_Inspection.Models.Response;
using System.Text;

namespace Stiffiner_Inspection.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<HistoryHub> _historyContext;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataService));
        private readonly IHubContext<HomeHub> _hubContext;

        const int CLIENT_1 = 1;
        const int CLIENT_2 = 2;
        const int CLIENT_3 = 3;
        const int CLIENT_4 = 4;

        const int OK = 1;
        const int NG = 2;
        const int EMPTY = 3;

        const int PERCENT = 100;

        public DataService(ApplicationDbContext dbContext, IHubContext<HomeHub> hubContext, IHubContext<HistoryHub> historyContext)
        {
            _dbContext = dbContext;
            _historyContext = historyContext;
            _hubContext = hubContext;
        }

        public int GetIndex(DataDTO dataDTO)
        {
            return dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2 ? dataDTO.index : dataDTO.index + 20;
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
                _logger.Error("Can not save images: " + ex.Message);
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
                _logger.Error("Can not save errors: " + ex.Message);
                throw;
            }
        }

        public int GetPosition(int index, int? clientId)
        {
            if (clientId == CLIENT_3 || clientId == CLIENT_4)
            {
                return index - 1;
            }

            return index + 19;
        }

        public int GetResult(int? result1, int? result2)
        {
            if (result1 == OK && result2 == OK)
            {
                return OK;
            }

            if (result1 == EMPTY && result2 == EMPTY)
            {
                return 0;
            }

            return NG;
        }

        public async Task SendToPLCV2(DataDTO dataDTO)
        {
            Global.CurrentTrayDataV2.Enqueue(dataDTO);

            if (Global.CurrentTrayDataV2.Count == 80)
            {
                List<DataCSV> dataCSV = [];

                for (int i = 1; i <= 20; i++)
                {
                    //pair left
                    var leftArea = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == CLIENT_1 && e.tray == Global.currentTray);
                    var leftLine = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == CLIENT_2 && e.tray == Global.currentTray);

                    //add to list to save excel
                    AddListPrepareSaveExcel(dataCSV, leftArea, leftLine);

                    //write register PLC
                    Global.controlPLC.WriteDataToRegister(GetResult(leftArea?.result, leftLine?.result), i - 1);

                    //save to db
                    await SaveToDB(leftArea, leftLine);

                    //pair right 
                    var rightArea = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == CLIENT_3 && e.tray == Global.currentTray);
                    var rightLine = Global.CurrentTrayDataV2.FirstOrDefault(e => e.index == i && e.client_id == CLIENT_4 && e.tray == Global.currentTray);

                    //add to list to save excel
                    AddListPrepareSaveExcel(dataCSV, rightArea, rightLine);

                    //write register PLC
                    Global.controlPLC.WriteDataToRegister(GetResult(rightArea?.result, rightLine?.result), i + 19);

                    //save to db
                    await SaveToDB(rightArea, rightLine);

                    //if enough 40 item => save to excel
                    if (dataCSV.Count == 40)
                    {
                        await SaveToExcel(dataCSV);
                    }
                }

                await _hubContext.Clients.All.SendAsync("RefreshData");

                //ater vision done, send signal
                Global.controlPLC.VisionDoneIns();

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
                index = dataArea?.client_id == CLIENT_1 || dataArea?.client_id == CLIENT_2 ? dataArea.index : dataArea.index + 20,
                result_area = dataArea?.result == 1 ? "OK" : (dataArea?.result == 2 ? "NG" : "Empty"),
                result_line = dataLine?.result == 1 ? "OK" : (dataLine?.result == 2 ? "NG" : "Empty"),
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
                _logger.Error("Can not save to file CSV: " + ex.Message);
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

            if (dataArea.result == NG || dataLine?.result == NG || (dataArea.result == OK && dataLine?.result == EMPTY) || (dataArea.result == EMPTY && dataLine?.result == OK))
            {
                await SaveImageV2(data, dataArea, dataLine);
                await SaveErrorV2(data, dataArea, dataLine);
            }
        }

        public async Task<int> GetTotal()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking().Where(d => d.ResultArea != null && d.ResultLine != null && d.TimeLine == Global.TimeLine).GroupBy(d => d.TargetId).Select(g => g.Count()).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Total: " + ex.Message);
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
                _logger.Error("Error Get Total Tray: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GetTotalEmpty()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking().Where(d => d.ResultArea == EMPTY && d.ResultLine == EMPTY && d.TimeLine == Global.TimeLine).GroupBy(d => d.TargetId).Select(g => g.Count()).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Total Empty: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GettotalOK()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking()
                  .Where(d => d.ResultArea == OK && d.ResultLine == OK && d.TimeLine == Global.TimeLine)
                  .GroupBy(d => d.TargetId)
                  .Select(g => g.Count())
                  .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Total OK: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GettotalNG()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking()
                .Where(d => d.TimeLine == Global.TimeLine && (
                    (d.ResultArea == NG && d.ResultLine == NG) || 
                    (d.ResultArea == NG && d.ResultLine == EMPTY) || 
                    (d.ResultArea == EMPTY && d.ResultLine == NG) ||
                    (d.ResultArea == OK && d.ResultLine == EMPTY) ||
                    (d.ResultArea == EMPTY && d.ResultLine == OK) ||
                    d.ResultLine == NG || d.ResultArea == NG))
                .GroupBy(d => d.TargetId)
                .Select(g => g.Count())
                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Total NG: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GetcurrTray()
        {
            int currTray = 0;
            int maxTray = await _dbContext.Data.AsNoTracking().Where(e => e.TimeLine == Global.TimeLine).OrderByDescending(x => x.Tray).Select(x => x.Tray).FirstOrDefaultAsync();

            var total = await _dbContext.Data
            .Where(d => d.ResultArea != null && d.ResultLine != null && d.Tray == maxTray && d.TimeLine == Global.TimeLine)
            .CountAsync();

            if (total >= 40)
            {
                currTray = maxTray++;
            }
            else
            {
                currTray = maxTray;
            }
            return currTray;
        }


        public double CalculateChartOK(int totalOK, double total, int totalEmpty)
        {
            return total == 0 ? 0 : Math.Round(totalOK / (total + totalEmpty) * PERCENT, 2);
        }

        public double CalculateChartNG(int totalNG, double total, int totalEmpty)
        {
            return total == 0 ? 0 : Math.Round(totalNG / (total + totalEmpty) * PERCENT, 2);
        }

        public double CalculateChartEmpty(double total, double percentNG, double percentOK)
        {
            return total == 0 ? 0 : Math.Round(100 - percentNG - percentOK, 2);
        }

        public List<ImageResponse>? DownloadFile(List<Image> images)
        {
            List<ImageResponse> imgsResponse = new List<ImageResponse>();

            try
            {
                string rootPath = @"D:\publish_image\images\";

                using (WebClient client = new WebClient())
                {
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
                _logger.Error("Error cannot download file: " + ex.ToString());
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
            if (clientId == CLIENT_1)
            {
                Global.StatusCam1 = status;
                return;
            }

            if (clientId == CLIENT_2)
            {
                Global.StatusCam2 = status;
                return;
            }

            if (clientId == CLIENT_3)
            {
                Global.StatusCam3 = status;
                return;
            }

            if (clientId == CLIENT_4)
            {
                Global.StatusCam4 = status;
                return;
            }
        }

        public void ChangeConnectVisionBusy(int clientId, int status)
        {
            if (clientId == CLIENT_1)
            {
                Global.ConnectCam1 = status;
                return;
            }

            if (clientId == CLIENT_2)
            {
                Global.ConnectCam2 = status;
                return;
            }

            if (clientId == CLIENT_3)
            {
                Global.ConnectCam3 = status;
                return;
            }

            if (clientId == CLIENT_4)
            {
                Global.ConnectCam4 = status;
                return;
            }
        }

        public void ChangeDeepLearningVisionBusy(int clientId, int status)
        {
            if (clientId == CLIENT_1)
            {
                Global.DeepLearningCam1 = status;
                return;
            }

            if (clientId == CLIENT_2)
            {
                Global.DeepLearningCam2 = status;
                return;
            }

            if (clientId == CLIENT_3)
            {
                Global.DeepLearningCam3 = status;
                return;
            }

            if (clientId == CLIENT_4)
            {
                Global.DeepLearningCam4 = status;
                return;
            }
        }

        public async Task<SearchDataResponse> SearchData(string fromDate, string toDate, int page, string model)
        {
            try
            {
                int pageSize = 200;

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

                string sqlTotalTray = "SELECT DISTINCT tray FROM data WHERE 1 = 1";

                if (!string.IsNullOrWhiteSpace(model))
                {
                    sqlTotalTray += $" AND model = '{model}'";
                }

                sqlTotalTray += " AND CONVERT(VARCHAR(16), time, 120) >= {0} and CONVERT(VARCHAR(16), time, 120) <= {1}";

                var totalTray = await _dbContext.Data
                    .FromSqlRaw(sqlTotalTray, fromDate, toDate)
                    .CountAsync();

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
            catch (Exception e)
            {
                _logger.Error("Error cannot get data: " + e.Message);
                throw;
            }
        }

        public List<string> GetListModelsAppearFourTime(string models)
        {
            string[] elements = models.Split(',');

            //get item appear four time and push to list
            return elements.GroupBy(x => x).Where(g => g.Count() == 4).Select(g => g.Key).ToList();
        }

        public async Task WriteOneLine(string path, string content)
        {
            try
            {
                await File.WriteAllTextAsync(path, content);
            }
            catch (Exception ex)
            {
                _logger.Error("Can not write line: " + ex.Message);
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
                _logger.Error("Can not write line: " + ex.Message);
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
                _logger.Error("Can not read line: " + ex.Message);
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
                _logger.Error("Can not read many line: " + ex.Message);
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
                _logger.Error("Error can not delete all data: " + ex.Message);
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
                _logger.Error("Error Get List History: " + ex.Message);
                return null;
            }
        }
    }
}
