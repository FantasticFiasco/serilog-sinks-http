# Serilog.Sinks.Http - A Serilog sink sending log events over HTTP

[![Build status](https://ci.appveyor.com/api/projects/status/ayvak8yo23k962sg/branch/master?svg=true)](https://ci.appveyor.com/project/FantasticFiasco/serilog-sinks-http)
[![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Http.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Http/) 
[![NuGet](https://img.shields.io/nuget/dt/Serilog.Sinks.Http.svg)](https://www.nuget.org/packages/Serilog.Sinks.Http/)
[![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/serilog/serilog/wiki)
[![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog)
[![Help](https://img.shields.io/badge/stackoverflow-serilog-orange.svg)](http://stackoverflow.com/questions/tagged/serilog)

**Package** - [Serilog.Sinks.Http](https://www.nuget.org/packages/serilog.sinks.http)
| **Platforms** - .NET 4.5, .NET Standard 1.3

## Table of contents

- [Super simple to use](#super-simple-to-use)
- [Typical use case](#typical-use-case)
- [HTTP sink](#http-sink)
- [Durable HTTP sink](#durable-http-sink)
- [Formatters](#formatters)
- [Install via NuGet](#install-via-nuget)
- [Credit](#credit)

---

## Super simple to use

In the following example, the sink will POST log events to `www.mylogs.com` over HTTP.

```csharp
ILogger log = new LoggerConfiguration()
  .MinimumLevel.Verbose()
  .WriteTo.Http("www.mylogs.com")
  .CreateLogger();

log.Information("Logging {@Heartbeat} from {Computer}", heartbeat, computer);
```

Used in conjunction with [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) the same sink can be configured in the following way:

```json
{
  "Serilog": {
    "MinimumLevel": "Verbose",
    "WriteTo": [
      {
        "Name": "Http",
        "Args": {
          "requestUri": "www.mylogs.com"
        } 
      }
    ]
  }
}
```

The sink is batching multiple log events into a single request, and the following hypothetical payload is sent over the network as JSON.

```json
{
  "events": [
    {
      "Timestamp": "2016-11-03T00:09:11.4899425+01:00",
      "Level": "Information",
      "MessageTemplate": "Logging {@Heartbeat} from {Computer}",
      "RenderedMessage": "Logging { UserName: \"Mike\", UserDomainName: \"Home\" } from \"Workstation\"",
      "Properties": {
        "Heartbeat": {
          "UserName": "Mike",
          "UserDomainName": "Home"
        },
        "Computer": "Workstation"
      }
    },
    {
      "Timestamp": "2016-11-03T00:09:12.4905685+01:00",
      "Level": "Information",
      "MessageTemplate": "Logging {@Heartbeat} from {Computer}",
      "RenderedMessage": "Logging { UserName: \"Mike\", UserDomainName: \"Home\" } from \"Workstation\"",
      "Properties": {
        "Heartbeat": {
          "UserName": "Mike",
          "UserDomainName": "Home"
        },
        "Computer": "Workstation"
      }
    }
  ]
}
```

## Typical use case

Producing log events is only half the story. Unless you are consuming them in a matter that benefits you in development or production, there is really no need to produce them in the first place.

Integration with [Elastic Stack](https://www.elastic.co/products) (formerly know as ELK, an acronym for Elasticsearch, Logstash and Kibana) is powerful beyond belief, but there are many alternatives to get the log events into Elasticsearch.

### Send log events to Elasticsearch

The log events can be sent directly to Elasticsearch using [Serilog.Sinks.Elasticsearch](https://github.com/serilog/serilog-sinks-elasticsearch). In this case you've solved your problem without using this sink, and all is well in the world.

### Send log events to Logstash

If you would like to send the log events to Logstash for further processing instead of sending them directly to Elasticsearch, this sink in combination with the [Logstash HTTP input plugin](https://www.elastic.co/blog/introducing-logstash-input-http-plugin) is the perfect match for you. It is a much better solution than having to install [Filebeat](https://www.elastic.co/products/beats/filebeat) on all your instances, mainly because it involves fewer moving parts.

## HTTP sink

The non-durable HTTP sink will send log events using HTTP POST over the network. A non-durable sink will lose data after a system or process restart.

### Arguments

The following arguments are available when creating a HTTP sink.

- `requestUri` - The URI the request is sent to.
- `batchPostingLimit` - The maximum number of events to post in a single batch. Default value is 1000.
- `period` - The time to wait between checking for event batches. Default value is 2 seconds.
- `textFormatter` - The formatter rendering individual log events into text, for example JSON. Default value is `NormalRenderedTextFormatter`.
- `batchFormatter` - The formatter batching multiple log events into a payload that can be sent over the network. Default value is `DefaultBatchFormatter`.
- `restrictedToMinimumLevel` - The minimum level for events passed through the sink. Default value is `LevelAlias.Minimum`.
- `httpClient` - A custom `IHttpClient` implementation. Default value is `HttpClient`.

## Durable HTTP sink

The durable HTTP sink will send log events using HTTP POST over the network. A durable sink will persist log events on disk before sending them, thus protecting against data loss after a system or process restart.

### Arguments

The following arguments are available when creating a durable HTTP sink.

- `requestUri` - The URI the request is sent to.
- `bufferPathFormat` - The path format for a set of files that will be used to buffer events until they can be successfully sent over the network. Default value is `"Buffer-{Date}.json"`. To use file rotation that is on an 30 or 60 minute interval pass `"Buffer-{Hour}.json"` or `"Buffer-{HalfHour}.json"`.
- `bufferFileSizeLimitBytes` - The maximum size, in bytes, to which the buffer log file for a specific date will be allowed to grow. By default no limit will be applied.
- `retainedBufferFileCountLimit` - The maximum number of buffer files that will be retained, including the current buffer file. Under normal operation only 2 files will be kept, however if the log server is unreachable, the number of files specified by `retainedBufferFileCountLimit` will be kept on the file system. For unlimited retention, pass `null`. Default value is 31.
- `batchPostingLimit` - The maximum number of events to post in a single batch. Default value is 1000.
- `period` - The time to wait between checking for event batches. Default value is 2 seconds.
- `textFormatter` - The formatter rendering individual log events into text, for example JSON. Default value is `NormalRenderedTextFormatter`.
- `batchFormatter` - The formatter batching multiple log events into a payload that can be sent over the network. Default value is `DefaultBatchFormatter`.
- `restrictedToMinimumLevel` - The minimum level for events passed through the sink. Default value is `LevelAlias.Minimum`.
- `httpClient` - A custom `IHttpClient` implementation. Default value is `HttpClient`.

## Formatters

Formatters build a pipeline that takes a sequence of log events and turn them into something that can be sent over the network. There are two types of formatters, the _event formatter_ and the _batch formatter_.

The event formatter has the responsibility of turning a single log event into a textual representation. It can serialize the log event into JSON, XML or anything else that matches the expectations of the receiving log server.

The batch formatter has the responsibility of batching a sequence of log events into a single payload that can be sent over the network. It does not care about individual log event formatting but instead focuses on how these events are serialized together into a single HTTP request. 

### Event formatters

The sink comes pre-loaded with four event formatters where `NormalRenderedTextFormatter` is the default. You can decide to configure your sink to use any of these four, or write your own by implementing `ITextFormatter`.

```csharp
/// <summary>
/// Formats log events in a textual representation.
/// </summary>
public interface ITextFormatter
{
  /// <summary>
  /// Format the log event into the output.
  /// </summary>
  /// <param name="logEvent">
  /// The event to format.
  /// </param>
  /// <param name="output">
  /// The output.
  /// </param>
  void Format(LogEvent logEvent, TextWriter output);
}
```

#### Formatter 1 - `NormalRenderedTextFormatter`

The log event is normally formatted and the message template is rendered into a message. This is the most verbose formatting type and its network load is higher than the other options.

```json
{
  "Timestamp": "2016-11-03T00:09:11.4899425+01:00",
  "Level": "Information",
  "MessageTemplate": "Logging {@Heartbeat} from {Computer}",
  "RenderedMessage": "Logging { UserName: \"Mike\", UserDomainName: \"Home\" } from \"Workstation\"",
  "Properties": {
    "Heartbeat": {
      "UserName": "Mike",
      "UserDomainName": "Home"
    },
    "Computer": "Workstation"
  }
}
```

#### Formatter 2 - `NormalTextFormatter`

The log event is normally formatted and its data normalized. The lack of a rendered message means improved network load compared to `NormalRenderedTextFormatter`. Often this formatting type is complemented with a log server that is capable of rendering the messages of the incoming log events.

```json
{
  "Timestamp": "2016-11-03T00:09:11.4899425+01:00",
  "Level": "Information",
  "MessageTemplate": "Logging {@Heartbeat} from {Computer}",
  "Properties": {
    "Heartbeat": {
      "UserName": "Mike",
      "UserDomainName": "Home"
    },
    "Computer": "Workstation"
  }
}
```

#### Formatter 3 - `CompactRenderedTextFormatter`

The log event is formatted with minimizing size as a priority but still render the message template into a message. This formatting type reduces the network load and should be used in situations where bandwidth is of importance.

The compact formatter adheres to the following rules:

- Built-in field names are short and prefixed with an `@`
- The `Properties` property is flattened
- The Information level is omitted since it is considered to be the default

```json
{
  "@t": "2016-11-03T00:09:11.4899425+01:00",
  "@mt": "Logging {@Heartbeat} from {Computer}",
  "@m":"Logging { UserName: \"Mike\", UserDomainName: \"Home\" } from \"Workstation\"",
  "Heartbeat": {
    "UserName": "Mike",
    "UserDomainName": "Home"
  },
  "Computer": "Workstation"
}
```

#### Formatter 4 - `CompactTextFormatter`

The log event is formatted with minimizing size as a priority and its data is normalized. The lack of a rendered message means even smaller network load compared to `CompactRenderedTextFormatter` and should be used in situations where bandwidth is of importance. Often this formatting type is complemented with a log server that is capable of rendering the messages of the incoming log events.

The compact formatter adheres to the following rules:

- Built-in field names are short and prefixed with an `@`
- The `Properties` property is flattened
- The Information level is omitted since it is considered to be the default

```json
{
  "@t": "2016-11-03T00:09:11.4899425+01:00",
  "@mt": "Logging {@Heartbeat} from {Computer}",
  "Heartbeat": {
    "UserName": "Mike",
    "UserDomainName": "Home"
  },
  "Computer": "Workstation"
}
```

### Batch formatters

The sink comes pre-loaded with a batch formatter called `DefaultBatchFormatter`. It creates a JSON object with a property called `events` that holds the log events sent over the network.

Example:
```json
{
  "events": [
    { "Message": "Event n" },
    { "Message": "Event n+1" }
  ]
}
```

You can decide to use this batch formatter, or write your own by implementing `IBatchFormatter`.

```csharp
/// <summary>
/// Formats batches of log events into payloads that can be sent over the network.
/// </summary>
public interface IBatchFormatter
{
  /// <summary>
  /// Format the log events into a payload.
  /// </summary>
  /// <param name="logEvents">
  /// The events to format.
  /// </param>
  /// <param name="formatter">
  /// The formatter turning the log events into a textual representation.
  /// </param>
  /// <param name="output">
  /// The payload to send over the network.
  /// </param>
  void Format(IEnumerable<LogEvent> logEvents, ITextFormatter formatter, TextWriter output);

  /// <summary>
  /// Format the log events into a payload.
  /// </summary>
  /// <param name="logEvents">
  /// The events to format.
  /// </param>
  /// <param name="output">
  /// The payload to send over the network.
  /// </param>
  void Format(IEnumerable<string> logEvents, TextWriter output);
}
```

## Install via NuGet

If you want to include the HTTP sink in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Serilog.Sinks.Http/).

To install the sink, run the following command in the Package Manager Console:

```
PM> Install-Package Serilog.Sinks.Http
```

## Credit

Thank you [JetBrains](https://www.jetbrains.com/) for your important initiative to support the open source community with free licenses to your products.

![JetBrains](./design/jetbrains.png)
