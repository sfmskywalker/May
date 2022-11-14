﻿namespace Elsa.Telnyx.Exceptions
{
    public class MissingCallControlAppIdException : TelnyxException
    {
        public MissingCallControlAppIdException(string message, Exception? innerException = default) : base(message, innerException)
        {
        }
    }
}