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

        private long nextLineBeginsAtOffset = 1;    // Start at offset 1 to get around the issue with BOM
        private int count;

        [Fact]
        public void ReadLogEvent()
        {
            // Arrange
            // The initial space is there to adapt to offset 1
            var fileContent = $" {Foo}{Environment.NewLine}";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(new[] { Foo });
            nextLineBeginsAtOffset.ShouldBe(fileContent.Length);
            count.ShouldBe(1);
        }

        [Fact]
        public void NotReadLogEventGivenPartiallyWritten()
        {
            // Arrange
            // The initial space is there to adapt to offset 1, and the partially written log event
            // is missing new line
            var fileContent = $" {Foo}";   

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBeEmpty();
            nextLineBeginsAtOffset.ShouldBe(fileContent.IndexOf(Foo, StringComparison.InvariantCulture));
            count.ShouldBe(0);
        }

        [Fact]
        public void ReadLogEvents()
        {
            // Arrange
            // The initial space is there to adapt to offset 1
            var fileContent =
                $" {Foo}{Environment.NewLine}" +
                $"{Bar}{Environment.NewLine}";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(new[] { Foo, Bar });
            nextLineBeginsAtOffset.ShouldBe(fileContent.Length);
            count.ShouldBe(2);
        }
        
        [Fact]
        public void NotReadEventsGivenPartiallyWritten()
        {
            // Arrange
            // The initial space is there to adapt to offset 1, and the partially written log event
            // is missing new line
            var fileContent =
                $" {Foo}{Environment.NewLine}" +
                $"{Bar}";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(new [] { Foo });
            nextLineBeginsAtOffset.ShouldBe(fileContent.IndexOf(Bar, StringComparison.InvariantCulture));
            count.ShouldBe(1);
        }
    }
}
