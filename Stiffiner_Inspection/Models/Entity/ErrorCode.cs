using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stiffiner_Inspection.Models.Entity
{
    [Table("error_code")]
    public class ErrorCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("error_content"), MaxLength(255)]
        public string? ErrorContent { get; set; } = string.Empty;

        
    }
}
