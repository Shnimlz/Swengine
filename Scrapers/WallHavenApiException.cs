using System;

namespace swengine.desktop.Scrapers
{
    public class WallHavenApiException : Exception
    {
        public int StatusCode { get; }

        public WallHavenApiException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
