namespace DzentoPOS2.Shared.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(List<string> errors)
            : base("Validation Error")
        {
            Errors = errors;
        }
    }
}