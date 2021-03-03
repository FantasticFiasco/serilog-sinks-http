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

using System;
using System.Collections.Generic;

namespace Serilog.Sinks.Http.Private.NonDurable
{
    public class LogEventQueue
    {
        private readonly int? queueLimit;
        private readonly Queue<string> queue;
        private readonly object syncRoot = new();

        public LogEventQueue(int? queueLimit)
        {
            if (queueLimit < 1)
                throw new ArgumentException("queueLimit must be either null or greater than 0", nameof(queueLimit));

            this.queueLimit = queueLimit;

            queue = new Queue<string>();
        }

        public bool TryEnqueue(string logEvent)
        {
            lock (syncRoot)
            {
                if (queueLimit.HasValue && queueLimit.Value == queue.Count)
                {
                    return false;
                }

                queue.Enqueue(logEvent);
                return true;
            }
        }

        public bool TryDequeue(out string logEvent)
        {
            lock (syncRoot)
            {
                if (queue.Count == 0)
                {
                    logEvent = null;
                    return false;
                }

                logEvent = queue.Dequeue();
                return true;
            }
        }
    }
}
