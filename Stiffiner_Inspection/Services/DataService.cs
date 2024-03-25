using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Ajax.Utilities;

namespace Stiffiner_Inspection.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;

        const int CLIENT_1 = 1;
        const int CLIENT_2 = 2;
        const int CLIENT_3 = 3;
        const int CLIENT_4 = 4;

        const int OK = 1;
        const int NG = 2;
        const int EMPTY = 3;

        public DataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Data> Save(DataDTO dataDTO)
        {
            //tìm đối của client
            var pairClientId = GetClientIdPair(dataDTO);

            //check exist
            var existEntity = await _dbContext.Data.Where(e => e.TargetId == Global.currentTargetId && e.Tray == Global.currentTray && e.ClientId == pairClientId).FirstOrDefaultAsync();

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

                var position = GetPosition(dataDTO.index, dataDTO.client_id);
                var rs = GetResult(existEntity.ResultArea, existEntity.ResultLine);
                Global.controlPLC.WriteDataToRegister(rs, position);

                await _dbContext.SaveChangesAsync();

                return existEntity;
            } else
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
                data.Index = dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2 ? dataDTO.index + 20 : dataDTO.index;

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

        public async Task<long> GetCurrentTargetID()
        {
            long maxTargetId = _dbContext.Targets.Max(t => t.TargetId);

            return maxTargetId;
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
            int totalCount = _dbContext.Data
            .Where(d => d.TargetId == targetId &&
                        d.ResultArea != null && d.ResultArea != 3 &&
                        d.ResultLine != null && d.ResultLine != 3)
            .GroupBy(d => d.TargetId)
            .Select(g => g.Count())
            .FirstOrDefault();

            return totalCount;
        }

        public async Task<int> GetTotalTray(long currtarget)
        {
            try
            {
                var trays = await _dbContext.Data
              .Where(d => d.TargetId == currtarget)
              .Select(d => d.Tray)
              .Distinct()
              .ToListAsync();

                int totalTray = trays.Count;
                return totalTray;
            }
            catch (Exception ex)
            { return 0; }
        }

        public async Task<int> GetTotalEmpty(long currtarget)
        {
            var totalEmpty = _dbContext.Data
           .Where(d => d.TargetId == currtarget &&
            d.ResultArea != null &&
            d.ResultLine != null && (d.ResultArea == 3 || d.ResultLine == 3))
           .GroupBy(d => d.TargetId)
           .Select(g => g.Count())
           .FirstOrDefault();

            return totalEmpty;

        }

        public async Task<int> GettotalOK(long currtarget)
        {

            var totalOK = _dbContext.Data.Where(d => d.TargetId == currtarget && d.ResultArea == 1 && d.ResultLine == 1 && d.ResultArea != null && d.ResultLine != null)
                .GroupBy(d => d.TargetId).Select(g => g.Count()).FirstOrDefault();
            return totalOK;
        }

        public async Task<int> GettotalNG(long currtarget)
        {
            var totalNG =  _dbContext.Data
            .Where(d => d.TargetId == currtarget && d.ResultArea != null && d.ResultLine != null &&
                        ((d.ResultArea == 2 && d.ResultLine != 3) || (d.ResultLine == 2 && d.ResultArea != 3)))
            .GroupBy(d => d.TargetId)
            .Select(g => g.Count())
            .FirstOrDefault();

            return totalNG;
        }

        public async Task<int> GetCurrentTray(long currtarget)
        {
            return 0;
        }

        public async Task<List<Data>> GetHistory()
        {
            string sqlQuery = "SELECT * from data";

            return _dbContext.Data.FromSqlRaw(sqlQuery).ToList();
        }
    }
}
