namespace LifeQuestAPI.Application.DTOs;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ApiResponse SuccessResponse(string message = "") => new() { Success = true, Message = message };
    public static ApiResponse ErrorResponse(string message) => new() { Success = false, Message = message };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "") => new() { Success = true, Data = data, Message = message };
    public new static ApiResponse<T> ErrorResponse(string message) => new() { Success = false, Message = message };
}
