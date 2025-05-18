using System.Security.Cryptography;
using CRM.FileStorage.Application.Interfaces.Services;
using CRM.FileStorage.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace CRM.FileStorage.Infrastructure.Services;

public class MinioStorageService : IFileStorageService
{
    private readonly MinioSettings _minioSettings;
    private readonly ILogger<MinioStorageService> _logger;
    private readonly IMinioClient _minioClient;
 
    private readonly Dictionary<string, string> _bucketMapping = new();

    public MinioStorageService(
        IOptions<MinioSettings> minioSettings,
        ILogger<MinioStorageService> logger)
    {
        _minioSettings = minioSettings.Value;
        _logger = logger;

        // Initialize MinIO client
        _minioClient = new MinioClient()
            .WithEndpoint(_minioSettings.Endpoint)
            .WithCredentials(_minioSettings.AccessKey, _minioSettings.SecretKey)
            .WithSSL(_minioSettings.UseSSL)
            .Build();

        // Ensure buckets exist
        EnsureBucketsExistAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureBucketsExistAsync()
    {
        if (!_minioSettings.CreateBucketsIfNotExist)
            return;

        try
        {
            // Check and create temp bucket if needed
            bool tempBucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_minioSettings.TempBucketName));
                
            if (!tempBucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_minioSettings.TempBucketName));
                _logger.LogInformation("Created temporary bucket: {BucketName}", _minioSettings.TempBucketName);
            }

            // Check and create permanent bucket if needed
            bool permBucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_minioSettings.PermanentBucketName));
                
            if (!permBucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_minioSettings.PermanentBucketName));
                _logger.LogInformation("Created permanent bucket: {BucketName}", _minioSettings.PermanentBucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring MinIO buckets exist");
            throw;
        }
    }

    public async Task<string> SaveTemporaryFileAsync(Stream fileStream, string fileName, string? bucketName = null)
    {
        try
        {
            // Create a unique object ID for the file
            var objectId = Guid.NewGuid().ToString();
            
            // Store the object path without prefixing it with the bucket name
            string objectName = objectId + Path.GetExtension(fileName);
            
            long fileSize = fileStream.Length;
            _logger.LogInformation("Uploading file {FileName} with size: {Size} to bucket {Bucket}", 
                fileName, fileSize, _minioSettings.TempBucketName);
            
            // Remember the mapping between custom bucket and MinIO bucket
            var customBucket = string.IsNullOrEmpty(bucketName) ? "default" : bucketName;
            _bucketMapping[objectName] = customBucket;
            
            // Put object to MinIO
            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_minioSettings.TempBucketName)
                    .WithObject(objectName)
                    .WithObjectSize(fileSize)
                    .WithStreamData(fileStream)
                    .WithContentType(GetContentType(fileName)));
            
            _logger.LogInformation("File {FileName} saved to temporary storage as {ObjectName} in bucket {Bucket}", 
                fileName, objectName, _minioSettings.TempBucketName);
            
            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file {FileName} to temporary storage", fileName);
            throw;
        }
    }

    public async Task<string> MoveToPermanentStorageAsync(string tempPath, string fileName, string tempBucket, 
        string? permanentBucket = null)
    {
        try
        {
            // Get the object name from the temp path - it should be just the filename
            string sourceObject = tempPath;
            
            _logger.LogInformation("Moving file from tempPath={TempPath}, tempBucket={TempBucket}", 
                tempPath, tempBucket);
            
            // Create new object name for permanent storage
            string destObject = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
            
            _logger.LogInformation("Retrieving file {SourceObject} from bucket {Bucket}", 
                sourceObject, _minioSettings.TempBucketName);
            
            // Download the object from temp bucket to a byte array
            byte[] fileBytes;
            try
            {
                using var memStream = new MemoryStream();
                await _minioClient.GetObjectAsync(
                    new GetObjectArgs()
                        .WithBucket(_minioSettings.TempBucketName)
                        .WithObject(sourceObject)
                        .WithCallbackStream(stream => 
                        {
                            stream.CopyTo(memStream);
                        }));
                
                fileBytes = memStream.ToArray();
                
                _logger.LogInformation("Successfully read file from temp bucket, size: {Size} bytes", fileBytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read file {Object} from bucket {Bucket}", 
                    sourceObject, _minioSettings.TempBucketName);
                throw;
            }
            
            // Check if we actually got data
            if (fileBytes.Length == 0)
            {
                _logger.LogError("File {Object} in bucket {Bucket} is empty", 
                    sourceObject, _minioSettings.TempBucketName);
                throw new InvalidOperationException($"File {sourceObject} in bucket {_minioSettings.TempBucketName} is empty or couldn't be read");
            }
            
            // Upload to permanent bucket
            using (var uploadStream = new MemoryStream(fileBytes))
            {
                _logger.LogInformation("Uploading file to permanent bucket {Bucket} as {DestObject}", 
                    _minioSettings.PermanentBucketName, destObject);
                
                await _minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_minioSettings.PermanentBucketName)
                        .WithObject(destObject)
                        .WithObjectSize(fileBytes.Length)
                        .WithStreamData(uploadStream)
                        .WithContentType(GetContentType(fileName)));
            }
            
            _logger.LogInformation("File {FileName} moved from temporary to permanent storage as {ObjectName}", 
                fileName, destObject);
            
            return destObject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file {FileName} from temporary to permanent storage", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string filePath, string bucketName)
    {
        try
        {
            // Don't try to parse bucketName - just use the correct MinIO bucket
            string minioBucket;
            
            if (bucketName.StartsWith("kyc-temp") || bucketName.Contains("temp"))
            {
                minioBucket = _minioSettings.TempBucketName;
            }
            else
            {
                minioBucket = _minioSettings.PermanentBucketName;
            }
            
            _logger.LogInformation("Deleting file {FilePath} from bucket {Bucket}", filePath, minioBucket);
            
            // Delete object
            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(minioBucket)
                    .WithObject(filePath));
            
            _logger.LogInformation("File deleted from storage: {FilePath}", filePath);
        }
        catch (ObjectNotFoundException)
        {
            // Object doesn't exist, consider it already deleted
            _logger.LogWarning("File {FilePath} not found, already deleted", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath} from bucket {BucketName}", filePath, bucketName);
            throw;
        }
    }

    public async Task<Stream> ReadFileAsync(string filePath, string bucketName)
    {
        try
        {
            // Don't try to parse bucketName - just use the correct MinIO bucket
            string minioBucket;
            
            if (bucketName.StartsWith("kyc-temp") || bucketName.Contains("temp"))
            {
                minioBucket = _minioSettings.TempBucketName;
            }
            else
            {
                minioBucket = _minioSettings.PermanentBucketName;
            }
            
            _logger.LogInformation("Reading file {FilePath} from bucket {Bucket}", filePath, minioBucket);
            
            // To avoid issues with streaming, we'll download the entire content first
            var memoryStream = new MemoryStream();
            
            await _minioClient.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(minioBucket)
                    .WithObject(filePath)
                    .WithCallbackStream(stream => 
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                    }));
            
            _logger.LogInformation("Successfully read file {FilePath}, size: {Size} bytes", 
                filePath, memoryStream.Length);
            
            return memoryStream;
        }
        catch (ObjectNotFoundException)
        {
            _logger.LogError("File {FilePath} not found in bucket {BucketName}", filePath, bucketName);
            throw new FileNotFoundException($"File not found: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FilePath} from bucket {BucketName}", filePath, bucketName);
            throw;
        }
    }

    public async Task<string> CalculateFileHashAsync(Stream fileStream)
    {
        try
        {
            // Save the current position
            long originalPosition = fileStream.Position;
            
            // Reset stream position for full read
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(fileStream);
            
            // Restore the original position
            if (fileStream.CanSeek)
            {
                fileStream.Position = originalPosition;
            }
            
            return Convert.ToBase64String(hashBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating file hash");
            throw;
        }
    }
    
    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}