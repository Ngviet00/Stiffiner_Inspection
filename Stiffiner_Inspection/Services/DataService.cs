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
            List<Error> listErrs = new List<Error>();

            string[] errors = listErrors.Split(',');

            foreach (string item in errors)
            {
                listErrs.Add(new Error
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
            //nếu client = 1 => 2, 2 => 1, 3 => 4 và ngược lại
            var clientIdPair = GetClientIdPair(dataDTO);

            Global._currentTray.Add(dataDTO);

            if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2)
            {
                //client 1, 2 thêm vào tray left
                Global.currentTrayLeft.Add(dataDTO);

                //tìm kiếm nếu có đủ area và line
                var exist = Global.currentTrayLeft.Find(e => e.tray == Global.currentTray && e.client_id == clientIdPair && e.index == dataDTO.index && e.side == dataDTO.side);

                if (exist is not null)
                {
                    await ExportDataToCsvRow(dataDTO, exist);
                }
            }
            else
            {
                //client 3, 4 thêm vào tray right
                Global.currentTrayRight.Add(dataDTO);

                //tìm kiếm nếu có đủ area và line
                var exist = Global.currentTrayRight.Find(e => e.tray == Global.currentTray && e.client_id == clientIdPair && e.index == dataDTO.index && e.side == dataDTO.side);

                if (exist is not null)
                {
                    await ExportDataToCsvRow(dataDTO, exist);
                }
                //calculate
                //send
            }

            //if

            if (Global._currentTray.Count == 80)
            {
                foreach (var item in Global._currentTray)
                {
                    var _clientIdPair = GetClientIdPair((DataDTO)item);
                    var _itemExist = Global._currentTray.Find(e => e.tray == Global.currentTray && e.client_id == _clientIdPair && e.index == item.index && e.side == item.side);

                    if (_itemExist is not null)
                    {
                        var _rs = GetResult(_itemExist.result, item.result);
                        var _position = GetPosition(item.index, item.client_id);
                        Global.controlPLC.WriteDataToRegister(_rs, _position);
                    }
                }

                Global.controlPLC.VisionDoneIns();
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
                             d.ResultArea != null && d.ResultArea != EMPTY &&
                             d.ResultLine != null && d.ResultLine != EMPTY)
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

        public async Task<List<Data>> GetHistory()
        {
            try
            {
                return await _dbContext.Data.AsNoTracking()
                    .OrderBy(e => e.Index)
                    .Include(p => p.Errors)
                    .Include(p => p.Images)
                    //.Take(500)
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

        public async Task ExportDataToCsvRow(DataDTO dataDTO, DataDTO exist)
        {
            var data = new DataCSV
            {
                model = "Stiffiner",
                time = dataDTO.time,
                index = dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2 ? exist.index + 20 : exist.index,
                image = dataDTO.image + "," + exist.image,
                errors = dataDTO.error + "," + exist.error
            };

            if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_3)
            {
                data.result_area = dataDTO.result == 1 ? "OK" : (dataDTO.result == 3 ? "Empty" : "NG");
                data.result_line = exist.result == 1 ? "OK" : (exist.result == 3 ? "Empty" : "NG");
            }
            else
            {
                data.result_area = exist.result == 1 ? "OK" : (exist.result == 3 ? "Empty" : "NG");
                data.result_line = dataDTO.result == 1 ? "OK" : (dataDTO.result == 3 ? "Empty" : "NG");
            }

            string directoryPath = Global.directoryPath;
            string fileName = Global.fileNameCSV;
            string filePath = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                bool fileExists = File.Exists(filePath);
                using (var writer = fileExists ? File.AppendText(filePath) : new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    if (!fileExists)
                    {
                        csv.WriteHeader<DataCSV>();
                        await csv.NextRecordAsync();
                    }
                    csv.WriteRecord(data);
                    await csv.NextRecordAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Can not save to file CSV: " + ex.Message);
            }
        }

        public async Task<List<ImageResponse>> DownloadFile(List<Image> images)
        {
            try
            {
                List<ImageResponse> imgsResponse = new List<ImageResponse>();

                string rootPath = @"D:\publish_image\images\";

                using (var client = new WebClient())
                {

                    foreach (var item in images)
                    {
                        client.Credentials = new NetworkCredential("MS", "1");

                        string filePathRemote = GetFilePathRemote(item.ClientId) + FormatUrlImage(item.Path);

                        string fileName = DateTime.Now.ToString("HH_mm_ss_ff") + ".bmp";

                        //string filePath = "";

                        client.DownloadFile(filePathRemote, rootPath + fileName);

                        imgsResponse.Add(new ImageResponse
                        {
                            client_id = item.ClientId,
                            path = "https://192.168.1.55:8089/images/" + fileName
                        });
                    }

                    return imgsResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cannot download file: " + ex.ToString());
                return null;
            }
        }

        public string GetFilePathRemote(int clientId)
        {
            switch (clientId)
            {
                case 1:
                    return "file://192.168.1.11/ScreenCapture/";
                case 2:
                    return "file://192.168.1.22/ScreenCapture/";
                case 3:
                    return "file://192.168.1.33/ScreenCapture/";
                case 4:
                    return "file://192.168.1.44/ScreenCapture/";
            }

            return "";
        }

        public string FormatUrlImage(string? urlImage)
        {
            string filePath = @urlImage;

            int startIndex = @"D:\SaveResults\ScreenCapture\".Length;

            string subPath = filePath.Substring(startIndex);

            return subPath.Replace('\\', '/');
        }
    }
}
