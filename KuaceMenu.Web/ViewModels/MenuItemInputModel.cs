using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KuaceMenu.Web.ViewModels;

public class MenuItemInputModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }

    public int MenuCategoryId { get; set; }

    public IFormFile? Photo { get; set; }

    public bool IsActive { get; set; } = true;
}
