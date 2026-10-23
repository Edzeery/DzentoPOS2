 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Shared.Results
{
     
    public class Result<T> : Result
    {
        public T? Data { get; set; }

        public static Result<T> Succeeded(
            T data,
            string? message = null)
        {
            return new Result<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public new static Result<T> Failed(string message)
        {
            return new Result<T>
            {
                Success = false,
                Message = message
            };
        }
    }
}
