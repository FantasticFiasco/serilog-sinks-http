// Copyright 2015-2020 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if NET45 || NETSTANDARD_1_3
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
