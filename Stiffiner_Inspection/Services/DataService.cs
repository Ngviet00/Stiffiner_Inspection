using Stiffiner_Inspection.Contexts;
using Stiffiner_Inspection.Models.DTO.Data;
using Stiffiner_Inspection.Models.Entity;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        string connectionString = "Data Source=192.168.0.119;Initial Catalog=DBTest;User ID=DBTest;Password=DBTest;Integrated Security=False;Trust Server Certificate=True";

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
            }
            else
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

        public async Task<int> GetCurrentTargetID()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                int currentTargetID = 0;
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select DISTINCT Target_id  from Target t where current_Qty <= Target_id ";
                    var currTarget = await command.ExecuteScalarAsync();
                    if (currTarget != null)
                    {
                        currentTargetID = Convert.ToInt32(currTarget);
                    }
                    return currentTargetID;
                }
            }

        }
        public async Task<int> GetCurrentTargetQty()
        {
            int Targetqty = 0;
            var currTarget = await GetCurrentTargetID();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select Target_Qty  from Target t where t.target_id =@targetid";
                    command.Parameters.AddWithValue("@targetid", currTarget);
                    var currTargetQty = await command.ExecuteScalarAsync();
                    if (currTargetQty != null)
                    {
                        Targetqty = Convert.ToInt32(currTargetQty);
                    }
                    return Targetqty;
                }
            }

        }
        

        public async Task<int> GetTotal()
        {
            try
            {
                int Total = 0;
                var currTarget = await GetCurrentTargetID();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "with a as (( select a.target_id, a.[index], a.result areacam, b.result linecam from data a, data b where a.client_id = 1 and b.client_id = 2 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id) union all ( select a.target_id, a.[index], a.result areacam, b.result linecam from data a, data b where a.client_id = 3 and b.client_id = 4 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id ) )select count(*) Total from a where a.target_id =@targetid and areacam<>3 and  linecam<> 3 group by a.target_id";
                        command.Parameters.AddWithValue("@targetid", currTarget);
                        var all = await command.ExecuteScalarAsync();
                        if (all == null)
                        {
                            Total = 0;
                        }
                        else
                        {
                            Total = int.Parse(all.ToString());
                        }
                    }
                }
                return Total;


            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        public async Task<int> GetTotalTray()
        {
            try
            {
                int totalTray = 0;
                var currTarget = await GetCurrentTargetID();
                List<string> lstTray = new List<string>();
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select DISTINCT tray from data where Target_id =@targetid";
                        cmd.Parameters.AddWithValue("@targetid", currTarget);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                lstTray.Add(reader.GetString(0));
                            }
                        }
                        if (lstTray != null && lstTray.Any())
                        {
                            totalTray = lstTray.Count;
                        }
                        else
                        {
                            totalTray = 0;
                        }
                    }
                }
                return totalTray;
            }
            catch (Exception ex)
            { return 0; }
        }

        public async Task<int> GetTotalEmpty()
        {
            int TotalEmpty = 0;
            var currTarget = await GetCurrentTargetID();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "with a as (( select a.target_id, a.tray, a.[index], 'left' side, a.result areacam, b.result linecam from data a, data b where a.client_id = 1 and b.client_id = 2 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id) union all ( select a.target_id, a.tray, a.[index], 'right' side, a.result areacam, b.result linecam from data a, data b where a.client_id = 3 and b.client_id = 4 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id ) )select count(*) TotalEmpty from a where a.target_id =@targetid and (areacam =3 or linecam =3) group by a.target_id";
                    cmd.Parameters.AddWithValue("@targetid", currTarget);
                    var allempty = cmd.ExecuteScalar();
                    if (allempty == null)
                    {
                        TotalEmpty = 0;
                    }
                    else
                    {
                        TotalEmpty = int.Parse(allempty.ToString());
                    }
                }
            }
            return TotalEmpty;

        }

        public async Task<int> GettotalOK()
        {
            int totalOK = 0;
            var currTarget = await GetCurrentTargetID();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "with a as (( select a.target_id, a.tray, a.[index], 'left' side, a.result areacam, b.result linecam from data a, data b where a.client_id = 1 and b.client_id = 2 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id) union all ( select a.target_id, a.tray, a.[index], 'right' side, a.result areacam, b.result linecam from data a, data b where a.client_id = 3 and b.client_id = 4 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id ) )select count (*) TotalOK from a where a.target_id =@targetid and a.areacam =a.linecam and a.areacam =1 group by a.target_id";
                    cmd.Parameters.AddWithValue("@targetid", currTarget);
                    var allok =  cmd.ExecuteScalar();
                    if (allok == null)
                    {
                        totalOK = 0;
                    }
                    else
                    {
                        totalOK = int.Parse(allok.ToString());
                    }

                }
            }

            return totalOK;
        }

        public async Task<int> GettotalNG()
        {
            int totalNG = 0;
            var currTarget = await GetCurrentTargetID();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "with a as (( select a.target_id, a.tray, a.[index], 'left' side, a.result areacam, b.result linecam from data a, data b where a.client_id = 1 and b.client_id = 2 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id) union all ( select a.target_id, a.tray, a.[index], 'right' side, a.result areacam, b.result linecam from data a, data b where a.client_id = 3 and b.client_id = 4 and a.[index] = b.[index] and a.tray = b.tray and a.side = b.side and a.target_id =b.target_id ) )select count(*) totalNG from a where a.target_id =@targetid AND ((areacam = 2 AND areacam <> 3 AND linecam <> 3) or (linecam = 2 AND linecam <> 3 AND areacam <> 3))";
                    cmd.Parameters.AddWithValue("@targetid", currTarget);
                    var allng = cmd.ExecuteScalar();
                    if (allng == null)
                    {
                        totalNG = 0;
                    }
                    else
                    {
                        totalNG = int.Parse(allng.ToString());
                    }

                }
            }

            return totalNG;
        }

    }
}
