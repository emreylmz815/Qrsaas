namespace KuaceMenu.Web.Models;

public class DomainConfig
{
    public int Id { get; set; }
    public string BaseDomain { get; set; } = "kuacemenu.com";
    public bool WildcardEnabled { get; set; } = true;
}
