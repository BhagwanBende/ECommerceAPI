namespace ECommerceAPI.DTOs;

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data = default);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);