# FSharp.MongoDB

> Add support for F# types to [.net MongoDB.Driver][csharp_driver].

## Goals of this project

  * Provide support for F# types for interacting with MongoDB.
  * Have an implementation that is fully testable without connecting to a server.
  * Isomorphic bson serialization for C# and F#.

## FSharp.MongoDB history

Repository origins are:
  1. Initial repository: https://github.com/mongodb-labs/mongo-fsharp-driver-prototype
  1. Fork by @visemet (Max Hirschhorn): https://github.com/visemet/FSharp.MongoDB
  1. This repository: migrated to netstandard 2.1, nullable, adds new features (C#/F# isomomorphic serialization, voption support...)
  
## Building
  * build using the top-level solution file (`FSharp.MongoDB.sln`).
  * you can use `make` with target `build` or `test`.

## Supported platforms

This project targets `.net 8`, `.net 9` and  `netstandard2.1` ([compatible runtimes](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1#select-net-standard-version))

:warning: NRT support starts with `.net sdk 9.0.200`. F# compiler in .net sdk 9.0.10x does not set correctly nullable attributes on F# types. NRT are not supported on `netstandard2.1`.

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
MongoDB.Bson.Serialization.FSharp.register()
```

# Serialization format

## List
`List<_>` is serialized as an array. Order is preserved.

## Set
`Set<_>` is serialized as an array. Do not rely on the order.

## Array
`List<_>` is serialized as an array. Order is preserved.

## Seq
`Seq<_>` is serialized as an array. Order is preserved.

## Map
`Map<_, _>` is serialized as an object. Do not rely on the order of the keys.

## Option
`Option<_>` is either serialized as:
* `null` if `Option.None`
* `object` if `Option.Some`

On deserialization, missing value is mapped to `None`.

## ValueOption
`ValueOption<_>` is either serialized as:
* `null` if `ValueOption.ValueNone`
* `object` if `ValueOption.ValueSome`

On deserialization, missing value is mapped to `ValueNone`.

## Record
A record is serialized as an `object.

If you want to auto-generate `ObjectId` (as the Id of the collection), add `[<CLIMutable>]` on the record.

## Discriminated union
The case of the discriminated union is stored in `_t` key.
Each value of the DU is serialized as an `object` using its corresponding value name.

## Nullable Reference Type
NRT are serialized as:
* `null` if `Null`
* `object` if `NonNull` object

On deserialization, missing value is mapped to `null`.

:warning: NRT support starts with .net sdk 9.0.200. F# compiler in .net sdk 9.0.10x does not set correctly nullable attributes on F# types.

# License
The contents of this library are made available under the [Apache License, Version 2.0][license].

# Build Status

[![Build status](https://github.com/pchalamet/FSharp.MongoDB/actions/workflows/on-push-branch.yml/badge.svg?branch=main)](https://github.com/pchalamet/FSharp.MongoDB/actions/workflows/on-push-branch.yml)


  [csharp_driver]: https://github.com/mongodb/mongo-csharp-driver
  [issues]:        https://github.com/pchalamet/FSharp.MongoDB/issues
  [license]:       LICENSE
  [pull_requests]: https://github.com/pchalamet/FSharp.MongoDB/pulls
