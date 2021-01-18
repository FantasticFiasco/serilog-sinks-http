using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Network
{
    public class BufferReaderShould
    {
        private const string FooLogEvent = "{ \"foo\": 1 }";
        private const string BarLogEvent = "{ \"bar\": 2 }";

        private long nextLineBeginsAtOffset;
        private int count;

        [Fact]
        public void ReadLogEvent()
        {
            // Arrange
            using var stream = new MemoryStream();

            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(FooLogEvent + Environment.NewLine);
            writer.Flush();

            // Act
            var actual = BufferReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(new[] { FooLogEvent });
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
            count.ShouldBe(1);
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
            var actual = BufferReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(new[] { FooLogEvent, BarLogEvent });
            nextLineBeginsAtOffset.ShouldBe(stream.Length);
            count.ShouldBe(2);
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
            var actual = BufferReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBeEmpty();
            nextLineBeginsAtOffset.ShouldBe(0);
            count.ShouldBe(0);
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
            var actual = BufferReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(new[] { FooLogEvent });
            nextLineBeginsAtOffset.ShouldBe(BufferReader.BomLength + FooLogEvent.Length + Environment.NewLine.Length);
            count.ShouldBe(1);
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
            var actual = BufferReader.Read(stream, ref nextLineBeginsAtOffset, ref count, batchPostingLimit);

            // Assert
            actual.ShouldBe(new[] { FooLogEvent });
            nextLineBeginsAtOffset.ShouldBe(BufferReader.BomLength + FooLogEvent.Length + Environment.NewLine.Length);
            count.ShouldBe(1);
        }
    }
}
