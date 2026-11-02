namespace TrustSystem.Backend.Application.Interfaces;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? Warning { get; set; }
    public string? Info { get; set; }

    private ApiResponse() { }
    public static ApiResponse<T> Success(T data, string? warning = null, string? info = null)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Warning = warning,
            Info = info
        };
    }
    // How do I call Success method?
    // 

    public static ApiResponse<T> Failure(string error)
    {
        return new ApiResponse<T>
        {
            Error = error
        };
    }
}