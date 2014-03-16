namespace GOLD
{
    using System;

    public class ParserException : Exception
    {
        internal ParserException(string message)
            : base(message)
        {
            this.Method = string.Empty;
        }

        internal ParserException(string message, Exception inner, string method)
            : base(message, inner)
        {
            this.Method = method;
        }

        public string Method { get; set; }
    }
}