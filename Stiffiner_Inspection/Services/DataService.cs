using Microsoft.EntityFrameworkCore;
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

        public async Task<Data?> Save(DataDTO dataDTO) //1
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

            //var clientIdPair = GetClientIdPair(dataDTO); //2

            ////find 2
            //var entity = _dbContext.Data.FirstOrDefaultAsync(e => e.Tray == dataDTO.tray && e.Index == dataDTO.index && e.ClientId == clientIdPair);

            //if (entity is not null)
            //{
            //    //update
            //    //send to PLC


            //    // Update other properties as needed
            //    // Optionally, mark entity as modified if needed
            //    _dbContext.Entry(entity).State = EntityState.Modified;

            //    Global.controlPLC.WriteDataToRegister(1, 1);

            //    return entity;
            //}
            //else
            //{
            //    //save to db
            //    var data = new Data
            //    {
            //        Id = dataDTO.id,
            //        Time = dataDTO.time,
            //        Model = dataDTO.model,
            //        Tray = dataDTO.tray,
            //        ClientId = dataDTO.client_id,
            //        Side = dataDTO.side,
            //        Index = dataDTO.index,
            //        Result = dataDTO.result,
            //        Camera = dataDTO.camera,
            //        ErrorCode = dataDTO.error_code,
            //    };

            //    if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_3)
            //    {
            //        data.Result = dataDTO.result;
            //    } else
            //    {
            //        data.Result1 = dataDTO.result;
            //    }

            //    await _dbContext.Data.AddAsync(data);

            //    return data;
            //}

            //await _dbContext.SaveChangesAsync();

            //return null;

            ////return dataDTO;
        }

        //public async Task<Data?> SaveV2(DataDTO dataDTO) //1
        //{
        //    var clientIdPair = GetClientIdPair(dataDTO); //2

        //    //find 2
        //    var entity = _dbContext.Data.FirstOrDefaultAsync(e => e.Tray == dataDTO.tray && e.Index == dataDTO.index && e.ClientId == clientIdPair);

        //    if (entity is not null)
        //    {
        //        //update
        //        //send to PLC


        //        // Update other properties as needed
        //        // Optionally, mark entity as modified if needed
        //        _dbContext.Entry(entity).State = EntityState.Modified;

        //        Global.controlPLC.WriteDataToRegister(1, 1);

        //        return entity;
        //    }
        //    else
        //    {
        //        //save to db
        //        var data = new Data
        //        {
        //            Id = dataDTO.id,
        //            Time = dataDTO.time,
        //            Model = dataDTO.model,
        //            Tray = dataDTO.tray,
        //            ClientId = dataDTO.client_id,
        //            Side = dataDTO.side,
        //            Index = dataDTO.index,
        //            Camera = dataDTO.camera,
        //            ErrorCode = dataDTO.error_code,
        //        };

        //        if (dataDTO.client_id == CLIENT_1 || dataDTO.client_id == CLIENT_3)
        //        {
        //            data.Result = dataDTO.result;
        //        }
        //        else
        //        {
        //            data.Result1 = dataDTO.result;
        //        }

        //        await _dbContext.Data.AddAsync(data);

        //        return data;
        //    }

        //    await _dbContext.SaveChangesAsync();

        //    return null;

        //    //return dataDTO;
        //}

        public async Task<Data> FindPair(Data? data)
        {
            int clientId = GetClientIdFindPair(data);

            return await _dbContext.Data.Where(e => e.Tray == data.Tray && e.ClientId == clientId && e.Index == data.Index).FirstOrDefaultAsync();
        }

        public int GetPosition(int? index, int? clientId)
        {
            if (clientId == CLIENT_3 || clientId == CLIENT_4)
            {
                return (int)index - 1;
            }

            return (int)index + 19;
        }

        public int GetResult(int? result1, int? result2)
        {
            if (result1 == OK && result2 == OK)
            {
                return OK;
            }

            if (result1 == EMPTY || result2 == EMPTY)
            {
                return EMPTY;
            }

            if (result1 == NG || result2 == NG)
            {
                return NG;
            }

            return 0;
        }

        public int GetClientIdFindPair(Data? data)
        {
            return data?.ClientId == CLIENT_1 || data?.ClientId == CLIENT_2
                ? (data.ClientId == CLIENT_1 ? CLIENT_2 : CLIENT_1)
                : (data?.ClientId == CLIENT_3 || data?.ClientId == CLIENT_4 ? (data.ClientId == CLIENT_3 ? CLIENT_4 : CLIENT_3) : 0);
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

        public async Task<int> CountTotal()
        {
            return await _dbContext.Data.CountAsync();
        }

        public async Task<int> CountOK()
        {
            return await _dbContext.Data.Where(e => e.Result1 == OK && e.Result == OK).CountAsync();
        }

        public async Task<int> CountNG()
        {
            return await _dbContext.Data.Where(e => e.Result1 == NG || e.Result == NG).CountAsync();
        }

        public async Task<int> CountEmpty()
        {
            return await _dbContext.Data.Where(e => e.Result1 == EMPTY || e.Result == EMPTY).CountAsync();
        }

        public async Task<String> GetMaxTray()
        {
            return "";
        }
    }
}
