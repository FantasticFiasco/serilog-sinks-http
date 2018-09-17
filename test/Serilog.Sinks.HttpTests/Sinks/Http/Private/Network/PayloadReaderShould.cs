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
            var expected = new[] { Foo };

            var fileContent =
                " " +   // Adapt to offset 1
                expected[0] + Environment.NewLine;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(expected);
            nextLineBeginsAtOffset.ShouldBe(fileContent.Length);
            count.ShouldBe(1);
        }

        [Fact]
        public void NotReadLogEventGivenPartiallyWritten()
        {
            // Arrange
            var expected = new string[0];

            var fileContent =
                " " +   // Adapt to offset 1
                Foo;    // Partially written log event since new line is missing

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(expected);
            nextLineBeginsAtOffset.ShouldBe(1);
            count.ShouldBe(0);
        }

        [Fact]
        public void ReadLogEvents()
        {
            // Arrange
            var expected = new[] { Foo, Bar };

            var fileContent =
                " " +   // Adapt to offset 1
                expected[0] + Environment.NewLine +
                expected[1] + Environment.NewLine;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(expected);
            nextLineBeginsAtOffset.ShouldBe(fileContent.Length);
            count.ShouldBe(2);
        }
        
        [Fact]
        public void NotReadEventsGivenPartiallyWritten()
        {
            // Arrange
            var expected = new[] { Foo };

            var fileContent =
                " " +           // Adapt to offset 1
                expected[0] + Environment.NewLine +
                Bar;    // Partially written log event since new line is missing

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            // Act
            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, int.MaxValue);

            // Assert
            actual.ShouldBe(expected);
            nextLineBeginsAtOffset.ShouldBe(fileContent.IndexOf(Bar, StringComparison.InvariantCulture));
            count.ShouldBe(1);
        }
    }
}
