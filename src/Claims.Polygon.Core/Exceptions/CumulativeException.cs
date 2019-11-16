using System;
using Claims.Polygon.Core.Enums;

namespace Claims.Polygon.Core.Exceptions
{
    public class CumulativeException : Exception
    {
        public CumulativeExceptionType Type { get; }

        public CumulativeException()
        {
        }

        public CumulativeException(CumulativeExceptionType type, string message)
            : base(message)
        {
            Type = type;
        }
    }
}
