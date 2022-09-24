using System;

namespace WorkTimer4.API.Connectors
{
    public class ConnectorException : Exception
    {
        public ConnectorException(string? message)
            : base(message)
        {
        }
    }
}
