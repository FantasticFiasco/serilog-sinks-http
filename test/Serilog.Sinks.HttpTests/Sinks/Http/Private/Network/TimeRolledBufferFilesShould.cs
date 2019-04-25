using System.Linq;
using Moq;
using Serilog.Sinks.Http.Private.IO;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Network
{
    public class TimeRolledBufferFilesShould
    {
        private const string BufferPathFormat = "Buffer-{Date}.json";

        private readonly Mock<IDirectoryService> directoryService;
        private readonly TimeRolledBufferFiles bufferFiles;

        public TimeRolledBufferFilesShould()
        {
            directoryService = new Mock<IDirectoryService>();
            bufferFiles = new TimeRolledBufferFiles(directoryService.Object, BufferPathFormat);
        }

        [Fact]
        public void HandleDates()
        {
            // Arrange
            var fileNames = new[]
            {
                "Buffer-20001020.json",
                "Buffer-20001021.json",
                "Buffer-20001022.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fileNames.Reverse().ToArray);  // Reverse expected elements

            // Act
            var actual = bufferFiles.Get();

            // Assert
            actual.ShouldBe(fileNames);
        }

        [Fact]
        public void HandleHours()
        {
            // Arrange
            var fileNames = new[]
            {
                "Buffer-2000102016.json",
                "Buffer-2000102017.json",
                "Buffer-2000102018.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fileNames.Reverse().ToArray);  // Reverse expected elements

            // Act
            var actual = bufferFiles.Get();

            // Assert
            actual.ShouldBe(fileNames);
        }

        [Fact]
        public void HandleHalfHours()
        {
            // Arrange
            var fileNames = new[]
            {
                "Buffer-200010201600.json",
                "Buffer-200010201630.json",
                "Buffer-200010201700.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fileNames.Reverse().ToArray);  // Reverse expected elements

            // Act
            var actual = bufferFiles.Get();

            // Assert
            actual.ShouldBe(fileNames);
        }
    }
}
