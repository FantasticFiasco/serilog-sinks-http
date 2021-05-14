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
            bufferFiles = new FileSizeRolledBufferFiles(directoryService.Object, "SomeBuffer");
        }

        [Fact]
        public void HandleThreeDigitSequenceNumbers()
        {
            // Arrange
            var want = new[]
            {
                "SomeBuffer-20001020.json",
                "SomeBuffer-20001020_001.json",
                "SomeBuffer-20001020_002.json",
                "SomeBuffer-20001020_003.json",
                "SomeBuffer-20001020_004.json",
                "SomeBuffer-20001020_005.json",
                "SomeBuffer-20001020_006.json",
                "SomeBuffer-20001020_007.json",
                "SomeBuffer-20001020_008.json",
                "SomeBuffer-20001020_009.json",
                "SomeBuffer-20001020_010.json"
            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Randomize.Values(want));

            // Act
            var got = bufferFiles.Get();

            // Assert
            got.ShouldBe(want);
        }

        //[Fact]
        //public void HandleThreeDigitsDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        "SomeBuffer-20001020.json",
        //        "SomeBuffer-20001020_001.json",
        //        "SomeBuffer-20001020_002.json",
        //        "SomeBuffer-20001020_003.json",
        //        "SomeBuffer-20001020_004.json",
        //        "SomeBuffer-20001020_005.json",
        //        "SomeBuffer-20001020_006.json",
        //        "SomeBuffer-20001020_007.json",
        //        "SomeBuffer-20001020_008.json",
        //        "SomeBuffer-20001020_009.json",
        //        "SomeBuffer-20001020_010.json",
        //        // "txt" is used from >= v8
        //        "SomeBuffer-20001020.txt",
        //        "SomeBuffer-20001020_001.txt",
        //        "SomeBuffer-20001020_002.txt",
        //        "SomeBuffer-20001020_003.txt",
        //        "SomeBuffer-20001020_004.txt",
        //        "SomeBuffer-20001020_005.txt",
        //        "SomeBuffer-20001020_006.txt",
        //        "SomeBuffer-20001020_007.txt",
        //        "SomeBuffer-20001020_008.txt",
        //        "SomeBuffer-20001020_009.txt",
        //        "SomeBuffer-20001020_010.txt"
        //    };

        //    directoryService
        //        .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(Randomize.Values(want));

        //    // Act
        //    var got = bufferFiles.Get();

        //    // Assert
        //    got.ShouldBe(want);
        //}

        //[Fact]
        //public void HandleFourDigits()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        "SomeBuffer-20001020_999.json",
        //        "SomeBuffer-20001020_1000.json",
        //        "SomeBuffer-20001020_1001.json"
        //    };

        //    directoryService
        //        .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(Randomize.Values(want));

        //    // Act
        //    var got = bufferFiles.Get();

        //    // Assert
        //    got.ShouldBe(want);
        //}

        //[Fact]
        //public void HandleFourDigitsDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        "SomeBuffer-20001020_999.json",
        //        "SomeBuffer-20001020_1000.json",
        //        "SomeBuffer-20001020_1001.json",
        //        // "txt" is used from >= v8
        //        "SomeBuffer-20001020_999.txt",
        //        "SomeBuffer-20001020_1000.txt",
        //        "SomeBuffer-20001020_1001.txt"
        //    };

        //    directoryService
        //        .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(Randomize.Values(want));

        //    // Act
        //    var got = bufferFiles.Get();

        //    // Assert
        //    got.ShouldBe(want);
        //}

        //[Fact]
        //public void HandleFiveDigits()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        "SomeBuffer-20001020_9999.json",
        //        "SomeBuffer-20001020_10000.json",
        //        "SomeBuffer-20001020_10001.json"
        //    };

        //    directoryService
        //        .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(Randomize.Values(want));

        //    // Act
        //    var got = bufferFiles.Get();

        //    // Assert
        //    got.ShouldBe(want);
        //}

        //[Fact]
        //public void HandleFiveDigitsDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        "SomeBuffer-20001020_9999.json",
        //        "SomeBuffer-20001020_10000.json",
        //        "SomeBuffer-20001020_10001.json",
        //        // "txt" is used from >= v8
        //        "SomeBuffer-20001020_9999.txt",
        //        "SomeBuffer-20001020_10000.txt",
        //        "SomeBuffer-20001020_10001.txt"
        //    };

        //    directoryService
        //        .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(Randomize.Values(want));

        //    // Act
        //    var got = bufferFiles.Get();

        //    // Assert
        //    got.ShouldBe(want);
        //}
    }
}
