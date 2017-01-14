# Serilog.Sinks.Http

[![Build status](https://ci.appveyor.com/api/projects/status/ayvak8yo23k962sg/branch/master?svg=true)](https://ci.appveyor.com/project/FantasticFiasco/serilog-sinks-http) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Http.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Http/) [![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/serilog/serilog/wiki) [![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog) [![Help](https://img.shields.io/badge/stackoverflow-serilog-orange.svg)](http://stackoverflow.com/questions/tagged/serilog)

A [Serilog](http://serilog.net/) sink that sends HTTP POST requests over the network.

**Package** - [Serilog.Sinks.Http](https://www.nuget.org/packages/serilog.sinks.http)
| **Platforms** - .NET 4.5, .NETStandard 1.5

### Getting started

In the example shown, the sink will send a HTTP POST request to URI `www.mylogs.com`.

```csharp
Serilog.ILogger log = new LoggerConfiguration()
  .MinimumLevel.Verbose()
  .WriteTo.Http("www.mylogs.com")
  .CreateLogger();
```

The sink is batching multiple events into a single request, and the following hypothetical payload is sent as JSON.

```json
{
  "events": [
    {
      "Timestamp": "2016-11-03T00:09:11.4899425+01:00",
      "Level": "Debug",
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
      "Level": "Debug",
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

### Typical use case

Producing log events is only half the story. Unless you are consuming them in a matter that benefits you in development or operations, there is really no need to produce them in the first place.

Integration with [Elastic Stack](https://www.elastic.co/products) (formerly know as ELK, an acronym for Elasticsearch, Logstash and Kibana) is powerful beyond belief, but there are many alternatives to get the log events into Elasticsearch.

#### Send log events to Elasticsearch

The log events can be sent directly to Elasticsearch using [Serilog.Sinks.Elasticsearch](https://github.com/serilog/serilog-sinks-elasticsearch). In this case you've solved your problem without using this sink, and all is well in the world.

#### Send log events to Logstash

If you would like to send the log events to Logstash for further processing instead of sending them directly to Elasticsearch, this sink in combination with the [Logstash HTTP input plugin](https://www.elastic.co/blog/introducing-logstash-input-http-plugin) is the perfect match for you. It is a much better solution than having to install [Filebeat](https://www.elastic.co/products/beats/filebeat) on all your instances, mainly because it involves fewer moving parts.

### Install via NuGet

If you want to include the HTTP POST sink in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Serilog.Sinks.Http/).

To install the sink, run the following command in the Package Manager Console:

```
PM> Install-Package Serilog.Sinks.Http
```

### Credit

Thank you [JetBrains](https://www.jetbrains.com/) for your important initiative to support the open source community with free licenses to your products.
