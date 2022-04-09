# Serilog.Sinks.Http - A Serilog sink sending log events over HTTP <!-- omit in toc -->

[![Build status](https://ci.appveyor.com/api/projects/status/ayvak8yo23k962sg/branch/master?svg=true)](https://ci.appveyor.com/project/FantasticFiasco/serilog-sinks-http)
[![codecov](https://codecov.io/gh/FantasticFiasco/serilog-sinks-http/branch/master/graph/badge.svg?token=cw6OYeQmdH)](https://codecov.io/gh/FantasticFiasco/serilog-sinks-http)
[![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Http.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Http/)
[![SemVer compatible](https://img.shields.io/badge/%E2%9C%85-SemVer%20compatible-blue)](https://semver.org/)
[![NuGet](https://img.shields.io/nuget/dt/Serilog.Sinks.Http.svg)](https://www.nuget.org/packages/Serilog.Sinks.Http/)
[![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/serilog/serilog/wiki)
[![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog)
[![Help](https://img.shields.io/badge/stackoverflow-serilog-orange.svg)](http://stackoverflow.com/questions/tagged/serilog)

**Package** - [Serilog.Sinks.Http](https://www.nuget.org/packages/serilog.sinks.http) | **Platforms** - .NET 4.5/4.6.1, .NET Standard 2.0/2.1

## Table of contents <!-- omit in toc -->

- [Introduction](#introduction)
- [Super simple to use](#super-simple-to-use)
- [Typical use cases](#typical-use-cases)
  - [Send log events from Docker containers](#send-log-events-from-docker-containers)
  - [Send log events to Elasticsearch](#send-log-events-to-elasticsearch)
  - [Send log events to Logstash](#send-log-events-to-logstash)
- [Sample applications](#sample-applications)
- [Install via NuGet](#install-via-nuget)
- [Contributors](#contributors)

---

## Introduction

This project started out with a wish to send log events to the Elastic Stack. I had prior experience of [Elastic Filebeat](https://www.elastic.co/beats/filebeat) and didn't like it. I thought the value it added was lower than the complexity it introduced.

Knowing that [Serilog.Sinks.Seq](https://github.com/serilog/serilog-sinks-seq) existed, and knowing that the code was of really good quality, I blatantly copied many of the core files into this project and started developing a general HTTP sink.

And here we are today. I hope you'll find the sink useful. If not, don't hesitate to open an issue.

## Super simple to use

In the following example, the sink will POST log events to `http://www.mylogs.com` over HTTP. We configure the sink using **[named arguments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments#named-arguments) instead of positional** because historically we've seen that most breaking changes where the result of a new parameter describing a new feature. Using named arguments means that you more often than not can migrate to new major versions without any changes to your code.

```csharp
ILogger log = new LoggerConfiguration()
  .MinimumLevel.Verbose()
  .WriteTo.Http(requestUri: "https://www.mylogs.com", queueLimitBytes: null)
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
          "requestUri": "https://www.mylogs.com",
          "queueLimitBytes": null
        }
      }
    ]
  }
}
```

The sink can also be configured to be durable, i.e. log events are persisted on disk before being sent over the network, thus protected against data loss after a system or process restart. For more information please read the [wiki](https://github.com/FantasticFiasco/serilog-sinks-http/wiki).

The sink is batching multiple log events into a single request, and the following hypothetical payload is sent over the network as JSON.

```json
[
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
```

## Typical use cases

Producing log events is only half the story. Unless you are consuming them in a matter that benefits you in development or production, there is really no need to produce them in the first place.

Integration with [Elastic Stack](https://www.elastic.co/products) (formerly know as ELK, an acronym for Elasticsearch, Logstash and Kibana) is powerful beyond belief, but there are many alternatives to get the log events into Elasticsearch.

### Send log events from Docker containers

A common solution, given your application is running in Docker containers, is to have *stdout* (standard output) and *stderr* (standard error) passed on to the Elastic Stack. There is a multitude of ways to accomplish this, but one using [Logspout](https://github.com/gliderlabs/logspout) is linked in the [Sample applications](#sample-applications) chapter.

### Send log events to Elasticsearch

The log events can be sent directly to Elasticsearch using [Serilog.Sinks.Elasticsearch](https://github.com/serilog/serilog-sinks-elasticsearch). In this case you've solved your problem without using this sink, and all is well in the world.

### Send log events to Logstash

If you would like to send the log events to Logstash for further processing instead of sending them directly to Elasticsearch, this sink in combination with the [Logstash HTTP input plugin](https://www.elastic.co/blog/introducing-logstash-input-http-plugin) is the perfect match for you. It is a much better solution than having to install [Filebeat](https://www.elastic.co/products/beats/filebeat) on all your instances, mainly because it involves fewer moving parts.

## Sample applications

The following sample applications demonstrate the usage of this sink in various contexts:

- [Serilog and the Elastic Stack](https://github.com/FantasticFiasco/serilog-sinks-http-sample-elastic-stack) - Sample application sending log events to Elastic Stack.
- [Serilog and Fluentd](https://github.com/FantasticFiasco/serilog-sinks-http-sample-fluentd) - Sample application sending log events to Fluentd.
- [Serilog.Sinks.Http - Sample in .NET Core](https://github.com/FantasticFiasco/serilog-sinks-http-sample-dotnet-core) - Sample application producing log events in .NET Core. This project also includes a sample log server and controller in .NET Core.
- [Serilog.Sinks.Http - Sample in .NET Framework](https://github.com/FantasticFiasco/serilog-sinks-http-sample-dotnet-framework) - Sample application producing log events in .NET Framework. This project also includes a sample log server and controller in .NET Framework.

The following sample application demonstrate how Serilog events from a Docker container end up in the Elastic Stack using [Logspout](https://github.com/gliderlabs/logspout), without using `Serilog.Sinks.Http`.

- [Serilog, Logspout and the Elastic Stack](https://github.com/FantasticFiasco/serilog-logspout-elastic-stack) - Sample application sending log events from a Docker container using Logspout to Elastic Stack

## Install via NuGet

If you want to include the HTTP sink in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Serilog.Sinks.Http/).

To install the sink, run the following command in the Package Manager Console:

```
PM> Install-Package Serilog.Sinks.Http
```

## Contributors

The following users have made significant contributions to this project. Thank you so much!

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://jetbrains.com/opensource"><img src="https://avatars.githubusercontent.com/u/878437?v=4?s=100" width="100px;" alt=""/><br /><sub><b>JetBrains</b></sub></a><br /><a href="#infra-JetBrains" title="Infrastructure (Hosting, Build-Tools, etc)">🚇</a></td>
    <td align="center"><a href="https://augustoproiete.net/"><img src="https://avatars.githubusercontent.com/u/177608?v=4?s=100" width="100px;" alt=""/><br /><sub><b>C. Augusto Proiete</b></sub></a><br /><a href="#financial-augustoproiete" title="Financial">💵</a> <a href="#question-augustoproiete" title="Answering Questions">💬</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=augustoproiete" title="Code">💻</a> <a href="#ideas-augustoproiete" title="Ideas, Planning, & Feedback">🤔</a></td>
    <td align="center"><a href="https://github.com/lhaussknecht"><img src="https://avatars.githubusercontent.com/u/140147?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Louis Haußknecht</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=lhaussknecht" title="Code">💻</a> <a href="#ideas-lhaussknecht" title="Ideas, Planning, & Feedback">🤔</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3Alhaussknecht" title="Bug reports">🐛</a></td>
    <td align="center"><a href="https://github.com/rob-somerville"><img src="https://avatars.githubusercontent.com/u/12766610?v=4?s=100" width="100px;" alt=""/><br /><sub><b>rob-somerville</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=rob-somerville" title="Code">💻</a> <a href="#ideas-rob-somerville" title="Ideas, Planning, & Feedback">🤔</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3Arob-somerville" title="Bug reports">🐛</a></td>
    <td align="center"><a href="https://github.com/kvpt"><img src="https://avatars.githubusercontent.com/u/1446221?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Kevin Petit</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=kvpt" title="Code">💻</a> <a href="#ideas-kvpt" title="Ideas, Planning, & Feedback">🤔</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3Akvpt" title="Bug reports">🐛</a></td>
    <td align="center"><a href="https://github.com/aleksaradz"><img src="https://avatars.githubusercontent.com/u/72725560?v=4?s=100" width="100px;" alt=""/><br /><sub><b>aleksaradz</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=aleksaradz" title="Code">💻</a> <a href="#ideas-aleksaradz" title="Ideas, Planning, & Feedback">🤔</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3Aaleksaradz" title="Bug reports">🐛</a></td>
    <td align="center"><a href="https://github.com/michaeltdaniels"><img src="https://avatars.githubusercontent.com/u/45430678?v=4?s=100" width="100px;" alt=""/><br /><sub><b>michaeltdaniels</b></sub></a><br /><a href="#ideas-michaeltdaniels" title="Ideas, Planning, & Feedback">🤔</a></td>
  </tr>
  <tr>
    <td align="center"><a href="https://github.com/dusse1dorf"><img src="https://avatars.githubusercontent.com/u/37047967?v=4?s=100" width="100px;" alt=""/><br /><sub><b>dusse1dorf</b></sub></a><br /><a href="#ideas-dusse1dorf" title="Ideas, Planning, & Feedback">🤔</a></td>
    <td align="center"><a href="https://github.com/vaibhavepatel"><img src="https://avatars.githubusercontent.com/u/23142694?v=4?s=100" width="100px;" alt=""/><br /><sub><b>vaibhavepatel</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3Avaibhavepatel" title="Bug reports">🐛</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=vaibhavepatel" title="Code">💻</a> <a href="#ideas-vaibhavepatel" title="Ideas, Planning, & Feedback">🤔</a></td>
    <td align="center"><a href="https://github.com/KalininAndreyVictorovich"><img src="https://avatars.githubusercontent.com/u/1285535?v=4?s=100" width="100px;" alt=""/><br /><sub><b>KalininAndreyVictorovich</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3AKalininAndreyVictorovich" title="Bug reports">🐛</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=KalininAndreyVictorovich" title="Code">💻</a></td>
    <td align="center"><a href="https://github.com/tipasergio"><img src="https://avatars.githubusercontent.com/u/6435956?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Sergios</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3Atipasergio" title="Bug reports">🐛</a></td>
    <td align="center"><a href="https://github.com/AntonSmolkov"><img src="https://avatars.githubusercontent.com/u/5318028?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Anton Smolkov</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=AntonSmolkov" title="Code">💻</a> <a href="https://github.com/FantasticFiasco/serilog-sinks-http/issues?q=author%3AAntonSmolkov" title="Bug reports">🐛</a></td>
    <td align="center"><a href="https://github.com/Siphonophora"><img src="https://avatars.githubusercontent.com/u/32316111?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Michael J Conrad</b></sub></a><br /><a href="https://github.com/FantasticFiasco/serilog-sinks-http/commits?author=Siphonophora" title="Documentation">📖</a></td>
    <td align="center"><a href="https://github.com/seruminar"><img src="https://avatars.githubusercontent.com/u/35008875?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Yuriy Sountsov</b></sub></a><br /><a href="#ideas-seruminar" title="Ideas, Planning, & Feedback">🤔</a></td>
  </tr>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
