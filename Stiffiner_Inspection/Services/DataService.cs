using Microsoft.EntityFrameworkCore;
using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using System.Text;

namespace Stiffiner_Inspection.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DataService(ApplicationDbContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
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

        public async Task<int> CountItemByResult(int result)
        {
            return await _dbContext.Data.CountAsync(e => e.Result == result);
        }

        public async Task SaveTimeLog(DateTime? time, string? type, string? message)
        {
            try
            {
                string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Files");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Folder created successfully.");
                }

                string filePath = Path.Combine(folderPath, "data.txt");

                using (StreamWriter writer = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8))
                {
                    await writer.WriteLineAsync($"{time};{type};{message}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
