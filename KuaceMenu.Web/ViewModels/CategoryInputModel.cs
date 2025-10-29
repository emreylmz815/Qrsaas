using System.ComponentModel.DataAnnotations;

namespace KuaceMenu.Web.ViewModels;

public class CategoryInputModel
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100)]
    public int DisplayOrder { get; set; }
}
