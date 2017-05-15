# Change Log

All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](http://semver.org/) and is following the [change log format](http://keepachangelog.com/).

## Unreleased

### Added

- [#8](https://github.com/FantasticFiasco/serilog-sinks-http/issues/8) [BREAKING CHANGE] Support for [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) required changing the extension methods configuring a HTTP sink. `Options` and `DurableOptions` no longer exist, and their properties are now optional parameters on the extension methods instead.
- [#11](https://github.com/FantasticFiasco/serilog-sinks-http/issues/11) Support for HTTP body configuration using `IBatchFormatter` and `ITextFormatter`. This enables full control of how messages are serialized before being sent over the network. (contribution by [@kvpt](https://github.com/kvpt))

### Changed

- [#11](https://github.com/FantasticFiasco/serilog-sinks-http/issues/11) Enum `FormattingType` has been replaces with public access to the formatters `NormalRenderedTextFormatter`, `NormalTextFormatter`, `CompactRenderedTextFormatter` and `CompactTextFormatter`. Removing the enum opens up the possibility to specify your own text formatter.  (contribution by [@kvpt](https://github.com/kvpt))

## 3.1.1 2017-04-24

### Fixed

- Package project URL

## 3.1.0 2017-03-12

### Added

- Support for the formatting types: `FormattingType.NormalRendered`, `FormattingType.Normal`, `FormattingType.CompactRendered` and `FormattingType.Compact`. The formatting type can be configured via `Options` and `DurableOptions`.

## 3.0.0 2017-03-04

### Added

- [#3](https://github.com/FantasticFiasco/serilog-sinks-http/issues/3) Support for configuring a sink to be durable using `Http(string, DurableOptions)`. A durable sink will persist log events on disk before sending them over the network, thus protecting against data loss after a system or process restart.

### Changed

- [BREAKING CHANGE] The syntax for creating a non-durable sink has been changed from `Http(string)` to `Http(string, Options)` to accommodate for the syntax to create a durable sink. A non-durable sink will lose data after a system or process restart.
- Improve compatibility by supporting .NET Standard 1.3

## 2.0.0 2016-11-23

### Changed

- [#1](https://github.com/FantasticFiasco/serilog-sinks-http/pull/1) [BREAKING CHANGE] Sinks can be configured to use a custom implementation of `IHttpClient` (contribution by [@lhaussknecht](https://github.com/lhaussknecht))

## 1.0.0 2016-11-03

Initial version.
