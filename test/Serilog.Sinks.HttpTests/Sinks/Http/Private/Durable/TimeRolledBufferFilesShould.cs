using Moq;
using Serilog.Sinks.Http.Private.IO;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable
{
    public class TimeRolledBufferFilesShould
    {
        private readonly Mock<IDirectoryService> directoryService;
        private readonly TimeRolledBufferFiles bufferFiles;

        public TimeRolledBufferFilesShould()
        {
            directoryService = new Mock<IDirectoryService>();
            bufferFiles = new TimeRolledBufferFiles(directoryService.Object, "Buffer");
        }

        [Fact]
        public void HandleYears()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-2008.json",
                "Buffer-2009.json",
                "Buffer-2010.json",
                "Buffer-2011.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Randomize.Values(want));

            // Act
            var got = bufferFiles.Get();

            // Assert
            got.ShouldBe(want);
        }

        [Fact]
        public void HandleMonths()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-200111.json",
                "Buffer-200112.json",
                "Buffer-200201.json",
                "Buffer-200202.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Randomize.Values(want));

            // Act
            var got = bufferFiles.Get();

            // Assert
            got.ShouldBe(want);
        }

        [Fact]
        public void HandleDays()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-20011230.json",
                "Buffer-20011231.json",
                "Buffer-20020101.json",
                "Buffer-20020102.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Randomize.Values(want));

            // Act
            var got = bufferFiles.Get();

            // Assert
            got.ShouldBe(want);
        }

        [Fact]
        public void HandleHours()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-2001123122.json",
                "Buffer-2001123123.json",
                "Buffer-2002010100.json",
                "Buffer-2002010101.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Randomize.Values(want));

            // Act
            var got = bufferFiles.Get();

            // Assert
            got.ShouldBe(want);
        }

        [Fact]
        public void HandleMinutes()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-200112312358.json",
                "Buffer-200112312359.json",
                "Buffer-200201010000.json",
                "Buffer-200201010001.json"            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Randomize.Values(want));

            // Act
            var got = bufferFiles.Get();

            // Assert
            got.ShouldBe(want);
        }
    }
}
