using System;

namespace Mic.Models
{
    public class Result
    {
        public int Count { get; private set; }

        public bool Success { get; private set; }

        public Exception Exception { get; private set; }

        public string Message { get; private set; }

        public string Sql { get; set; }

        public Result(Exception ex)
        {
            Exception = ex;
            Success = false;
        }

        public Result(string message)
        {
            Message = message;
            Success = false;
        }

        public Result(int count)
        {
            Count = count;
            Success = true;
        }

        public Result(bool success)
        {
            Success = success;
        }
    }

    public class Result<T> : Result where T : class, new()
    {
        public T Data { get; set; }

        public Result(Exception ex) : base(ex) { }

        public Result(string message) : base(message) { }

        public Result(int count) : base(count) { }

        public Result(T data) : base(true)
        {
            Data = data;
        }
    }

    public class JsonResult : Result
    {
        public string Data { get; set; }

        public JsonResult(Exception ex) : base(ex) { }
        
        public JsonResult(int count) : base(count) { }

        public JsonResult(string data) : base(true)
        {
            Data = data;
        }
    }

    public class ScalarResult<T> : Result
    {
        public T Data { get; set; }

        public ScalarResult(Exception ex) : base(ex) { }
        
        public ScalarResult(T data) : base(true)
        {
            Data = data;
        }
    }
}