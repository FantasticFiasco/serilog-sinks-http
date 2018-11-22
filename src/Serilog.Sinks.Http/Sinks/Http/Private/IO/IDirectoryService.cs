// Copyright 2015-2018 Serilog Contributors
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

namespace Serilog.Sinks.Http.Private.IO
{
    public interface IDirectoryService
    {
        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search
        /// pattern in the specified directory.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search. This string is not
        /// case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in <paramref name="path" />. This
        /// parameter can contain a combination of valid literal path and wildcard (* and ?)
        /// characters, but doesn't support regular expressions.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory
        /// that match the specified search pattern, or an empty array if no files are found.
        /// </returns>
        string[] GetFiles(string path, string searchPattern);
    }
}
