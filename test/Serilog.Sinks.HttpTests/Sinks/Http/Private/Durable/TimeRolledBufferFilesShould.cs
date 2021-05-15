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
            bufferFiles = new TimeRolledBufferFiles(directoryService.Object, "SomeBuffer");
        }

        [Fact]
        public void HandleYears()
        {
            // Arrange
            var want = new[]
            {
                "SomeBuffer-2008.txt",
                "SomeBuffer-2009.txt",
                "SomeBuffer-2010.txt",
                "SomeBuffer-2011.txt"
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
        public void HandleYearsDuringV8Migration()
        {
            // Arrange
            var want = new[]
            {
                // "json" extension was used < v8
                "SomeBuffer-2008.json",
                "SomeBuffer-2009.json",
                "SomeBuffer-2010.json",
                // "txt" is used from >= v8
                "SomeBuffer-2010.txt",
                "SomeBuffer-2011.txt",
                "SomeBuffer-2012.txt"
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
                "SomeBuffer-200111.txt",
                "SomeBuffer-200112.txt",
                "SomeBuffer-200201.txt",
                "SomeBuffer-200202.txt"
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
        public void HandleMonthsDuringV8Migration()
        {
            // Arrange
            var want = new[]
            {
                // "json" extension was used < v8
                "SomeBuffer-200111.json",
                "SomeBuffer-200112.json",
                "SomeBuffer-200201.json",
                // "txt" is used from >= v8
                "SomeBuffer-200201.txt",
                "SomeBuffer-200202.txt",
                "SomeBuffer-200203.txt"
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
                "SomeBuffer-20011230.txt",
                "SomeBuffer-20011231.txt",
                "SomeBuffer-20020101.txt",
                "SomeBuffer-20020102.txt"
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
        public void HandleDaysDuringV8Migration()
        {
            // Arrange
            var want = new[]
            {
                // "json" extension was used < v8
                "SomeBuffer-20011230.json",
                "SomeBuffer-20011231.json",
                "SomeBuffer-20020101.json",
                // "txt" is used from >= v8
                "SomeBuffer-20020101.txt",
                "SomeBuffer-20020102.txt",
                "SomeBuffer-20020103.txt"
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
                "SomeBuffer-2001123122.txt",
                "SomeBuffer-2001123123.txt",
                "SomeBuffer-2002010100.txt",
                "SomeBuffer-2002010101.txt"
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
        public void HandleHoursDuringV8Migration()
        {
            // Arrange
            var want = new[]
            {
                // "json" extension was used < v8
                "SomeBuffer-2001123122.json",
                "SomeBuffer-2001123123.json",
                "SomeBuffer-2002010100.json",
                // "txt" is used from >= v8
                "SomeBuffer-2002010100.txt",
                "SomeBuffer-2002010101.txt",
                "SomeBuffer-2002010102.txt"
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
                "SomeBuffer-200112312358.txt",
                "SomeBuffer-200112312359.txt",
                "SomeBuffer-200201010000.txt",
                "SomeBuffer-200201010001.txt"            };

            directoryService
                .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Randomize.Values(want));

            // Act
            var got = bufferFiles.Get();

            // Assert
            got.ShouldBe(want);
        }

        [Fact]
        public void HandleMinutesDuringV8Migration()
        {
            // Arrange
            var want = new[]
            {
                // "json" extension was used < v8
                "SomeBuffer-200112312358.json",
                "SomeBuffer-200112312359.json",
                "SomeBuffer-200201010000.json",
                // "txt" is used from >= v8
                "SomeBuffer-200201010000.txt",
                "SomeBuffer-200201010001.txt",
                "SomeBuffer-200201010002.txt"
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
