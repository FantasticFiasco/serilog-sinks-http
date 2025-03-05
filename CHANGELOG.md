# Change Log

All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/) and is following the [change log format](https://keepachangelog.com/).

## Unreleased

### :zap: Added

- [#416](https://github.com/FantasticFiasco/serilog-sinks-http/pull/416) Updates `NormalTextFormatter` to have virtual methods for each field so child implementations can override specific fields instead of requiring the duplication of the entire class. Additionally, `CompactTextFormatter` and `NamespacedTextFormatter` now extend `NormalTextFormatter` to provide a common framework for the text formatters, as well as default logic for most fields. (contribution by [@josephcappellino](https://github.com/josephcappellino))

## [9.1.0] - 2025-01-19

### :zap: Added

- [#409](https://github.com/FantasticFiasco/serilog-sinks-http/issues/409) Have `CompactTextFormatter`, `CompactRenderedTextFormatter`, `NamespacedTextFormatter`, `NormalTextFormatter` and `NormalRenderedTextFormatter` propagate the current trace and span id by writing the values into the JSON payload (contribution by [@consulting-dev](https://github.com/consulting-dev))

## [9.0.0] - 2024-04-14

### :zap: Added

- [#311](https://github.com/FantasticFiasco/serilog-sinks-http/issues/311) [BREAKING CHANGE] Support specifying `levelSwitch` when creating the sink, thus adding the support to [dynamically change the log level at runtime](https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/) (contribution by [@yuriy-millen](https://github.com/yuriy-millen))

  **Migration guide**

  The parameter `levelSwitch` has been introduced to the methods `Http`, `DurableHttpUsingFileSizeRolledBuffers` and `DurableHttpUsingTimeRolledBuffers`. Please verify that the arguments pass by you to these methods still align with your intentions.

  To automatically mitigate this kind of *new parameter issue* in the future, move from using positional arguments to named arguments.
- [#262](https://github.com/FantasticFiasco/serilog-sinks-http/issues/262) [BREAKING CHANGE] Support specifying `flushOnClose` when creating the sink, thus adding the support to suppress sending unsent log events to the log server before closing the sink (proposed by [@murlidakhare](https://github.com/murlidakhare), [@julichan](https://github.com/julichan), [@janichirag11](https://github.com/janichirag11) & [@prasadpaul53](https://github.com/prasadpaul53))

  **Migration guide**

  The parameter `flushOnClose` has been introduced to the methods `Http`, `DurableHttpUsingFileSizeRolledBuffers` and `DurableHttpUsingTimeRolledBuffers`. Please verify that the arguments pass by you to these methods still align with your intentions.

  To automatically mitigate this kind of *new parameter issue* in the future, move from using positional arguments to named arguments.
- Support for .NET Framework 4.6.2

### :dizzy: Changed

- [#262](https://github.com/FantasticFiasco/serilog-sinks-http/issues/262) [BREAKING CHANGE] A new argument of type `CancellationToken` has been added to the `PostAsync` method of interface `IHttpClient`.

### :skull: Removed

- [#253](https://github.com/FantasticFiasco/serilog-sinks-http/issues/253) [BREAKING CHANGE] Remove support for .NET Framework 4.5, aligning with the [.NET Framework support policy](https://learn.microsoft.com/lifecycle/products/microsoft-net-framework)
- [BREAKING CHANGE] Remove support for .NET Framework 4.6.1, aligning with the [.NET Framework support policy](https://learn.microsoft.com/lifecycle/products/microsoft-net-framework)

## [9.0.0-beta.2] - 2024-03-27

### :zap: Added

- Support for .NET Framework 4.6.2
- [#262](https://github.com/FantasticFiasco/serilog-sinks-http/issues/262) [BREAKING CHANGE] Support specifying `flushOnClose` when creating the sink, thus adding the support to suppress sending unsent log events to the log server before closing the sink (proposed by [@murlidakhare](https://github.com/murlidakhare), [@julichan](https://github.com/julichan), [@janichirag11](https://github.com/janichirag11) & [@prasadpaul53](https://github.com/prasadpaul53))

  **Migration guide**

  The parameter `flushOnClose` has been introduced to the methods `Http`, `DurableHttpUsingFileSizeRolledBuffers` and `DurableHttpUsingTimeRolledBuffers`. Please verify that the arguments pass by you to these methods still align with your intentions.

  To automatically mitigate this kind of *new parameter issue* in the future, move from using positional arguments to named arguments.

### :dizzy: Changed

- [#262](https://github.com/FantasticFiasco/serilog-sinks-http/issues/262) [BREAKING CHANGE] A new argument of type `CancellationToken` has been added to the `PostAsync` method of interface `IHttpClient`.

### :skull: Removed

- [#253](https://github.com/FantasticFiasco/serilog-sinks-http/issues/253) [BREAKING CHANGE] Remove support for .NET Framework 4.5, aligning with the [.NET Framework support policy](https://learn.microsoft.com/lifecycle/products/microsoft-net-framework)
- [BREAKING CHANGE] Remove support for .NET Framework 4.6.1, aligning with the [.NET Framework support policy](https://learn.microsoft.com/lifecycle/products/microsoft-net-framework)

## [9.0.0-beta.1] - 2023-04-12

### :zap: Added

- [#311](https://github.com/FantasticFiasco/serilog-sinks-http/issues/311) [BREAKING CHANGE] Support specifying `levelSwitch` when creating the sink, thus adding the support to [dynamically change the log level at runtime](https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/) (contribution by [@yuriy-millen](https://github.com/yuriy-millen))

  **Migration guide**

  The parameter `levelSwitch` has been introduced to the methods `Http`, `DurableHttpUsingFileSizeRolledBuffers` and `DurableHttpUsingTimeRolledBuffers`. Please verify that the arguments pass by you to these methods still align with your intentions.

  To automatically mitigate this kind of *new parameter issue* in the future, move from using positional arguments to named arguments.

## [8.0.0] - 2022-04-10

### :zap: Added

- [#116](https://github.com/FantasticFiasco/serilog-sinks-http/issues/116) [BREAKING CHANGE] Support specifying `batchSizeLimitBytes` when creating the sink, thus limiting the size of the payloads sent to the log server (proposed by [@michaeltdaniels](https://github.com/michaeltdaniels))

  **Migration guide**

  The parameter `batchSizeLimitBytes` has been introduced to the methods `Http`, `DurableHttpUsingFileSizeRolledBuffers` and `DurableHttpUsingTimeRolledBuffers`. Please verify that the arguments pass by you to these methods still align with your intentions.

  To automatically mitigate this kind of *new parameter issue* in the future, move from using positional arguments to named arguments.

- [#166](https://github.com/FantasticFiasco/serilog-sinks-http/issues/166) Support for content encoding [Gzip](https://en.wikipedia.org/wiki/Gzip) using HTTP client `JsonGzipHttpClient` (contribution by [@vaibhavepatel](https://github.com/vaibhavepatel), [@KalininAndreyVictorovich](https://github.com/KalininAndreyVictorovich) and [@AntonSmolkov](https://github.com/AntonSmolkov))
- [#166](https://github.com/FantasticFiasco/serilog-sinks-http/issues/166) Support for specifying `HttpClient` when creating `JsonHttpClient` and `JsonGzipHttpClient`

### :dizzy: Changed

- [#166](https://github.com/FantasticFiasco/serilog-sinks-http/issues/166) [BREAKING CHANGE] Interface `IHttpClient` has changed to accommodate for different HTTP content types

  **Migration guide**

  You'll have to migrate your code if you've implemented your own version of `IHttpClient`. The signature of method `IHttpClient.PostAsync` has changed from `Task<HttpResponseMessage> PostAsync(string, HttpContent)` to `Task<HttpResponseMessage> PostAsync(string, Stream)`.

  ```csharp
  // Before migration
  public class MyHttpClient : IHttpClient
  {
    // Code removed for brevity...

    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
    {
      // Here you probably have some code updating the content,
      // and then you send the request
      return await httpClient.PostAsync(requestUri, content)
    }
  }

  // After migration
  public class MyHttpClient : IHttpClient
  {
    // Code removed for brevity...

    public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
    {
      using (var content = new StreamContent(contentStream))
      {
        content.Headers.Add("Content-Type", "application/json");

        // Here you probably have some code updating the content,
        // and then you send the request
        return await httpClient.PostAsync(requestUri, content)
      }
    }
  }
  ```

- [#162](https://github.com/FantasticFiasco/serilog-sinks-http/issues/162) [BREAKING CHANGE] Deprecated dependency [Serilog.Sinks.RollingFile](https://www.nuget.org/packages/serilog.sinks.rollingfile) has been removed (discovered by [@tipasergio](https://github.com/tipasergio))

  **Migration guide**

  You'll have to migrate your code if you're using `DurableHttpUsingTimeRolledBuffers`, i.e. use the durable HTTP sink with a rolling behavior defined by a time interval. The parameter `bufferPathFormat` has been renamed to `bufferBaseFileName`, and the parameter `bufferRollingInterval` has been added.

  Given you are configuring the sink in code you should apply the following changes.

  ```csharp
  // Before migration
  log = new LoggerConfiguration()
    .WriteTo.DurableHttpUsingTimeRolledBuffers(
      requestUri: "https://www.mylogs.com",
      bufferPathFormat: "MyBuffer-{Hour}.json")
    .CreateLogger();

  // After migration
  log = new LoggerConfiguration()
    .WriteTo.DurableHttpUsingTimeRolledBuffers(
      requestUri: "https://www.mylogs.com",
      bufferBaseFileName: "MyBuffer",
      bufferRollingInterval: BufferRollingInterval.Hour)
    .CreateLogger();
  ```

  Given you are configuring the sink in application configuration you should apply the following changes.

  ```json
  // Before migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "DurableHttpUsingTimeRolledBuffers",
          "Args": {
            "requestUri": "https://www.mylogs.com",
            "bufferPathFormat": "MyBuffer-{Hour}.json"
          }
        }
      ]
    }
  }

  // After migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "DurableHttpUsingTimeRolledBuffers",
          "Args": {
            "requestUri": "https://www.mylogs.com",
            "bufferBaseFileName": "MyBuffer",
            "bufferRollingInterval": "Hour"
          }
        }
      ]
    }
  }
  ```

- [#206](https://github.com/FantasticFiasco/serilog-sinks-http/issues/206) [BREAKING CHANGE] Argument `bufferFileSizeLimitBytes` to extension methods `DurableHttpUsingFileSizeRolledBuffers` and `DurableHttpUsingTimeRolledBuffers` no longer accepts `0` as value
- [#203](https://github.com/FantasticFiasco/serilog-sinks-http/issues/203), [#245](https://github.com/FantasticFiasco/serilog-sinks-http/issues/245) [BREAKING CHANGE] Non-durable sink has changed from having its maximum queue size defined as number of events into number of bytes, making it far easier to reason about memory consumption. It's importance to the behavior of the sink was also the reasoning for promoting it from being optional to being mandatory. (proposed by [@seruminar](https://github.com/seruminar))

  Given you are limiting the queue in code you should do the following changes.

  ```csharp
  // Before migration
  log = new LoggerConfiguration()
    .WriteTo.Http(
      requestUri: "https://www.mylogs.com",
      queueLimit: 1000)
    .CreateLogger();

  // After migration
  log = new LoggerConfiguration()
    .WriteTo.Http(
      requestUri: "https://www.mylogs.com",
      queueLimitBytes: 50 * ByteSize.MB)
    .CreateLogger();
  ```

  Given you are limiting the queue in application configuration you should do the following changes.

  ```json
  // Before migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Http",
          "Args": {
            "requestUri": "https://www.mylogs.com",
            "queueLimit": 1000
          }
        }
      ]
    }
  }

  // After migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Http",
          "Args": {
            "requestUri": "https://www.mylogs.com",
            "queueLimitBytes": 52428800
          }
        }
      ]
    }
  }
  ```

  Given you aren't limiting the queue in code you should do the following changes.

  ```csharp
  // Before migration
  log = new LoggerConfiguration()
    .WriteTo.Http(requestUri: "https://www.mylogs.com")
    .CreateLogger();

  // After migration
  log = new LoggerConfiguration()
    .WriteTo.Http(
      requestUri: "https://www.mylogs.com",
      queueLimitBytes: null)
    .CreateLogger();
  ```

  Given you aren't limiting the queue in application configuration you should do the following changes.

  ```json
  // Before migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Http",
          "Args": {
            "requestUri": "https://www.mylogs.com"
          }
        }
      ]
    }
  }

  // After migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Http",
          "Args": {
            "requestUri": "https://www.mylogs.com",
            "queueLimitBytes": null
          }
        }
      ]
    }
  }
  ```

- [#171](https://github.com/FantasticFiasco/serilog-sinks-http/issues/171) [BREAKING CHANGE] Move maximum log event size configuration from batch formatter to sink configuration, and change it's default value from 256 kB to `null`

  **Migration guide**

  Given that you're depending on the default maximum log event size configuration in `DefaultBatchFormatter` or `ArrayBatchFormatter`, or have defined your own limit when instantiating these classes, you should apply the following changes.

  ```csharp
  // Before migration
  log = new LoggerConfiguration()
    // Changes are also applicable to Http and DurableHttpUsingTimeRolledBuffers
    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
      requestUri: "https://www.mylogs.com",
      batchFormatter: new ArrayBatchFormatter(ByteSize.MB))
    .CreateLogger();

  // After migration
  log = new LoggerConfiguration()
    .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
      requestUri: "https://www.mylogs.com",
      logEventLimitBytes: ByteSize.MB,
      batchFormatter: new ArrayBatchFormatter())
    .CreateLogger();
  ```

- [#171](https://github.com/FantasticFiasco/serilog-sinks-http/issues/171) [BREAKING CHANGE] Rename sink configuration argument `batchPostingLimit` to `logEventsInBatchLimit`

  **Migration guide**

  Given tha you're defining the maximum number of log events in a single batch, you should apply the following changes.

  ```csharp
  // Before migration
  log = new LoggerConfiguration()
    // Changes are also applicable to DurableHttpUsingFileSizeRolledBuffers
    // and DurableHttpUsingTimeRolledBuffers
    .WriteTo.Http(
      requestUri: "https://www.mylogs.com",
      batchPostingLimit: 100,
    .CreateLogger();

  // After migration
  log = new LoggerConfiguration()
    .WriteTo.Http(
      requestUri: "https://www.mylogs.com",
      logEventsInBatchLimit: 100,
    .CreateLogger();
  ```

  Given you are configuring the sink in application configuration you should apply the following changes.

  ```json
  // Before migration
  // Changes are also applicable to DurableHttpUsingFileSizeRolledBuffers
  // and DurableHttpUsingTimeRolledBuffers
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Http",
          "Args": {
            "requestUri": "https://www.mylogs.com",
            "batchPostingLimit": 100
          }
        }
      ]
    }
  }

  // After migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "Http",
          "Args": {
            "requestUri": "https://www.mylogs.com",
            "logEventsInBatchLimit": 100
          }
        }
      ]
    }
  }
  ```

- [BREAKING CHANGE] Method `IHttpClient.Configure` on a custom HTTP client implementation is no longer called unless sink is provided an instance of `IConfiguration`

### :skull: Removed

- [#182](https://github.com/FantasticFiasco/serilog-sinks-http/issues/182) [BREAKING CHANGE] Extension method `DurableHttp` which was marked as deprecated in v5.2.0

  **Migration guide**

  Given you are configuring the sink in code you should apply the following changes.

  ```csharp
  // Before migration
  log = new LoggerConfiguration()
    .WriteTo.DurableHttp(requestUri: "https://www.mylogs.com")
    .CreateLogger();

  // After migration
  log = new LoggerConfiguration()
    .WriteTo.DurableHttpUsingTimeRolledBuffers(requestUri: "https://www.mylogs.com")
    .CreateLogger();
  ```

  Given you are configuring the sink in application configuration you should apply the following changes.

  ```json
  // Before migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "DurableHttp",
          "Args": {
            "requestUri": "https://www.mylogs.com"
          }
        }
      ]
    }
  }

  // After migration
  {
    "Serilog": {
      "WriteTo": [
        {
          "Name": "DurableHttpUsingTimeRolledBuffers",
          "Args": {
            "requestUri": "https://www.mylogs.com"
          }
        }
      ]
    }
  }
  ```

- [#215](https://github.com/FantasticFiasco/serilog-sinks-http/issues/215) [BREAKING CHANGE] Remove support for .NET Standard 1.3, aligning with the [cross-platform targeting library guidance](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting)
- [#196](https://github.com/FantasticFiasco/serilog-sinks-http/issues/196) [BREAKING CHANGE] Overloaded method `IBatchFormatter.Format(IEnumerable<LogEvent>, ITextFormatter, TextWriter)` has been removed in favour of keeping `IBatchFormatter.Format(IEnumerable<string>, TextWriter output)`

  **Migration guide**

  You'll have to migrate your code if you've implemented your own version of `IBatchFormatter`.

  ```csharp
  // Before migration
  public class MyBatchFormatter : IBatchFormatter
  {
    public void Format(IEnumerable<LogEvent> logEvents, ITextFormatter formatter, TextWriter output)
    {
      // Your implementation accepting a sequence of log events
    }

    public void Format(IEnumerable<string> logEvents, TextWriter output)
    {
      // Your implementation accepting a sequence of serialized log events
    }
  }

  // After migration
  public class MyBatchFormatter : IBatchFormatter
  {
    public void Format(IEnumerable<string> logEvents, TextWriter output)
    {
      // Your implementation accepting a sequence of serialized log events
    }
  }
  ```

- [#178](https://github.com/FantasticFiasco/serilog-sinks-http/issues/178) [BREAKING CHANGE] Batch formatter `DefaultBatchFormatter` was removed when `ArrayBatchFormatter` was promoted to default batch formatter.

  **Migration guide**

  You'll have to migrate your code if you used `DefaultBatchFormatter`, either implicitly by not specifying a batch formatter, or explicitly by specifying `DefaultBatchFormatter` when configuring the sink.

  Given that you wish to continue using `DefaultBatchFormatter` as your batch formatter you should copy its implementation from [the wiki](https://github.com/FantasticFiasco/serilog-sinks-http/wiki/Batch-formatters) into your own codebase.

  Given that you decide to migrate into using `ArrayBatchFormatter` instead of `DefaultBatchFormatter`, you should verify that your log server is capable of receiving the new JSON payload format.

### :syringe: Fixed

- Durable buffer files are no longer created with an initial [BOM](https://en.wikipedia.org/wiki/Byte_order_mark)
- [#169](https://github.com/FantasticFiasco/serilog-sinks-http/issues/169) Rename buffer files to use the file extension `.txt` instead of `.json`
- [#208](https://github.com/FantasticFiasco/serilog-sinks-http/issues/208) Transient dependency conflict for package [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration) on ASP.NET Core 2.x (contribution by [@AntonSmolkov](https://github.com/AntonSmolkov))
- [#220](https://github.com/FantasticFiasco/serilog-sinks-http/issues/220) - Text formatters `NamespacedTextFormatter`, `NormalRenderedTextFormatter` and `NormalTextFormatter` should write the timestamp in UTC

## [7.2.0] - 2020-10-19

### :zap: Added

- Support for .NET Standard 2.1 (contributed by [@augustoproiete](https://github.com/augustoproiete))

## [7.1.0] - 2020-10-18

### :zap: Added

- Add class `ByteSize` which helps specifying multipliers of the unit *byte*.

### :syringe: Fixed

- [#135](https://github.com/FantasticFiasco/serilog-sinks-http/issues/135) The type `IConfiguration` exists in both `Microsoft.Extensions.Configuration.Abstractions` and `Serilog.Sinks.Http` (discovered by [@brian-pickens-web](https://github.com/brian-pickens-web) and contributed by [@aleksaradz](https://github.com/aleksaradz))

## [7.0.1] - 2020-08-14

### :syringe: Fixed

- [#127](https://github.com/FantasticFiasco/serilog-sinks-http/issues/127) NuGet package does not show an icon
- Configure [SourceLink](https://github.com/dotnet/sourcelink) to embed untracked sources
- Configure [SourceLink](https://github.com/dotnet/sourcelink) to use deterministic builds when running on AppVeyor

## [7.0.0] - 2020-08-12

### :zap: Added

- [#49](https://github.com/FantasticFiasco/serilog-sinks-http/issues/49), [#123](https://github.com/FantasticFiasco/serilog-sinks-http/issues/123) [BREAKING CHANGE] Improve support for configuring HTTP client when using [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration). See [Custom HTTP client in wiki](https://github.com/FantasticFiasco/serilog-sinks-http/wiki/Custom-HTTP-client) for more information. (discovered by [@brunorsantos](https://github.com/brunorsantos))

## [6.0.0] - 2020-05-13

### :zap: Added

- [#95](https://github.com/FantasticFiasco/serilog-sinks-http/issues/95) [BREAKING CHANGE] Support to specify `bufferFileShared` when creating a durable sink, thus allowing the buffer file to be shared by multiple processes (discovered by [@esakkiraja-k](https://github.com/esakkiraja-k))

## [5.2.1] - 2020-02-16

### :syringe: Fixed

- [#97](https://github.com/FantasticFiasco/serilog-sinks-http/issues/97) Make sure the sink respects the exponential backoff, even after numerous unsuccessful attempts to send the log events to a log server (discovered by [@markusbrueckner](https://github.com/markusbrueckner))

## [5.2.0] - 2019-04-27

### :zap: Added

- Extension method `DurableHttpUsingFileSizeRolledBuffers`, creating a durable sink using buffer files with a file size based rolling behavior

### :zzz: Deprecated

- Extension method `DurableHttp` has been renamed to `DurableHttpUsingTimeRolledBuffers`, providing clarification between the two durable sink types

## [5.1.0] - 2019-01-07

### :zap: Added

- Support for .NET Framework 4.6.1 due to recommendations from the [cross-platform targeting guidelines](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting#multi-targeting)
- Support for .NET Standard 2.0 due to recommendations from the [cross-platform targeting guidelines](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting#net-standard)

## [5.0.1] - 2018-09-24

### :syringe: Fixed

- [#54](https://github.com/FantasticFiasco/serilog-sinks-http/issues/54) Prevent durable HTTP sink from posting partially written log events

## [5.0.0] - 2018-08-30

### :zap: Added

- [#51](https://github.com/FantasticFiasco/serilog-sinks-http/issues/51) [BREAKING CHANGE] Support to specify `queueLimit` when creating a non-durable sink, limiting the number of events queued in memory waiting to be posted over the network.

## [4.3.0] - 2018-02-01

### :zap: Added

- Event formatter called `NamespacedTextFormatter` suited for a micro-service architecture where log events are sent to the Elastic Stack. The event formatter reduces the risk of two services logging properties with identical names but with different types, which the Elastic Stack doesn't support.

### :skull: Removed

- Support for .NET Standard 2.0 since the sink also has support for .NET Standard 1.3

## [4.2.1] - 2017-10-11

### :syringe: Fixed

- [#32](https://github.com/FantasticFiasco/serilog-sinks-http/issues/32) Prevent durable HTTP sink from posting HTTP messages without any log events

## [4.2.0] - 2017-08-20

### :zap: Added

- Support for .NET Core 2.0

## [4.1.0] - 2017-08-13

### :zap: Added

- [#22](https://github.com/FantasticFiasco/serilog-sinks-http/issues/22) Batch formatter `ArrayBatchFormatter` which is compatible with the Logstash HTTP input plugin configured to use the JSON codec

### :syringe: Fixed

- Prevent posting HTTP messages without any log events

## [4.0.0] - 2017-06-17

### :zap: Added

- [#8](https://github.com/FantasticFiasco/serilog-sinks-http/issues/8) [BREAKING CHANGE] Support for [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) required changing the extension methods configuring a HTTP sink. `Options` and `DurableOptions` no longer exist, and their properties are now optional parameters on the extension methods instead.
- [#11](https://github.com/FantasticFiasco/serilog-sinks-http/issues/11) Support for HTTP body configuration using `IBatchFormatter` and `ITextFormatter`. This enables full control of how messages are serialized before being sent over the network. (contribution by [@kvpt](https://github.com/kvpt))
- [#19](https://github.com/FantasticFiasco/serilog-sinks-http/issues/19) Support for specifying the maximum number of retained buffer files and their rotation period on the durable HTTP sink. (contribution by [@rob-somerville](https://github.com/rob-somerville))

### :dizzy: Changed

- [#11](https://github.com/FantasticFiasco/serilog-sinks-http/issues/11) Enum `FormattingType` has been replaces with public access to the formatters `NormalRenderedTextFormatter`, `NormalTextFormatter`, `CompactRenderedTextFormatter` and `CompactTextFormatter`. Removing the enum opens up the possibility to specify your own text formatter.  (contribution by [@kvpt](https://github.com/kvpt))

## [3.1.1] - 2017-04-24

### :syringe: Fixed

- Package project URL

## [3.1.0] - 2017-03-12

### :zap: Added

- Support for the formatting types: `FormattingType.NormalRendered`, `FormattingType.Normal`, `FormattingType.CompactRendered` and `FormattingType.Compact`. The formatting type can be configured via `Options` and `DurableOptions`.

## [3.0.0] - 2017-03-04

### :zap: Added

- [#3](https://github.com/FantasticFiasco/serilog-sinks-http/issues/3) Support for configuring a sink to be durable using `Http(string, DurableOptions)`. A durable sink will persist log events on disk before sending them over the network, thus protecting against data loss after a system or process restart.

### :dizzy: Changed

- [BREAKING CHANGE] The syntax for creating a non-durable sink has been changed from `Http(string)` to `Http(string, Options)` to accommodate for the syntax to create a durable sink. A non-durable sink will lose data after a system or process restart.
- Improve compatibility by supporting .NET Standard 1.3

## [2.0.0] - 2016-11-23

### :dizzy: Changed

- [#1](https://github.com/FantasticFiasco/serilog-sinks-http/pull/1) [BREAKING CHANGE] Sinks can be configured to use a custom implementation of `IHttpClient` (contribution by [@lhaussknecht](https://github.com/lhaussknecht))

## [1.0.0] - 2016-11-03

Initial version.
