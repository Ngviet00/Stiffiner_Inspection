using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("errors")]
    public class Error
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("data_id", TypeName = "bigint")]
        public long DataId { get; set; }

        [Column("description"), MaxLength(255)]
        public string? Description { get; set; } = string.Empty;

        [Column("type")]
        public int Type { get; set; }

        [JsonIgnore]
        public Data? Data { get; set; }
    }
}
