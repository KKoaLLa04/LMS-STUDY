using Amazon.S3;
using Amazon.S3.Model;
using Backend.Configuration;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class R2StorageService : IR2StorageService
{
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".avi", ".mkv" };
    private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PlaybackUrlExpiry = TimeSpan.FromHours(6);

    private readonly IAmazonS3 _client;
    private readonly string _bucketName;

    private readonly bool _isConfigured;

    public R2StorageService(IOptions<R2Options> options)
    {
        var config = options.Value;
        _bucketName = config.BucketName;
        _isConfigured = !string.IsNullOrWhiteSpace(config.AccountId) && !string.IsNullOrWhiteSpace(config.AccessKey) &&
                        !string.IsNullOrWhiteSpace(config.SecretKey) && !string.IsNullOrWhiteSpace(config.BucketName);

        _client = new AmazonS3Client(config.AccessKey, config.SecretKey, new AmazonS3Config
        {
            ServiceURL = $"https://{config.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
        });
    }

    public async Task<(string UploadUrl, string Key)> GeneratePresignedUploadUrlAsync(string fileName, string contentType)
    {
        if (!_isConfigured)
            throw new InvalidOperationException(
                "Thiếu cấu hình R2 (AccountId/AccessKey/SecretKey/BucketName). " +
                "Kiểm tra biến môi trường R2__AccountId, R2__AccessKey, R2__SecretKey, R2__BucketName trên server.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedVideoExtensions.Contains(extension))
            throw new ArgumentException(
                $"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedVideoExtensions)}");

        var key = $"videos/{Guid.NewGuid()}{extension}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(UploadUrlExpiry),
            ContentType = contentType,
        };

        var url = await _client.GetPreSignedURLAsync(request);
        return (url, key);
    }

    public async Task<string> GeneratePresignedPlaybackUrlAsync(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(PlaybackUrlExpiry),
        };

        return await _client.GetPreSignedURLAsync(request);
    }

    public async Task DeleteAsync(string key)
    {
        await _client.DeleteObjectAsync(_bucketName, key);
    }
}
