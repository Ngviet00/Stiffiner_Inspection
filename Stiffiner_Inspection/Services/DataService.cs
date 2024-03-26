using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using System.Globalization;
using CsvHelper;
using log4net;
using Stiffiner_Inspection.Controllers;

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
                    await SaveImage(existEntity, dataDTO.image);

                    //save error
                    await SaveError(existEntity, dataDTO.error);
                }

                await _dbContext.SaveChangesAsync();

                //save excel

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
                    await SaveImage(data, dataDTO.image);

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

        private async Task SaveImage(Data data, string listImgs)
        {
            List<Image> listImages = new List<Image>();

            string[] imgs = listImgs.Split(',');

            foreach (string item in imgs)
            {
                listImages.Add(new Image
                {
                    DataId = data.Id,
                    Path = item,
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
            var clientIdPair = GetClientIdPair(dataDTO);

            if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2)
            {
                Global.currentTrayLeft.Add(dataDTO);

                var exist = Global.currentTrayLeft.Find(e => e.tray == Global.currentTray && e.client_id == clientIdPair && e.index == dataDTO.index);

                if (exist is not null)
                {
                    var rs = GetResult(dataDTO.result, exist.result);
                    var position = GetPosition(exist.index, dataDTO.client_id);
                    Global.controlPLC.WriteDataToRegister(rs, position);

                    await SaveToCSV(dataDTO, exist);
                }
            }
            else
            {
                Global.currentTrayRight.Add(dataDTO);

                var exist = Global.currentTrayRight.Find(e => e.tray == Global.currentTray && e.client_id == clientIdPair && e.index == dataDTO.index);

                if (exist is not null)
                {
                    var rs = GetResult(dataDTO.result, exist.result);
                    var position = GetPosition(exist.index, dataDTO.client_id);
                    Global.controlPLC.WriteDataToRegister(rs, position);

                    await SaveToCSV(dataDTO, exist);
                }
            }
        }

        public async Task<long> GetCurrentTargetID()
        {
            return await _dbContext.Targets
                .OrderByDescending(t => t.TargetId)
                .Select(t => t.TargetId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCurrentTargetQty(long currTargetid)
        {
            int targetQty = 0;

            var target = await _dbContext.Targets
                .Where(t => t.TargetId == currTargetid)
                .Select(t => t.Target_qty)
                .FirstOrDefaultAsync();

            if (target != null)
            {
                targetQty = target;
            }

            return targetQty;

        }

        public async Task<int> GetTotal(long targetId)
        {
            return await _dbContext.Data
            .Where(d => d.TargetId == targetId &&
                        d.ResultArea != null && d.ResultArea != EMPTY &&
                        d.ResultLine != null && d.ResultLine != EMPTY)
            .GroupBy(d => d.TargetId)
            .Select(g => g.Count())
            .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalTray(long currtarget)
        {
            return await _dbContext.Data.Where(d => d.TargetId == currtarget).Select(d => d.Tray).Distinct().CountAsync();
        }

        public async Task<int> GetTotalEmpty(long currtarget)
        {
            return await _dbContext.Data
               .Where(d => d.TargetId == currtarget &&
                d.ResultArea != null &&
                d.ResultLine != null && (d.ResultArea == EMPTY || d.ResultLine == EMPTY))
               .GroupBy(d => d.TargetId)
               .Select(g => g.Count())
               .FirstOrDefaultAsync();
        }

        public async Task<int> GettotalOK(long currtarget)
        {
            return await _dbContext.Data
                .Where(d => d.TargetId == currtarget && d.ResultArea == OK && d.ResultLine == OK && d.ResultArea != null && d.ResultLine != null)
                .GroupBy(d => d.TargetId)
                .Select(g => g.Count())
                .FirstOrDefaultAsync();
        }

        public async Task<int> GettotalNG(long currtarget)
        {
            return await _dbContext.Data
                .Where(d => d.TargetId == currtarget && d.ResultArea != null && d.ResultLine != null &&
                            ((d.ResultArea == NG && d.ResultLine != EMPTY) || (d.ResultLine == NG && d.ResultArea != EMPTY)))
                .GroupBy(d => d.TargetId)
                .Select(g => g.Count())
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCurrentTray(long currtarget)
        {
            return 0;
        }

        public async Task<List<Data>> GetHistory()
        {
            string sqlQuery = "SELECT * from data";

            return await _dbContext.Data.FromSqlRaw(sqlQuery).ToListAsync();
        }

        public double CalculateChartOK(int totalOK, double total, int totalEmpty)
        {
            return total == 0 ? 0 : Math.Round(totalOK / (total + totalEmpty) * PERCENT, 1);
        }

        public double CalculateChartNG(int totalNG, double total, int totalEmpty)
        {
            return total == 0 ? 0 : Math.Round(totalNG / (total + totalEmpty) * PERCENT, 1);
        }

        public double CalculateChartEmpty(double total, double percentNG, double percentOK)
        {
            return total == 0 ? 0 : Math.Round(100 - percentNG - percentOK, 1);
        }

        //public async Task<bool> ExportDataToCsv(long targetId, int tray)
        //{
        //    string directoryPath = @"D:\Export";
        //    string fileName = $"test_{targetId}_{tray}.csv";
        //    string filePath = Path.Combine(directoryPath, fileName);

        //    if (!Directory.Exists(directoryPath))
        //    {
        //        Directory.CreateDirectory(directoryPath);
        //    }
        //    //sql
        //    var query = _dbContext.Data
        //        .Where(t => t.TargetId == targetId && t.Tray == tray);

        //    try
        //    {
        //        using (var writer = new StreamWriter(filePath))
        //        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        //        {
        //            await csv.WriteRecordsAsync(query);
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        public async Task SaveToCSV(DataDTO dataDTO, DataDTO exist)
        {

            //var newDataCSV = new List<DataCSV> {
            //    new DataCSV {
            //        model = "Stiffiner",
            //        time = dataDTO.time,
            //        index = dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2 ?  exist.index + 20 : exist.index,
            //        image = dataDTO.image + "," + exist.image,
            //        errors = dataDTO.error + "," + exist.error
            //    }
            //};

            var newDataCSV = new DataCSV
            {
                model = "Stiffiner",
                time = dataDTO.time,
                index = dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2 ? exist.index + 20 : exist.index,
                image = dataDTO.image + "," + exist.image,
                errors = dataDTO.error + "," + exist.error
            };

            if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_3)
            {
                newDataCSV.result_area = dataDTO.result == 1 ? "OK" : (dataDTO.result == 3 ? "Empty" : "NG");
                newDataCSV.result_line = exist.result == 1 ? "OK" : (exist.result == 3 ? "Empty" : "NG");
            }
            else
            {
                newDataCSV.result_area = exist.result == 1 ? "OK" : (exist.result == 3 ? "Empty" : "NG");
                newDataCSV.result_line = dataDTO.result == 1 ? "OK" : (dataDTO.result == 3 ? "Empty" : "NG");
            }

            string directoryPath = Global.directoryPath;
            string fileName = Global.fileNameCSV;
            string filePath = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            bool appendHeader = !File.Exists(filePath) || new FileInfo(filePath).Length == 0;

            try
            {
                using (var writer = new StreamWriter(filePath, true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    if (appendHeader)
                    {
                        csv.WriteHeader<DataCSV>();
                        csv.NextRecord();
                    }
                    csv.WriteRecord(newDataCSV);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Can not save to file CSV:" + ex.Message);
            }
        }
    }
}
