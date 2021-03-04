﻿using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http
{
    public class ByteSizeShould
    {
        [Fact]
        public void ReturnByteValue()
        {
            // Act
            const long got = ByteSize.B;

            // Assert
            got.ShouldBe(1);
        }

        [Fact]
        public void ReturnKiloByteValue()
        {
            // Act
            const long got = ByteSize.KB;

            // Assert
            got.ShouldBe(1024);
        }

        [Fact]
        public void ReturnMegaByteValue()
        {
            // Act
            const long got = ByteSize.MB;

            // Assert
            got.ShouldBe(1048576);
        }

        [Fact]
        public void ReturnGigaByteValue()
        {
            // Act
            const long got = ByteSize.GB;

            // Assert
            got.ShouldBe(1073741824);
        }   

        [Theory]
        [InlineData("a", 1)]
        [InlineData("abcdefghij", 10)]
        [InlineData("abcdefghijklmnopqrstuvwxyz", 26)]
        public void ReturnStringSizeInBytes(string text, long want)
        {
            // Act
            var got = ByteSize.From(text);

            // Assert
            got.ShouldBe(want);
        }
    }
}
