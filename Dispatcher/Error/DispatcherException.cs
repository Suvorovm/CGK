using System;

namespace CommonGameKit.CommonGameKit.Dispatcher.Error
{
    public class DispatcherException : Exception
    {
        public DispatcherException(string message) : base(message)
        {
        }
    }
}