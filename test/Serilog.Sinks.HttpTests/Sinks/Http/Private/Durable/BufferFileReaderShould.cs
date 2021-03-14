using System;
using System.IO;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable
{
    public class BufferFileReaderShould
    {
        private const string FooLogEvent = "{ \"foo\": 1 }";
        private const string BarLogEvent = "{ \"bar\": 2 }";

        private long nextLineBeginsAtOffset;

        [Fact]
        public void ReadLogEvent()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, BufferFileReader.Encoding);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Flush();

            // Act
            var got = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent });
            got.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
        }

        [Fact]
        public void ReadLogEvents()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, BufferFileReader.Encoding);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            // Act
            var got = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent, BarLogEvent });
            got.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
        }

        [Fact]
        public void NotReadFirstLogEventGivenPartiallyWritten()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, BufferFileReader.Encoding);
            writer.Write(FooLogEvent);  // The partially written log event is missing new line
            writer.Flush();

            // Act
            var got = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            got.LogEvents.ShouldBeEmpty();
            got.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(0);
        }

        [Fact]
        public void NotReadSecondLogEventGivenPartiallyWritten()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, BufferFileReader.Encoding);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent);  // The partially written log event is missing new line
            writer.Flush();

            // Act
            var got = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent });
            got.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(FooLogEvent.Length + Environment.NewLine.Length);
        }

        [Fact]
        public void RespectBatchPostingLimit()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, BufferFileReader.Encoding);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            const int batchPostingLimit = 1;

            // Act
            var got = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, batchPostingLimit, long.MaxValue);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent });
            got.HasReachedLimit.ShouldBeTrue();
            nextLineBeginsAtOffset.ShouldBe(FooLogEvent.Length + Environment.NewLine.Length);
        }

        [Fact]
        public void RespectBatchSizeLimit()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, BufferFileReader.Encoding);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            var batchSizeLimit = stream.Length * 2 / 3;

            // Act
            var got = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, batchSizeLimit);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent });
            got.HasReachedLimit.ShouldBeTrue();
            nextLineBeginsAtOffset.ShouldBe(FooLogEvent.Length + Environment.NewLine.Length);
        }

        [Fact]
        public void SkipLogEventGivenItExceedsBatchSizeLimit()
        {
            // Arrange
            using var stream = new MemoryStream();

            const string logEventExceedingBatchSizeLimit = "{ \"foo\": \"This document exceeds the batch size limit\" }";

            using var writer = new StreamWriter(stream, BufferFileReader.Encoding);
            writer.Write(logEventExceedingBatchSizeLimit + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            var batchSizeLimit = ByteSize.From(logEventExceedingBatchSizeLimit) - 1;

            // Act
            var got = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, batchSizeLimit);

            // Assert
            got.LogEvents.ShouldBe(new[] { BarLogEvent });
            got.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
        }
    }
}
