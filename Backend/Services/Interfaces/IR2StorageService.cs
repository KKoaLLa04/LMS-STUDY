namespace Backend.Services.Interfaces;

public interface IR2StorageService
{
    /// <summary>
    /// Sinh presigned PUT URL để browser upload file thẳng lên R2 (không qua backend).
    /// Trả về URL upload và key (đường dẫn object) sẽ được lưu vào DB.
    /// </summary>
    Task<(string UploadUrl, string Key)> GeneratePresignedUploadUrlAsync(string fileName, string contentType);

    /// <summary>
    /// Sinh presigned GET URL tạm thời để phát video từ bucket private.
    /// </summary>
    Task<string> GeneratePresignedPlaybackUrlAsync(string key);

    /// <summary>
    /// Xóa object khỏi bucket (dùng khi video bị thay thế/xóa để tránh chiếm hạn mức free).
    /// </summary>
    Task DeleteAsync(string key);
}
