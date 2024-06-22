using Stiffiner_Inspection.Commons;

namespace Stiffiner_Inspection.Jobs
{
    public class AutoDeleteOldFile : BackgroundService
    {
        public AutoDeleteOldFile() {}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);

                DeleteExcel(Global.PATH_SAVE_EXCEL, DateTime.Now.Subtract(TimeSpan.FromDays(Global.AUTO_DELETE_EXCEL)));
                DeleteImageDownload(Global.PATH_SAVE_IMAGE, DateTime.Now.Subtract(TimeSpan.FromDays(Global.AUTO_DELETE_IMAGE)));
            }
        }

        private void DeleteExcel(string basePath, DateTime thresholdDate)
        {
            if (!Directory.Exists(basePath))
            {
                Log.Error("Not exists path excel folder to delete!");
                return;
            }

            int batchSize = 1000;

            var fileBatch = Directory.EnumerateFiles(basePath).Take(batchSize);

            while (fileBatch.Any())
            {
                foreach (var file in fileBatch)
                {
                    var fileInfo = new FileInfo(file);

                    if (fileInfo.CreationTime < thresholdDate)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error deleting file {file}: {ex.Message}");
                        }
                    }
                }
                fileBatch = Directory.EnumerateFiles(basePath).Skip(batchSize).Take(batchSize);
            }

            var directories = Directory.GetDirectories(basePath);

            foreach (var directory in directories)
            {
                DeleteExcel(directory, thresholdDate);
            }
        }

        private void DeleteImageDownload(string basePath, DateTime thresholdDate)
        {
            if (!Directory.Exists(basePath))
            {
                Log.Error("Not exists path image download folder to delete!");
                return;
            }

            int batchSize = 1000;

            var fileBatch = Directory.EnumerateFiles(basePath).Take(batchSize);

            while (fileBatch.Any())
            {
                foreach (var file in fileBatch)
                {
                    ExecuteDelete(file, thresholdDate);
                }

                fileBatch = Directory.EnumerateFiles(basePath).Skip(batchSize).Take(batchSize);
            }

            var directories = Directory.GetDirectories(basePath);

            foreach (var directory in directories)
            {
                DeleteImageDownload(directory, thresholdDate);
            }
        }

        private void ExecuteDelete(string file, DateTime thresholdDate)
        {
            try
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.CreationTime < thresholdDate)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error deleting file {file}: {ex.Message}");
            }

        }
    }
}
