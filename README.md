![Build status](https://github.com/pchalamet/FSharp.MongoDB.Driver/actions/workflows/build.yml/badge.svg?branch=main)

# WARNING
:exclamation: This is alpha quality. Following cases do not work as of now:
* DU of record

# FSharp.MongoDB.Driver
This project adds support for F# types to the [official .NET MongoDB driver][1].

It's a fork of [MongoDB.FSharp](https://github.com/tkellogg/MongoDB.FSharp) and has been extensively reworked to support .net 9 and other features.

Following types are supported:
* List
* Map
* Set
* Option
* ValueOption
* Discriminated Unions

## Breaking changes vs MongoDB.FSharp
* Discriminated unions are serialized with more information as this now uses DU property names.

# Installation
Install this project via NuGet.

Package | Status | Description
--------|--------|------------
FSharp.MongoDB.Driver | [![Nuget](https://img.shields.io/nuget/v/FSharp.MongoDB.Driver)](https://nuget.org/packages/FSharp.MongoDB.Driver) | Add F# support to MongoDB.Driver

On startup you have to register `FSharp.MongoDB.Driver`:
```ocaml
FSharp.MongoDB.Driver.Register()
```

# Usage
Use FSharp.MongoDB.Driver like you normally would in C#. 

```ocaml
type Person = { Id : BsonObjectId; Name : string; Scores : int list }

let connectionString = "mongodb://localhost"
let client = new MongoClient(connectionString)
let server = client.GetServer();
let db = server.GetDatabase("test")

let collection = db.GetCollection<Person> "people"

let id = BsonObjectId(ObjectId.GenerateNewId())
collection.Insert { Id = id; Name = "George"; Scores = [13; 52; 6] }

let george = collection.Find(fun person -> person.Id = id)
```

# Mapping

## Option and ValueOption
`null` if `None` or `ValueNone`.\
Otherwise the value.

## Map
key/value mapping.

## Discriminated Unions
key is the case name.\
value is an array of the values of the case

## Record
Records are supported as well out of the box with official MongoDB driver. Probably you want to add `CLIMutable` attribute on the record to support upsert operations.
```
[<CLIMutable>]
type RecordTypeOptId =
    { [<BsonIgnoreIfDefault>] Id : ObjectId
      Name : string }
```
