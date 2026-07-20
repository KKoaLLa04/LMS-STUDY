using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class KhoiHoc
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public int OrderNumber { get; set; }
}
