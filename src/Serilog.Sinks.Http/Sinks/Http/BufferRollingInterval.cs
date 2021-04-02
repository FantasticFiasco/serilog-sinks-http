// Copyright 2015-2021 Serilog Contributors
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

namespace Serilog.Sinks.Http
{
    /// <summary>
    /// Specifies the frequency at which the buffer files should roll.
    /// </summary>
    public enum BufferRollingInterval
    {
        /// <summary>
        /// Buffer files roll every year.
        /// </summary>
        Year,

        /// <summary>
        /// Buffer files roll every month.
        /// </summary>
        Month,

        /// <summary>
        /// Buffer files roll every day.
        /// </summary>
        Day,

        /// <summary>
        /// Buffer files roll every hour.
        /// </summary>
        Hour,

        /// <summary>
        /// Buffer files roll every minute.
        /// </summary>
        Minute
    }
}
