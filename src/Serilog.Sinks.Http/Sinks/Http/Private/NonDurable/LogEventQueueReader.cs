// Copyright 2015-2025 Serilog Contributors
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

using Serilog.Debugging;

namespace Serilog.Sinks.Http.Private.NonDurable;

public static class LogEventQueueReader
{
    public static Batch Read(LogEventQueue queue, int? logEventsInBatchLimit, long? batchSizeLimitBytes)
    {
        var batch = new Batch();
        var remainingBatchSizeBytes = batchSizeLimitBytes;

        while (true)
        {
            var result = queue.TryDequeue(remainingBatchSizeBytes, out var logEvent);
            if (result == LogEventQueue.DequeueResult.Ok)
            {
                batch.LogEvents.Add(logEvent);
                remainingBatchSizeBytes -= ByteSize.From(logEvent);

                // Respect batch posting limit
                if (batch.LogEvents.Count == logEventsInBatchLimit)
                {
                    batch.HasReachedLimit = true;
                    break;
                }
            }
            else if (result == LogEventQueue.DequeueResult.MaxSizeViolation)
            {
                if (batch.LogEvents.Count == 0)
                {
                    // This single log event exceeds the batch size limit, let's drop it
                    queue.TryDequeue(long.MaxValue, out var logEventToDrop);

                    SelfLog.WriteLine(
                        "Event exceeds the batch size limit of {0} bytes set for this sink and will be dropped; data: {1}",
                        batchSizeLimitBytes,
                        logEventToDrop);
                }
                else
                {
                    batch.HasReachedLimit = true;
                    break;
                }
            }
            else if (result == LogEventQueue.DequeueResult.QueueEmpty)
            {
                break;
            }
        }

        return batch;
    }
}