namespace Stiffiner_Inspection.Models.DTO.Data
{
    public class DataDTO
    {
        public int id { get; set; }
        public DateTime? time { get; set; }
        public string? model { get; set; } = string.Empty;
        public int tray { get; set; }
        public int? client_id { get; set; }
        public string? side { get; set; } = string.Empty;
        public int index { get; set; }
        public string? camera { get; set; } = string.Empty;
        public int result { get; set; }
        public string? error { get; set; }
        public string? image { get; set; } = string.Empty;
    }
}