using log4net;
using Microsoft.EntityFrameworkCore;
using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.Entity;

namespace Stiffiner_Inspection.Services
{
    public class StatusCAMService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILog _logger = LogManager.GetLogger(typeof(StatusCAMService));

        public StatusCAMService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateStatusCAM(StatusCAM entity)
        {
            try
            {
                _dbContext.StatusCAM.Update(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Update status CAM failed: " + ex.Message);
            }
        }

        public async Task<List<StatusCAM>> GetAll()
        {
            return await _dbContext.StatusCAM.ToListAsync();
        }
    }
}
