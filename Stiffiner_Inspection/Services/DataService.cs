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
                Camera = dataDTO.camera,
                Result = dataDTO.result,
                ErrorCode = dataDTO.error_code,
            };

            await _dbContext.Data.AddAsync(data);
            await _dbContext.SaveChangesAsync();

            return data;
        }

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

        public async void GetMaxTray()
        {

        }

        public async void GetTotal()
        {

        }

        public async void GetTotalOk()
        {

        }

        public async void GetEmpty()
        {

        }
    }
}
