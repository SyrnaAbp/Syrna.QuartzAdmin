namespace Syrna.QuartzAdmin
{
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public new static ApiResponse<T> Fail(string errorCode, string errorMessage)
        {
            return new ApiResponse<T> { Succeeded = false, Code = errorCode, Message = errorMessage };
        }

        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T> { Succeeded = true, Data = data };
        }
    }

    public class ApiResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }

        public static ApiResponse Fail(string errorCode, string errorMessage)
        {
            return new ApiResponse { Succeeded = false, Code = errorCode, Message = errorMessage };
        }
        public static ApiResponse Success()
        {
            return new ApiResponse { Succeeded = true};
        }
    }
}