using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("data")]
    public class Data
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("time")]
        public DateTime? Time { get; set; }

        [Column("model"), MaxLength(255)]
        public string? Model { get; set; } = string.Empty;

        [Column("tray")]
        public int? Tray { get; set; }

        [Column("client_id")]
        public int? ClientId { get; set; }

        [Column("side"), MaxLength(255)]
        public string? Side { get; set; } = string.Empty;

        [Column("index")]
        public int? Index { get; set; }

        [Column("camera"), MaxLength(255)]
        public string? Camera { get; set; } = string.Empty;

        [Column("result")]
        public int? Result { get; set; }

        [Column("error_code")]
        public int? ErrorCode { get; set; }

        [Column("image"), MaxLength(255)]
        public string? Image { get; set; } = string.Empty;

        [Column("time_start")]
        public DateTime? TimeStart { get; set; }

        [Column("time_end")]
        public DateTime? TimeEnd { get; set; }
    }
}
