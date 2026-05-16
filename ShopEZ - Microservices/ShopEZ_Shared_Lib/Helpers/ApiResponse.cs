namespace ShopEZ_Shared_Lib.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public int? Count { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success", int? count = null)
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data, Count = count };
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message };
        }
    }
}