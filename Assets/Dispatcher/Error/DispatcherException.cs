using System;

namespace CommonGameKit.CommonGameKit.Assets.Dispatcher.Error
{
    public class DispatcherException : Exception
    {
        public DispatcherException(string message) : base(message)
        {
        }
    }
}