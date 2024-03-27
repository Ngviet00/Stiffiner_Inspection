using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.Entity;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace Stiffiner_Inspection.Services
{
    public class TargetService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DataService _dataService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DataService));

        public TargetService(ApplicationDbContext dbContext, DataService dataService)
        {
            _dbContext = dbContext;
            _dataService = dataService;
        }

        public async Task<bool> InsertTargetQty(long currtarget, int target_qty)
        {
            try
            {
                bool chkTarget = await CheckFullTarget(currtarget);
                if (chkTarget)
                {
                    var target = new Target
                    {
                        Target_qty = target_qty,
                        Created_date = DateTime.Now,
                        Updated_date = DateTime.Now
                    };

                    _dbContext.Targets.Add(target);
                    await _dbContext.SaveChangesAsync();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Insert full target failed: " + ex);
                return false;
            }
        }

        public async Task<bool> CheckFullTarget(long currtarget)
        {
            try
            {
                int totalCount = await _dbContext.Data
                   .Where(d => d.TargetId == currtarget &&
                               d.ResultArea != null && d.ResultLine != null)
                   .GroupBy(d => d.TargetId)
                   .Select(g => g.Count())
                   .FirstOrDefaultAsync();

                int targetQty = await _dataService.GetCurrentTargetQty(currtarget);

                if (targetQty <= totalCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error Check full target error: " + ex);
                return false;
            }
        }

        public async Task<bool> UpdateTargetQty(long currtarget, int target_qty)
        {

            var targetToUpdate = await _dbContext.Targets.Where(t => t.TargetId == currtarget).FirstOrDefaultAsync();

            if (targetToUpdate != null)
            {
                targetToUpdate.Target_qty = target_qty;
                targetToUpdate.Updated_date = DateTime.Now;
                _dbContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
