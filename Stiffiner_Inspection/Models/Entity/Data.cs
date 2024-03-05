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

        [Column("result"), MaxLength(255)]
        public string? Result { get; set; } = string.Empty;

        [Column("error_code"), MaxLength(255)]
        public string? ErrorCode { get; set; } = string.Empty;

        [Column("time"), MaxLength(255)]
        public DateTime? Time { get; set; }

        [Column("client_id")]
        public int? ClientId { get; set; }

        [Column("model"), MaxLength(255)]
        public string? Model { get; set; } = string.Empty;

        [Column("tray")]
        public int? Tray { get; set; }

        [Column("side"), MaxLength(255)]
        public string? Side { get; set; } = string.Empty;

        [Column("no")]
        public int? No { get; set; }

        [Column("camera"), MaxLength(255)]
        public string? Camera { get; set; } = string.Empty;

        [Column("error_detection"), MaxLength(255)]
        public string? ErrorDetection { get; set; } = string.Empty;
    }
}
