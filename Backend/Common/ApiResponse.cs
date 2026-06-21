using System.Text.Json.Serialization;

namespace Backend.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    [JsonIgnore]
    public int HttpStatusCode { get; set; } = 200;

    public static ApiResponse<T> Ok(T data, string message = "Thành công") =>
        new() { Success = true, Message = message, Data = data, HttpStatusCode = 200 };

    public static ApiResponse<T> NotFound(string message) =>
        new() { Success = false, Message = message, HttpStatusCode = 404 };

    public static ApiResponse<T> BadRequest(string message) =>
        new() { Success = false, Message = message, HttpStatusCode = 400 };

    public static ApiResponse<T> Error(string message) =>
        new() { Success = false, Message = message, HttpStatusCode = 500 };
}
