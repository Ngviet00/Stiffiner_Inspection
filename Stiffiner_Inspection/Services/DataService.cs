using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Stiffiner_Inspection.Models.Response;
using System.Drawing;

namespace Stiffiner_Inspection.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;

        public DataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Data> Save(DataDTO dataDTO)
        {
            var data = new Data
            {
                Id = dataDTO.id,
                Result = dataDTO.result,
                ErrorCode = dataDTO.error_code,
                Time = dataDTO.time,
                ClientId = dataDTO.id,
                Model = dataDTO.model,
                Tray = dataDTO.tray,
                Side = dataDTO.side,
                No = dataDTO.no,
                Camera = dataDTO.camera,
                ErrorDetection = dataDTO.error_detection,
            };

            await _dbContext.Data.AddAsync(data);
            await _dbContext.SaveChangesAsync();

            return data;
        }
    }
}
