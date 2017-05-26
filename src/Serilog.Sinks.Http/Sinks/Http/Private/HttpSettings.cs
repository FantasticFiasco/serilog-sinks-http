
namespace Serilog.Sinks.Http
{
    /// <summary>
    /// Global HTTP Settings
    /// </summary>
    internal static class HttpSettings
    {
        /// The list of date formats supported by the rolling file appender
        public static string[] DateFormats
        {
            get { return new string[] { "{date}", "{hour}", "{halfhour}" }; }
        }
    }
}
