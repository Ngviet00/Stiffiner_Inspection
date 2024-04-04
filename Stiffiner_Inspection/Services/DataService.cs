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
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Text;
using Microsoft.Ajax.Utilities;

namespace Stiffiner_Inspection.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataService));

        const int CLIENT_1 = 1;
        const int CLIENT_2 = 2;
        const int CLIENT_3 = 3;
        const int CLIENT_4 = 4;

        const int OK = 1;
        const int NG = 2;
        const int EMPTY = 3;

        const int PERCENT = 100;

        public DataService(ApplicationDbContext dbContext, IHubContext<HomeHub> hubContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Data> Save(DataDTO dataDTO)
        {
            //get index
            var indexItem = GetIndex(dataDTO);

            //check exist
            var existEntity = await _dbContext.Data.Where(e => e.TargetId == Global.currentTargetId && e.Tray == Global.currentTray && e.Index == indexItem).FirstOrDefaultAsync();

            if (existEntity != null)
            {
                if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_3)
                {
                    existEntity.ResultArea = dataDTO.result;
                }
                else
                {
                    existEntity.ResultLine = dataDTO.result;
                }

                if (dataDTO.result == NG)
                {
                    //save image
                    await SaveImage(existEntity, dataDTO);

                    //save error
                    await SaveError(existEntity, dataDTO.error);
                }

                await _dbContext.SaveChangesAsync();

                return existEntity;
            }
            else
            {
                var data = new Data
                {
                    Id = dataDTO.id,
                    Time = dataDTO.time,
                    Model = dataDTO.model,
                    Tray = dataDTO.tray,
                    ClientId = dataDTO.client_id,
                    Side = dataDTO.side,
                    Camera = dataDTO.camera,
                    TargetId = Global.currentTargetId
                };

                //set index từ 1 đến 40 tính từ bên phải, từ trên xuống dưới
                data.Index = indexItem;

                //client là 1 hoặc 3 là cam area, client 2 hoặc 4 là cam line
                if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_3)
                {
                    data.ResultArea = dataDTO.result;
                }
                else
                {
                    data.ResultLine = dataDTO.result;
                }

                await _dbContext.Data.AddAsync(data);
                await _dbContext.SaveChangesAsync();

                //nếu như là NG thì sẽ lưu lỗi vào bảng error và lưu hình ảnh vào bảng image
                if (dataDTO.result == NG)
                {
                    //save image
                    await SaveImage(data, dataDTO);

                    //save error
                    await SaveError(data, dataDTO.error);
                }

                return data;
            }
        }

        public int GetIndex(DataDTO dataDTO)
        {
            return dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2 ? dataDTO.index + 20 : dataDTO.index;
        }

        private async Task SaveImage(Data data, DataDTO dataDto)
        {
            List<Image> listImages = new List<Image>();

            string[] imgs = dataDto.image.Split(',');

            foreach (string item in imgs)
            {
                listImages.Add(new Image
                {
                    DataId = data.Id,
                    Path = item,
                    ClientId = (int)dataDto.client_id
                });
            }

            await _dbContext.Images.AddRangeAsync(listImages);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveError(Data data, string listErrors)
        {
            List<Models.Entity.Error> listErrs = new List<Models.Entity.Error>();

            string[] errors = listErrors.Split(',');

            foreach (string item in errors)
            {
                listErrs.Add(new Models.Entity.Error
                {
                    DataId = data.Id,
                    Description = item,
                    Type = data.ClientId == CLIENT_1 || data.ClientId == CLIENT_3 ? 1 : 2, //(1,3 type area, 2,4 type line)
                });
            }

            await _dbContext.Errors.AddRangeAsync(listErrs);
            await _dbContext.SaveChangesAsync();
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

            if (result1 == EMPTY && result2 == OK || result1 == OK && result2 == EMPTY)
            {
                return OK;
            }

            if (result1 == EMPTY && result2 == NG || result1 == NG && result2 == EMPTY)
            {
                return NG;
            }

            if (result1 == EMPTY && result2 == EMPTY)
            {
                return 0;
            }

            return NG;
        }

        public int GetClientIdPair(DataDTO dataDTO)
        {
            switch (dataDTO.client_id)
            {
                case CLIENT_1:
                    return CLIENT_2;

                case CLIENT_2:
                    return CLIENT_1;

                case CLIENT_3:
                    return CLIENT_4;

                default:
                    return CLIENT_3;
            }
        }

        public async Task SendToPLC(DataDTO dataDTO)
        {
            //=================== TEST ===================
            Global.CurrentTrayData.Add(dataDTO);

            if (Global.CurrentTrayData.Count == 80)
            {
                List<DataCSV> dataCSV = [];

                for (int i = 1; i <= 20; i++)
                {
                    //pair left 
                    //await save to db
                    //check if Ng => save image, error
                    var leftArea = Global.CurrentTrayData.Find(e => e.index == i && e.client_id == CLIENT_1 && e.tray == Global.currentTray);
                    
                    var leftLine = Global.CurrentTrayData.Find(e => e.index == i && e.client_id == CLIENT_2 && e.tray == Global.currentTray);
                    AddListPrepareSaveExcel(dataCSV, leftArea, leftLine); //add to list
                    Global.controlPLC.WriteDataToRegister(GetResult(leftArea?.result, leftLine?.result), i - 1); //write register PLC

                    //pair left 
                    //await save to db area and line
                    //check if Ng => save image, error
                    var rightArea = Global.CurrentTrayData.Find(e => e.index == i && e.client_id == CLIENT_3 && e.tray == Global.currentTray);
                    var rightLine = Global.CurrentTrayData.Find(e => e.index == i && e.client_id == CLIENT_4 && e.tray == Global.currentTray);
                    AddListPrepareSaveExcel(dataCSV, rightArea, rightLine); //add to list
                    Global.controlPLC.WriteDataToRegister(GetResult(rightArea?.result, rightLine?.result), i + 19); //write register PLC

                    //if enough 40 item => save to excel
                    if (dataCSV.Count == 40)
                    {
                        await SaveToExcel(dataCSV);
                    }
                }

                //ater vision done, send signal
                Global.controlPLC.VisionDoneIns();
            }
        }

        public void AddListPrepareSaveExcel(List<DataCSV> dataCSV, DataDTO? dataArea, DataDTO? dataLine)
        {
            dataCSV.Add(new DataCSV
            {
                model = "Stiffiner",
                time = dataArea?.time,
                index = dataArea?.client_id == CLIENT_1 || dataArea?.client_id == CLIENT_2 ? dataArea.index : dataArea.index + 20,
                result_area = dataArea.result == 1 ? "OK" : (dataArea.result == 2 ? "NG" : "Empty"),
                result_line = dataLine?.result == 1 ? "OK" : (dataLine?.result == 2 ? "NG" : "Empty"),
                image = dataArea?.image + "," + dataLine?.image,
                errors = dataArea?.error + "," + dataLine?.error
            });
        }

        private async Task SaveToExcel(List<DataCSV> dataCSV)
        {
            string directoryPath = @"D:\Export_Result\" + DateTime.Now.ToString(@"yyyy_MM_dd");
            string fileNameCSV = "MAY_1_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "_stiffiner.csv";
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

        public async Task<long> GetCurrentTargetID()
        {
            try
            {
                return await _dbContext.Targets
                .AsNoTracking()
                .OrderByDescending(t => t.TargetId)
                .Select(t => t.TargetId)
                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Current Target ID: " + ex.Message);
                return 0;
            }

        }

        public async Task<int> GetCurrentTargetQty(long currTargetid)
        {
            try
            {
                int targetQty = 0;

                var target = await _dbContext.Targets
                    .AsNoTracking()
                    .Where(t => t.TargetId == currTargetid)
                    .Select(t => t.Target_qty)
                    .FirstOrDefaultAsync();

                if (target != null)
                {
                    targetQty = target;
                }

                return targetQty;
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Current Target Qty: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GetTotal(long targetId)
        {
            try
            {
                return await _dbContext.Data
                 .AsNoTracking()
                 .Where(d => d.TargetId == targetId &&
                             d.ResultArea != null &&
                             d.ResultLine != null)
                 .GroupBy(d => d.TargetId)
                 .Select(g => g.Count())
                 .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Total: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GetTotalTray(long currtarget)
        {
            try
            {
                return await _dbContext.Data.AsNoTracking().Where(d => d.TargetId == currtarget).Select(d => d.Tray).Distinct().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Total Tray: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GetTotalEmpty(long currtarget)
        {
            try
            {
                return await _dbContext.Data.AsNoTracking()
              .Where(d => d.TargetId == currtarget &&
               d.ResultArea != null &&
               d.ResultLine != null && (d.ResultArea == EMPTY || d.ResultLine == EMPTY))
              .GroupBy(d => d.TargetId)
              .Select(g => g.Count())
              .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Total Empty: " + ex.Message);
                return 0;
            }
        }

        public async Task<int> GettotalOK(long currtarget)
        {
            try
            {
                return await _dbContext.Data.AsNoTracking()
                  .Where(d => d.TargetId == currtarget && d.ResultArea == OK && d.ResultLine == OK && d.ResultArea != null && d.ResultLine != null)
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

        public async Task<int> GettotalNG(long currtarget)
        {
            try
            {
                return await _dbContext.Data.AsNoTracking()
                .Where(d => d.TargetId == currtarget && d.ResultArea != null && d.ResultLine != null &&
                            ((d.ResultArea == NG && d.ResultLine != EMPTY) || (d.ResultLine == NG && d.ResultArea != EMPTY)))
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

        public async Task<int> GetcurrTray(long currtarget)
        {
            int currTray = 0;
            int maxTray = await _dbContext.Data.AsNoTracking().Where(x => x.TargetId == currtarget).OrderByDescending(x => x.Tray).Select(x => x.Tray).FirstOrDefaultAsync();

            var total = await _dbContext.Data
            .Where(d => d.TargetId == currtarget && d.ResultArea != null && d.ResultLine != null && d.Tray == maxTray)
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

        public async Task<List<Data>?> GetHistory()
        {
            try
            {
                return await _dbContext.Data
                    .AsNoTracking()
                    .OrderBy(e => e.Index)
                    .Include(p => p.Errors)
                    .Include(p => p.Images)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get List History: " + ex.Message);
                return null;
            }
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
            try
            {
                List<ImageResponse> imgsResponse = new List<ImageResponse>();

                string rootPath = @"D:\publish_image\images\";

                using (WebClient client = new WebClient())
                {
                    foreach (var item in images)
                    {
                        if (item?.Path?.Trim() != "No_save")
                        {
                            string imageUrl = GetImageRemote(item);

                            string fileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".bmp";

                            client.DownloadFile(imageUrl, rootPath + fileName);

                            imgsResponse.Add(new ImageResponse
                            {
                                client_id = item.ClientId,
                                path = "https://localhost:8089/images/" + fileName
                            });
                        }
                    }
                }

                return imgsResponse;
            }
            catch (Exception ex)
            {
                _logger.Error("Error cannot download file: " + ex.ToString());
                return null;
            }
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
    }
}
