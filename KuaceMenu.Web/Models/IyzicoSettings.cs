namespace KuaceMenu.Web.Models;

public class IyzicoSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string CallbackUrl { get; set; } = string.Empty;
    public decimal AnnualPrice { get; set; } = 1499m;
}
