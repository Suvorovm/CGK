using System;

namespace CGK.Dispatcher.Error
{
    public class DispatcherException : Exception
    {
        public DispatcherException(string message) : base(message)
        {
        }
    }
}