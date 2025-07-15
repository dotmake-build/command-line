using System;

namespace DotMake.CommandLine.Util
{
    internal static class ExceptionUtil
    {
        public static ArgumentNullException ParameterNull(string parameterName)
        {
            return new ArgumentNullException(parameterName, "Parameter cannot be null.");
        }

        public static ArgumentException ParameterEmptyString(string parameterName)
        {
            return new ArgumentException("Parameter cannot be empty string.", parameterName);
        }

        public static ArgumentException ParameterEmptyArray(string parameterName)
        {
            return new ArgumentException("Parameter cannot be empty array.", parameterName);
        }

        public static ArgumentException ParameterInvalid(string parameterName, string message)
        {
            return new ArgumentException(message, parameterName);
        }
    }
}
