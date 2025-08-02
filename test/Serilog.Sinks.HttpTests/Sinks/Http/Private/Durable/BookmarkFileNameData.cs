using System.Collections;
using System.Collections.Generic;
using System.IO;

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
