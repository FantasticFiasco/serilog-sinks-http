using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Network
{
    public class PayloadReaderShould
    {
        private const string Foo = "{ \"foo\": 1 }";
        private const string Bar = "{ \"bar\": 2 }";

        private long nextLineBeginsAtOffset;
        private int count;

        [Fact]
        public void ReadLogEvent()
        {
            // Arrange
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(Foo + Environment.NewLine);
                writer.Flush();

                // Act
                var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

                // Assert
                actual.ShouldBe(new[] { Foo });
                nextLineBeginsAtOffset.ShouldBe(stream.Length);
                count.ShouldBe(1);
            }
        }

        [Fact]
        public void NotReadLogEventGivenPartiallyWritten()
        {
            // Arrange
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                // The partially written log event is missing new line
                writer.Write(Foo);
                writer.Flush();

                // Act
                var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

                // Assert
                actual.ShouldBeEmpty();
                nextLineBeginsAtOffset.ShouldBe(0);
                count.ShouldBe(0);
            }
        }

        [Fact]
        public void ReadLogEvents()
        {
            // Arrange
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(Foo + Environment.NewLine);
                writer.Write(Bar + Environment.NewLine);
                writer.Flush();

                // Act
                var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

                // Assert
                actual.ShouldBe(new[] { Foo, Bar });
                nextLineBeginsAtOffset.ShouldBe(stream.Length);
                count.ShouldBe(2);
            }
        }

        [Fact]
        public void NotReadEventsGivenPartiallyWritten()
        {
            // Arrange
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(Foo + Environment.NewLine);
                // The partially written log event is missing new line
                writer.Write(Bar);
                writer.Flush();

                // Act
                var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

                // Assert
                actual.ShouldBe(new[] { Foo });
                nextLineBeginsAtOffset.ShouldBe(PayloadReader.BomLength + Foo.Length + Environment.NewLine.Length);
                count.ShouldBe(1);
            }
        }
    }
}
