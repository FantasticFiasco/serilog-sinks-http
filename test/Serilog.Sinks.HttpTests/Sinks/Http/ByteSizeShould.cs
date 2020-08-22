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
            var actual = ByteSize.B;

            // Assert
            actual.ShouldBe(1);
        }

        [Fact]
        public void ReturnKiloByteValue()
        {
            // Act
            var actual = ByteSize.KB;

            // Assert
            actual.ShouldBe(1024);
        }

        [Fact]
        public void ReturnMegaByteValue()
        {
            // Act
            var actual = ByteSize.MB;

            // Assert
            actual.ShouldBe(1048576);
        }

        [Fact]
        public void ReturnGigaByteValue()
        {
            // Act
            var actual = ByteSize.GB;

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
