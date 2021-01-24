using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Network
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

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Flush();

            // Act
            var actual = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            actual.LogEvents.ShouldBe(new[] { FooLogEvent });
            actual.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
        }

        [Fact]
        public void ReadLogEvents()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            // Act
            var actual = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            actual.LogEvents.ShouldBe(new[] { FooLogEvent, BarLogEvent });
            actual.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
        }

        [Fact]
        public void NotReadFirstLogEventGivenPartiallyWritten()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(FooLogEvent);  // The partially written log event is missing new line
            writer.Flush();

            // Act
            var actual = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            actual.LogEvents.ShouldBeEmpty();
            actual.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(0);
        }

        [Fact]
        public void NotReadSecondLogEventGivenPartiallyWritten()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent);  // The partially written log event is missing new line
            writer.Flush();

            // Act
            var actual = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, long.MaxValue);

            // Assert
            actual.LogEvents.ShouldBe(new[] { FooLogEvent });
            actual.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(BufferFileReader.BomLength + FooLogEvent.Length + Environment.NewLine.Length);
        }

        [Fact]
        public void RespectBatchPostingLimit()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            const int batchPostingLimit = 1;

            // Act
            var actual = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, batchPostingLimit, long.MaxValue);

            // Assert
            actual.LogEvents.ShouldBe(new[] { FooLogEvent });
            actual.HasReachedLimit.ShouldBeTrue();
            nextLineBeginsAtOffset.ShouldBe(BufferFileReader.BomLength + FooLogEvent.Length + Environment.NewLine.Length);
        }

        [Fact]
        public void RespectBatchSizeLimit()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            var batchSizeLimit = stream.Length * 2 / 3;

            // Act
            var actual = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, batchSizeLimit);

            // Assert
            actual.LogEvents.ShouldBe(new[] { FooLogEvent });
            actual.HasReachedLimit.ShouldBeTrue();
            nextLineBeginsAtOffset.ShouldBe(BufferFileReader.BomLength + FooLogEvent.Length + Environment.NewLine.Length);
        }

        [Fact]
        public void SkipLogEventGivenItExceedsBatchSizeLimit()
        {
            // Arrange
            using var stream = new MemoryStream();

            const string logEventExceedingBatchSizeLimit = "{ \"foo\": \"This document exceeds the batch size limit\" }";

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(logEventExceedingBatchSizeLimit + Environment.NewLine);
            writer.Write(BarLogEvent + Environment.NewLine);
            writer.Flush();

            var batchSizeLimit = ByteSize.From(logEventExceedingBatchSizeLimit) - 1;

            // Act
            var actual = BufferFileReader.Read(stream, ref nextLineBeginsAtOffset, int.MaxValue, batchSizeLimit);

            // Assert
            actual.LogEvents.ShouldBe(new[] { BarLogEvent });
            actual.HasReachedLimit.ShouldBeFalse();
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
        }
    }
}
