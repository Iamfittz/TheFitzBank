namespace TheFitzBankAPI.Application {
    public class Result<T> {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int? ErrorCode { get; private set; }
        private Result(bool success, T data, string? errorMessage = null, int? errorCode = null) {
            IsSuccess = success;
            Data = data;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
        public static Result<T> Success(T data) => new Result<T>(true, data);
        public static Result<T> Failure(string errorMessage, int errorCode = 500) {
            return new Result<T>(false, default, errorMessage, errorCode);
        } 
    }

    public class Result {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int? ErrorCode { get; private set; }
        private Result(bool success, string? errorMessage = null, int? errorCode = null) {
            IsSuccess = success;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
        public static Result Success() => new Result(true);
        public static Result Failure(string errorMessage, int? errorCode = 500) {
            return new Result(false, errorMessage, errorCode);
        } 
    }
}
