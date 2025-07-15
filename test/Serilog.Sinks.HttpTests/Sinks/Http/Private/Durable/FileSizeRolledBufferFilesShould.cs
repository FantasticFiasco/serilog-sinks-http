using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Sinks.Http.Private.IO;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable;

public class BookmarkFileNameData : IEnumerable<object[]>
{
    private readonly List<object[]> _data =
    [
        ["SomeBuffer", Path.Combine("{CurrentDirectory}", "SomeBuffer.bookmark")],
        [Path.Combine(".", "SomeBuffer"), Path.Combine("{CurrentDirectory}", "SomeBuffer.bookmark")],
        [Path.Combine("Folder", "SomeBuffer"), Path.Combine("{CurrentDirectory}", "Folder", "SomeBuffer.bookmark")],
        [Path.Combine(".", "Folder", "SomeBuffer"), Path.Combine("{CurrentDirectory}", "Folder", "SomeBuffer.bookmark")],
        [Path.Combine("..", "Folder", "SomeBuffer"), Path.Combine("{CurrentDirectory}", "..", "Folder", "SomeBuffer.bookmark")],
        [Path.Combine(".", "..", "Folder", "SomeBuffer"), Path.Combine("{CurrentDirectory}", "..", "Folder", "SomeBuffer.bookmark")],
        [Path.Combine("C:", "SomeBuffer"), Path.Combine("C:", "SomeBuffer.bookmark")],
        [Path.Combine("C:", "Folder", "SomeBuffer"), Path.Combine("C:", "Folder", "SomeBuffer.bookmark")],
    ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class FileSizeRolledBufferFilesShould
{
    private readonly DirectoryServiceMock directoryService;

    public FileSizeRolledBufferFilesShould()
    {
        directoryService = new DirectoryServiceMock();
    }

    [Theory]
    [ClassData(typeof(BookmarkFileNameData))]
    public void HaveBookmarkFileName(string bufferBaseFilePath, string want)
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, bufferBaseFilePath);

        want = want.Replace("{CurrentDirectory}", Environment.CurrentDirectory);
        want = Path.GetFullPath(want);

        // Act
        var got = bufferFiles.BookmarkFileName;

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void GetOnlyBufferFiles()
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, "SomeBuffer");

        var want = new[]
        {
            "SomeBuffer-20001020.txt",
            "SomeBuffer-20001020_001.txt",
            "SomeBuffer-20001020_010.txt",
            "SomeBuffer-20001020_100.txt",
            "SomeBuffer-20001020_1000.txt",
            "SomeBuffer-20001020_10000.txt"
        };

        directoryService.Files = Randomize.Values(
            want.Concat(new[]
            {
                // Wrong extension
                "SomeBuffer-20001020.config",
                "SomeBuffer-20001020.dll",
                "SomeBuffer-20001020.exe",
                "SomeBuffer-20001020.xml",
                // Wrong file name format
                "SomeBuffer.txt",
                "SomeBuffer.json",
                "XSomeBuffer-20001020.txt",
                "XSomeBuffer-20001020.json",
                "SomeBufferX-20001020.txt",
                "SomeBufferX-20001020.json",
                "SomeBuffer-X20001020.txt",
                "SomeBuffer-X20001020.json",
                "SomeBuffer-20001020X.txt",
                "SomeBuffer-20001020X.json",
                "SomeBuffer-20001020.Xtxt",
                "SomeBuffer-20001020.Xjson"
            }));

        // Act
        var got = bufferFiles.Get();

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void HandleThreeDigitSequenceNumbers()
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, "SomeBuffer");

        var want = new[]
        {
            "SomeBuffer-20001020.txt",
            "SomeBuffer-20001020_001.txt",
            "SomeBuffer-20001020_002.txt",
            "SomeBuffer-20001020_003.txt",
            "SomeBuffer-20001020_004.txt",
            "SomeBuffer-20001020_005.txt",
            "SomeBuffer-20001020_006.txt",
            "SomeBuffer-20001020_007.txt",
            "SomeBuffer-20001020_008.txt",
            "SomeBuffer-20001020_009.txt",
            "SomeBuffer-20001020_010.txt"
        };

        directoryService.Files = Randomize.Values(want);

        // Act
        var got = bufferFiles.Get();

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void HandleThreeDigitSequenceNumbersDuringV8Migration()
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, "SomeBuffer");

        var want = new[]
        {
            // "json" extension was used < v8
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
            "SomeBuffer-20001020_010.json",
            // "txt" is used from >= v8
            "SomeBuffer-20001020.txt",
            "SomeBuffer-20001020_001.txt",
            "SomeBuffer-20001020_002.txt",
            "SomeBuffer-20001020_003.txt",
            "SomeBuffer-20001020_004.txt",
            "SomeBuffer-20001020_005.txt",
            "SomeBuffer-20001020_006.txt",
            "SomeBuffer-20001020_007.txt",
            "SomeBuffer-20001020_008.txt",
            "SomeBuffer-20001020_009.txt",
            "SomeBuffer-20001020_010.txt"
        };

        directoryService.Files = Randomize.Values(want);

        // Act
        var got = bufferFiles.Get();

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void HandleFourDigitSequenceNumbers()
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, "SomeBuffer");

        var want = new[]
        {
            "SomeBuffer-20001020_999.txt",
            "SomeBuffer-20001020_1000.txt",
            "SomeBuffer-20001020_1001.txt"
        };

        directoryService.Files = Randomize.Values(want);

        // Act
        var got = bufferFiles.Get();

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void HandleFourDigitSequenceNumbersDuringV8Migration()
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, "SomeBuffer");

        var want = new[]
        {
            // "json" extension was used < v8
            "SomeBuffer-20001020_999.json",
            "SomeBuffer-20001020_1000.json",
            "SomeBuffer-20001020_1001.json",
            // "txt" is used from >= v8
            "SomeBuffer-20001020.txt",
            "SomeBuffer-20001020_001.txt",
            "SomeBuffer-20001020_002.txt"
        };

        directoryService.Files = Randomize.Values(want);

        // Act
        var got = bufferFiles.Get();

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void HandleFiveDigitSequenceNumbers()
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, "SomeBuffer");

        var want = new[]
        {
            "SomeBuffer-20001020_9999.txt",
            "SomeBuffer-20001020_10000.txt",
            "SomeBuffer-20001020_10001.txt"
        };

        directoryService.Files = Randomize.Values(want);

        // Act
        var got = bufferFiles.Get();

        // Assert
        got.ShouldBe(want);
    }

    [Fact]
    public void HandleFiveDigitsDuringV8Migration()
    {
        // Arrange
        var bufferFiles = new FileSizeRolledBufferFiles(directoryService, "SomeBuffer");

        var want = new[]
        {
            // "json" extension was used < v8
            "SomeBuffer-20001020_9999.json",

           "SomeBuffer-20001020_10000.json",
            "SomeBuffer-20001020_10001.json",
            // "txt" is used from >= v8
            "SomeBuffer-20001020_001.txt",
            "SomeBuffer-20001020_002.txt",
            "SomeBuffer-20001020_003.txt"
        };

        directoryService.Files = Randomize.Values(want);

        // Act
        var got = bufferFiles.Get();

        // Assert
        got.ShouldBe(want);
    }
}
