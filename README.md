# Serilog.Sinks.Http

[![Build status](https://ci.appveyor.com/api/projects/status/ayvak8yo23k962sg?svg=true)](https://ci.appveyor.com/project/FantasticFiasco/serilog-sinks-http) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Http.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Http/) [![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/serilog/serilog/wiki) [![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog) [![Help](https://img.shields.io/badge/stackoverflow-serilog-orange.svg)](http://stackoverflow.com/questions/tagged/serilog)

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

### Install via NuGet

If you want to include the HTTP POST sink in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Serilog.Sinks.Http/).

To install the sink, run the following command in the Package Manager Console:

```
PM> Install-Package Serilog.Sinks.Http
```