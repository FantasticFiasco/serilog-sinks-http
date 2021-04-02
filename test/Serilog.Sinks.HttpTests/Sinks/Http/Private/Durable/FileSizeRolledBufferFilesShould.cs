using Moq;
using Serilog.Sinks.Http.Private.IO;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable
{
    public class FileSizeRolledBufferFilesShould
    {
        private readonly Mock<IDirectoryService> directoryService;
        private readonly FileSizeRolledBufferFiles bufferFiles;

        public FileSizeRolledBufferFilesShould()
        {
            directoryService = new Mock<IDirectoryService>();
            bufferFiles = new FileSizeRolledBufferFiles(directoryService.Object, "Buffer");
        }

        [Fact]
        public void HandleThreeDigits()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-20001020.json",
                "Buffer-20001020_001.json",
                "Buffer-20001020_002.json",
                "Buffer-20001020_003.json",
                "Buffer-20001020_004.json",
                "Buffer-20001020_005.json",
                "Buffer-20001020_006.json",
                "Buffer-20001020_007.json",
                "Buffer-20001020_008.json",
                "Buffer-20001020_009.json",
                "Buffer-20001020_010.json"
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
        public void HandleFourDigits()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-20001020_999.json",
                "Buffer-20001020_1000.json",
                "Buffer-20001020_1001.json"
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
        public void HandleFiveDigits()
        {
            // Arrange
            var want = new[]
            {
                "Buffer-20001020_9999.json",
                "Buffer-20001020_10000.json",
                "Buffer-20001020_10001.json"
            };

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
