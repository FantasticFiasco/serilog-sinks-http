#if !NETSTANDARD_2_0
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Represents a set of key/value application configuration properties.
    /// </summary>
    /// <remarks>
    /// Polyfill for
    /// <see href="https://docs.microsoft.com/dotnet/api/microsoft.extensions.configuration.iconfiguration">
    /// Microsoft.Extensions.Configuration.IConfiguration</see>.
    /// </remarks>
    public interface IConfiguration
    {
        /// <summary>
        /// Gets or sets a configuration value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configuration value.</returns>
        string this[string key] { get; set; }
    }
}
#endif
