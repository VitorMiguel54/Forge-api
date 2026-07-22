namespace Forge.Infrastructure.Storage;

public class AdminImageStorageOptions
{
    public string RootPath { get; set; } = string.Empty;
    public string PublicBasePath { get; set; } = "/uploads/backoffice";
}
