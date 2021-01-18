using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http
{
    public class ByteSizeShould
    {
        [Fact]
        public void ReturnByteValue()
        {
            // Act
            const long actual = ByteSize.B;

            // Assert
            actual.ShouldBe(1);
        }

        [Fact]
        public void ReturnKiloByteValue()
        {
            // Act
            const long actual = ByteSize.KB;

            // Assert
            actual.ShouldBe(1024);
        }

        [Fact]
        public void ReturnMegaByteValue()
        {
            // Act
            const long actual = ByteSize.MB;

            // Assert
            actual.ShouldBe(1048576);
        }

        [Fact]
        public void ReturnGigaByteValue()
        {
            // Act
            const long actual = ByteSize.GB;

            // Assert
            actual.ShouldBe(1073741824);
        }   

        [Theory]
        [InlineData("a", 1)]
        [InlineData("abcdefghij", 10)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", 26)]
        public void ReturnStringSizeInBytes(string text, long expected)
        {
            // Act
            var actual = ByteSize.From(text);

            // Assert
            actual.ShouldBe(expected);
        }
    }
}
