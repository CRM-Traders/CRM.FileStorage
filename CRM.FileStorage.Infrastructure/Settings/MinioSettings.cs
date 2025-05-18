namespace CRM.FileStorage.Infrastructure.Settings;

public class MinioSettings
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public bool UseSSL { get; set; } = false;
    public string TempBucketName { get; set; } = "temp-files";
    public string PermanentBucketName { get; set; } = "permanent-files";
    public bool CreateBucketsIfNotExist { get; set; } = true; 
}