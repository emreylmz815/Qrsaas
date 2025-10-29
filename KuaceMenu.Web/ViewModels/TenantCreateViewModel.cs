using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KuaceMenu.Web.ViewModels;

public class TenantCreateViewModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [RegularExpression("[a-z0-9-]+", ErrorMessage = "Sadece küçük harf ve rakam kullanılabilir")]
    public string Slug { get; set; } = string.Empty;

    [Phone]
    public string? ContactPhone { get; set; }

    public string? Address { get; set; }

    public IFormFile? Logo { get; set; }
}
