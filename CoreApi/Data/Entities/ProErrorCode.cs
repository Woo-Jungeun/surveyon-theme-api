using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreApi.Data.Entities;

[Table("pro_error_code")]
public class ProErrorCode
{
    [Key]
    [Column("errorcode")]
    public string ErrorCode { get; set; } = string.Empty;

    [Column("errortext")]
    public string? ErrorText { get; set; }
}
