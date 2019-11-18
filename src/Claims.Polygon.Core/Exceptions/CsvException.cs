using System;
using Claims.Polygon.Core.Enums;

namespace Claims.Polygon.Core.Exceptions
{
    public class CsvException : Exception
    {
        public CsvExceptionType Type { get; }

        public CsvException()
        {
        }

        public CsvException(CsvExceptionType type, string message)
            : base(message)
        {
            Type = type;
        }

        public CsvException(CsvExceptionType type, string message, Exception innException)
            : base(message, innException)
        {
            Type = type;
        }
    }
}
