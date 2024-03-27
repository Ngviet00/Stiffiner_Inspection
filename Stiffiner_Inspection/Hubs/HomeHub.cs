using log4net;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Services;

namespace Stiffiner_Inspection.Hubs
{
    public class HomeHub : Hub
    {
        private readonly TargetService _targetService;
        private readonly ILog _logger = LogManager.GetLogger(typeof(HomeHub));

        public HomeHub(TargetService targetService)
        {
            _targetService = targetService;
        }

        public async Task AddNewTarget(int targetValue)
        {
            try
            {
                await _targetService.InsertTargetQty(Global.currentTargetId, targetValue);
            }
            catch (Exception ex)
            {
                _logger.Error("Add new target failed: " + ex);
            }
        }
        public async Task UpdateTarget(int targetValue)
        {
            try
            {
                await _targetService.UpdateTargetQty(Global.currentTargetId, targetValue);

            }
            catch (Exception ex)
            {
                _logger.Error("Update target failed: " + ex);
            }
        }
    }
}
