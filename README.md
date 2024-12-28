![Build status](https://github.com/pchalamet/FSharp.MongoDB/actions/workflows/build.yml/badge.svg?branch=main)

# FSharp.MongoDB

> an F# interface for the MongoDB .NET driver

## Goals of this project

  * Provide an idiomatic F# API for interacting with MongoDB.
  * Have an implementation that is fully testable without connecting to a server.
  
## FSharp.MongoDB history

Repository origins are:
  1. Initial repository: https://github.com/mongodb-labs/mongo-fsharp-driver-prototype
  1. Fork by @visemet (Max Hirschhorn): https://github.com/visemet/FSharp.MongoDB
  1. This repository: migrated to netstandard 2.1, nullable, adds new features (C#/F# isomomorphic serialization, voption support...)
  
## Building
  * build using the top-level solution file (`FSharp.MongoDB.sln`).
  * you can use `make` with target `build` or `test`.

## Supported platforms

This project targets `netstandard2.1` ([compatible runtimes](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1#select-net-standard-version)). 

## Contributing
  * If you have a question about the library, then create an [issue][issues] with the `question` label.
  * If you'd like to report a bug or submit a feature request, then create an [issue][issues] with the appropriate label.
  * If you'd like to contribute, then feel free to send a [pull request][pull_requests].

# Usage

## Installation
To serialize F# types, first install this project via NuGet:

Package | Status | Description
--------|--------|------------
FSharp.MongoDB | [![Nuget](https://img.shields.io/nuget/v/FSharp.MongoDB)](https://nuget.org/packages/FSharp.MongoDB) | Add F# support to MongoDB.Driver

## Register

On startup you have to register serializers:
```ocaml
MongoDB.Bson.Serialization.FSharpSerializer.register()
```

# License
The contents of this library are made available under the [Apache License, Version 2.0][license].

  [csharp_driver]: https://github.com/mongodb/mongo-csharp-driver
  [issues]:        https://github.com/pchalamet/FSharp.MongoDB/issues
  [license]:       LICENSE
  [pull_requests]: https://github.com/pchalamet/FSharp.MongoDB/pulls
