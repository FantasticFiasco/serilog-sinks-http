// Copyright 2015-2023 Serilog Contributors
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
        private readonly Queue<string> queue;
        private readonly long? queueLimitBytes;
        private readonly long? queueSizeLimit;
        private readonly object syncRoot = new();

        private long queueBytes;
        private long queueSize;

        public LogEventQueue(long? queueLimitBytes = null, long? queueSizeLimit = null)
        {
            if (queueLimitBytes < 1)
                throw new ArgumentException("queueLimitBytes must be either null or greater than 0", nameof(queueLimitBytes));

            queue = new Queue<string>();
            this.queueLimitBytes = queueLimitBytes;
            this.queueSizeLimit = queueSizeLimit;

            queueBytes = 0;
            queueSize = 0;
        }

        public void Enqueue(string logEvent)
        {
            var result = TryEnqueue(logEvent);
            if (result != EnqueueResult.Ok)
            {
                throw new Exception($"Enqueue log event failed: {result}");
            }
        }

        public EnqueueResult TryEnqueue(string logEvent)
        {
            lock (syncRoot)
            {
                var logEventByteSize = ByteSize.From(logEvent);
                if (queueBytes + logEventByteSize > queueLimitBytes || queueSize + 1 > queueSizeLimit)
                {
                    return EnqueueResult.QueueFull;
                }

                queueBytes += logEventByteSize;
                queueSize++;
                queue.Enqueue(logEvent);
                return EnqueueResult.Ok;
            }
        }

        public DequeueResult TryDequeue(long? logEventMaxSize, out string logEvent)
        {
            lock (syncRoot)
            {
                if (queue.Count == 0)
                {
                    logEvent = string.Empty;
                    return DequeueResult.QueueEmpty;
                }

                logEvent = queue.Peek();
                var logEventByteSize = ByteSize.From(logEvent);

                if (logEventByteSize > logEventMaxSize)
                {
                    logEvent = string.Empty;
                    return DequeueResult.MaxSizeViolation;
                }

                queueBytes -= logEventByteSize;
                queueSize--;
                queue.Dequeue();
                return DequeueResult.Ok;
            }
        }

        public enum EnqueueResult
        {
            Ok,
            QueueFull
        }

        public enum DequeueResult
        {
            Ok,
            QueueEmpty,
            MaxSizeViolation
        }
    }
}
