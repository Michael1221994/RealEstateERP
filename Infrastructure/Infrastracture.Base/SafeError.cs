using System.Diagnostics;

namespace Infrastracture.Base
{
    /// <summary>
    /// Client-safe error text. Exception details (DB messages, stack traces, connection
    /// strings) must never reach the client; they are logged server-side and correlated
    /// back to the client via <see cref="TraceId"/>.
    /// </summary>
    public static class SafeError
    {
        public const string UnexpectedMessage = "An unexpected error occurred. Please try again, or contact support with the reference code.";

        /// <summary>
        /// W3C trace id of the current request, set by ASP.NET Core hosting. Matches the
        /// id the global exception middleware logs and returns in the X-Trace-Id header.
        /// </summary>
        public static string? TraceId => Activity.Current?.TraceId.ToString();

        public static Response<T> Unexpected<T>(Exception? ex = null)
            => Response<T>.Error(UnexpectedMessage, ex, TraceId);
    }
}
