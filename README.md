# FSharp.MongoDB history

Starting from this commit, this repository has been pruned and updated to netstandard 2.1.

Repository origins are:
* Initial repository: https://github.com/mongodb-labs/mongo-fsharp-driver-prototype
* Forked by @visemet (Max Hirschhorn): https://github.com/visemet/FSharp.MongoDB





FSharp.MongoDB
==============

> an F# interface for the MongoDB .NET driver

**Disclaimer.** This is an experimental project being maintained in my _infinite free time_. It is
in no way supported by MongoDB, Inc., and probably shouldn't be used in production.

### Goals of this project

  * Provide an idiomatic F# API for interacting with MongoDB.
  * Have an implementation that is fully testable without connecting to a server.

### Non-goals of this project

  * Have feature parity with the [C# driver][csharp_driver].

Building
--------

  - Simply build the `FSharpDriver-2012.sln` solution in Visual Studio, Xamarin Studio, or Mono
    Develop. You can also run the FAKE script

      * `build.cmd` on Windows.
      * `build.sh` on Linux or OS X.

### Supported F# runtimes

  - FSharp.Core v4.3.0.0 (F# 3.0)
  - FSharp.Core v4.3.1.0 (F# 3.1)
  - FSharp.Core v4.4.0.0 (F# 4.0)

### Supported platforms

  - .NET Framework 4.5

Contributing
------------

  - If you have a question about the library, then create an [issue][issues] with the `question`
    label.
  - If you'd like to report a bug or submit a feature request, then create an [issue][issues] with
    the appropriate label.
  - If you'd like to contribute, then feel free to send a [pull request][pull_requests].

License
-------

The contents of this library are made available under the [Apache License, Version 2.0][license].

  [csharp_driver]: https://github.com/mongodb/mongo-csharp-driver
  [issues]:        https://github.com/visemet/FSharp.MongoDB/issues
  [license]:       LICENSE
  [pull_requests]: https://github.com/visemet/FSharp.MongoDB/pulls
