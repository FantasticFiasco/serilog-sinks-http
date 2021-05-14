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

        //[Fact]
        //public void HandleYearsDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        // "txt" is used from >= v8
        //        "Buffer-2008.json",
        //        "Buffer-2008.txt",
        //        "Buffer-2009.json",
        //        "Buffer-2009.txt",

        //        "Buffer-2010.json",
        //        "Buffer-2011.json",
                
                
                
        //        "Buffer-2010.txt",
        //        "Buffer-2011.txt"
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
        //public void HandleMonths()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        "Buffer-200111.json",
        //        "Buffer-200112.json",
        //        "Buffer-200201.json",
        //        "Buffer-200202.json"
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
        //public void HandleMonthsDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        "Buffer-200111.json",
        //        "Buffer-200112.json",
        //        "Buffer-200201.json",
        //        "Buffer-200202.json",
        //        // "txt" is used from >= v8
        //        "Buffer-200111.txt",
        //        "Buffer-200112.txt",
        //        "Buffer-200201.txt",
        //        "Buffer-200202.txt"
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
        //public void HandleDays()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        "Buffer-20011230.json",
        //        "Buffer-20011231.json",
        //        "Buffer-20020101.json",
        //        "Buffer-20020102.json"
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
        //public void HandleDaysDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        "Buffer-20011230.json",
        //        "Buffer-20011231.json",
        //        "Buffer-20020101.json",
        //        "Buffer-20020102.json",
        //        // "txt" is used from >= v8
        //        "Buffer-20011230.txt",
        //        "Buffer-20011231.txt",
        //        "Buffer-20020101.txt",
        //        "Buffer-20020102.txt"
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
        //public void HandleHours()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        "Buffer-2001123122.json",
        //        "Buffer-2001123123.json",
        //        "Buffer-2002010100.json",
        //        "Buffer-2002010101.json"
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
        //public void HandleHoursDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        "Buffer-2001123122.json",
        //        "Buffer-2001123123.json",
        //        "Buffer-2002010100.json",
        //        "Buffer-2002010101.json",
        //        // "txt" is used from >= v8
        //        "Buffer-2001123122.txt",
        //        "Buffer-2001123123.txt",
        //        "Buffer-2002010100.txt",
        //        "Buffer-2002010101.txt"
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
        //public void HandleMinutes()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        "Buffer-200112312358.json",
        //        "Buffer-200112312359.json",
        //        "Buffer-200201010000.json",
        //        "Buffer-200201010001.json"            };

        //    directoryService
        //        .Setup(mock => mock.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(Randomize.Values(want));

        //    // Act
        //    var got = bufferFiles.Get();

        //    // Assert
        //    got.ShouldBe(want);
        //}

        //[Fact]
        //public void HandleMinutesDuringV8Migration()
        //{
        //    // Arrange
        //    var want = new[]
        //    {
        //        // "json" extension was used < v8
        //        "Buffer-200112312358.json",
        //        "Buffer-200112312359.json",
        //        "Buffer-200201010000.json",
        //        "Buffer-200201010001.json",
        //        // "txt" is used from >= v8
        //        "Buffer-200112312358.txt",
        //        "Buffer-200112312359.txt",
        //        "Buffer-200201010000.txt",
        //        "Buffer-200201010001.txt"
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
