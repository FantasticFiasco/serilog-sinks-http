# Change Log

All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](http://semver.org/) and is following the [change log format](http://keepachangelog.com/).

## Unreleased

## 3.0.0 2017-03-04

### Added

- A sink is durable when created using `Http(string, DurableOptions)`. A durable sink will persist log events on disk before sending them over the network, thus protecting against data loss after a system or process restart.

### Changed

- [BREAKING CHANGE] The syntax for creating a non-durable sink has been changed from `Http(string)` to `Http(string, Options)` to accommodate for the syntax to create a durable sink. A non-durable sink will loose data after a system or process restart.
- Improve compatibility by supporting .NET Standard 1.3

## 2.0.0 2016-11-23

### Changed

- [BREAKING CHANGE] Custom implementation of `IHttpClient` can be passed to sink when creating it (contribution by [@lhaussknecht](https://github.com/lhaussknecht))

## 1.0.0 2016-11-03

Initial version.
