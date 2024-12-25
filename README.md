# FSharp.MongoDB.Driver
This project takes the [official .NET MongoDB driver][1] and serializers for F# immutable type.

It's a fork of [MongoDB.FSharp](https://github.com/tkellogg/MongoDB.FSharp) and has been extensively reworked to make it support .net 9 and nullable.

Following types are supported:
* List
* Map
* Set
* Option
* ValueOption
* Discriminated Unions

Records are supported as well out of the box with official MongoDB driver. Probably you want to add `CLIMutable` attribute on the record to support automatic ObjectId initialization.
```
[<CLIMutable>]
type RecordTypeOptId =
    { [<BsonIgnoreIfDefault>] Id : ObjectId
      Name : string }
```

# Installation
Install this project via NuGet.

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
