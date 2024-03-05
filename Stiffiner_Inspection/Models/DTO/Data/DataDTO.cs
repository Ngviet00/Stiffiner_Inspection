namespace Stiffiner_Inspection.Models.DTO.Data
{
    public class DataDTO
    {
        public int id { get; set; }
        public string? result { get; set; } = string.Empty;
        public string? error_code { get; set; } = string.Empty;
        public DateTime? time { get; set; }
        public int? client_id { get; set; }
        public string? model { get; set; } = string.Empty;
        public int? tray { get; set; }
        public string? side { get; set; } = string.Empty;
        public int? no { get; set; }
        public string? camera { get; set; } = string.Empty;
        public string? error_detection { get; set; } = string.Empty;
    }
}