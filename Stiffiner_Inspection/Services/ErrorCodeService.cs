using Microsoft.EntityFrameworkCore;
using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.Entity;

namespace Stiffiner_Inspection.Services
{
    public class ErrorCodeService
    {
        private readonly ApplicationDbContext _dbContext;

        public ErrorCodeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ErrorCode>> GetAll()
        {
            return await _dbContext.ErrorCodes.ToListAsync();
        }
    }
}
