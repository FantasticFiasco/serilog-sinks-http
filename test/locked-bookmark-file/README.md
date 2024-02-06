# Locked bookmark file

Steps to reproduce the issue filed at <https://github.com/FantasticFiasco/serilog-sinks-http/issues/262>.

1. Start three instances of a terminal of your choice.
2. In the first terminal instance, start the web server by navigating into `test\Serilog.Sinks.HttpTests.LogServer` and run `dotnet run Serilog.Sinks.HttpTests.LogServer.csproj`
3. In the second terminal instance, start the first producer by navigating into `test\locked-bookmark-file\producer` and run `dotnet run producer.csproj`
4. In the third terminal instance, start the second producer according to the instructions above

The observed behavior is as follows:

- Since the sink is configured using `bufferFileShared: true`, the same buffer file will be shared among the producers. This is good, as this means that we won't drop any log events from either of the producers.
- The bookmark file is locked when the log events from the buffer file is sent over the network. This means that one of the producers will be prevented from opening the bookmark file when the other producer is in the process of sending the log events.
- Disposing `HttpLogShipper` seems to be slow, I was expecting this to be a much faster process. Let's look into this.
- If app-pooling seems to be a common issue, perhaps we need to catch that specific error and write a more humanly friendly error message to the `SelfLog`.