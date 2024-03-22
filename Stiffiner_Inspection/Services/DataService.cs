using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;

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
            var data = new Data
            {
                Id = dataDTO.id,
                Time = dataDTO.time,
                Model = dataDTO.model,
                Tray = dataDTO.tray,
                ClientId = dataDTO.client_id,
                Side = dataDTO.side,
                Index = dataDTO.index,
                Result = dataDTO.result,
                Camera = dataDTO.camera,
                ErrorCode = dataDTO.error_code,
            };

            await _dbContext.Data.AddAsync(data);
            await _dbContext.SaveChangesAsync();

            return data;
        }

        public void SendToPLC(DataDTO dataDTO)
        {
            //client 1, 2 thêm vào tray left, còn lại tray right
            if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2)
            {
                Global.TrayLeft.Add(dataDTO);
            } else
            {
                Global.TrayRight.Add(dataDTO);
            }

            //tìm cặp của client id, ex: dataDto.client_id == 1 => 2, 3 => 4 và ngược lại
            var clientIdPair = GetClientIdPair(dataDTO);

            if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_2)
            {
                var existItem = Global.TrayLeft.Find(obj => obj.tray == dataDTO.tray && obj.index == dataDTO.index && obj.client_id == clientIdPair);
                if (existItem is not null)
                {
                    //nếu có cặp => lấy kết quả, vị trí và gửi cho PLC
                    var rs = GetResult(existItem.result, dataDTO.result);
                    var position = GetPosition(existItem.index, existItem?.client_id);
                    Global.controlPLC.WriteDataToRegister(position, rs);
                    //add execl
                }
            }
            else
            {
                var existItem = Global.TrayRight.Find(obj => obj.tray == dataDTO.tray && obj.index == dataDTO.index && obj.client_id == clientIdPair);
                if (existItem is not null)
                {
                    var rs = GetResult(existItem.result, dataDTO.result);
                    var position = GetPosition(existItem.index, existItem?.client_id);
                    Global.controlPLC.WriteDataToRegister(position, rs);
                }
            }

            //nếu như client inspection xong, set PLC trạng thái done
            if (Global.TrayLeft.Count == 40 && Global.TrayRight.Count == 40)
            {
                Global.controlPLC.VisionBusy(true);
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
    }
}
