using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("images")]
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("data_id", TypeName = "bigint")]
        public long DataId { get; set; }

        [Column("path"), MaxLength(255)]
        public string? Path { get; set; } = string.Empty;

        public Data? Data { get; set; }
    }
}
