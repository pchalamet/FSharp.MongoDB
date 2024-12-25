module ``Acceptance Tests``

open System
open Mongo2Go
open FsUnit
open NUnit.Framework
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.FSharp
open System.Linq
open TestUtils

type ObjectWithList() =
    member val Id : BsonObjectId = newBsonObjectId() with get, set
    member val List : string list = [] with get, set

type RecordType = {
    Id : BsonObjectId
    Name : string
}

type Child = {
    ChildName: string
    Age: int
}

type Person = {
    Id: BsonObjectId
    PersonName: string
    Age: int
    Childs: Child seq
}

type DimmerSwitch =
    | Off
    | Dim of int
    | DimMarquee of int * string
    | On
    
type RecordWithList =
    { Id: BsonObjectId
      IntVal: int
      DoubleVal: double
      ListVal: int list
      OptionVal: int option }

type ObjectWithDimmer() =
    member val Id : BsonObjectId = newBsonObjectId() with get, set
    member val Switch : DimmerSwitch = Off with get, set

type ObjectWithDimmers() =
    member val Id : BsonObjectId = newBsonObjectId() with get, set
    member val Kitchen : DimmerSwitch = Off with get, set
    member val Bedroom1 : DimmerSwitch = Off with get, set
    member val Bedroom2 : DimmerSwitch = Off with get, set

type ObjectWithOptions() =
    member val Id : BsonObjectId = newBsonObjectId() with get, set
    member val Age : int option = None with get, set


let mutable client: MongoClient = Unchecked.defaultof<MongoClient>
let mutable db: IMongoDatabase = Unchecked.defaultof<IMongoDatabase>

[<OneTimeSetUp>]
let init() =
    let runner = MongoDbRunner.Start()
    client <- new MongoClient(runner.ConnectionString)
    db <- client.GetDatabase("IntegrationTest")
    Serializers.Register()

[<OneTimeTearDown>]
let teardown() =
    client.Dispose()


[<Test>]
let ``It can serialize an object with a list``() =
    let collection = db.GetCollection<ObjectWithList> "objects"
    let obj = ObjectWithList(List = [ "hello"; "world" ])
    collection.InsertOne(obj)

    let genCollection = db.GetCollection<ObjectWithList> "objects"
    let fromDb = genCollection.Find(fun x -> x.Id = obj.Id).ToList().First()
    let array = fromDb.List
    array.Length |> should equal 2

[<Test>]
let ``It can deserialze lists``() =
    let list = BsonArray([ "hello"; "world" ])
    let id = newBsonObjectId()
    let document = BsonDocument([ BsonElement("_id", id); BsonElement("List", list) ])
    let collection = db.GetCollection<BsonDocument> "objects"
    collection.InsertOne document

    let collection = db.GetCollection<ObjectWithList> "objects"
    let fromDb = collection.Find(fun x -> x.Id = id).ToList().First()
    let array = fromDb.List
    array.Length |> should equal 2

[<Test>]
let ``It can serialize records``() =
    let collection = db.GetCollection<RecordType> "objects"
    let obj = { Id = newBsonObjectId(); Name = "test"  }
    collection.InsertOne obj

    let genCollection = db.GetCollection<BsonDocument> "objects"
    let fromDb = genCollection |> findById obj.Id
    let name = fromDb["Name"].AsString
    name |> should equal "test"

[<Test>]
let ``It can deserialize records``() =
    let id = newBsonObjectId()
    let document = BsonDocument([BsonElement("_id", id); BsonElement("Name", BsonString("value"))])
    let collection = db.GetCollection "objects"
    collection.InsertOne(document)

    let collection = db.GetCollection<RecordType>("objects")
    let fromDb = collection.Find(fun x -> x.Id = id).ToList().First()
    Assert.NotNull(fromDb)
    fromDb.Name |> should equal "value"

[<Test>]
let ``It can serialize and deserialize nested records``() =
    let collection = db.GetCollection<Person> "persons"
    let obj = { Id = newBsonObjectId(); PersonName = "test"; Age = 33; Childs = [{ChildName = "Adrian"; Age = 3}] }
    collection.InsertOne obj

    let genCollection = db.GetCollection<Person> "persons"
    let person = query { 
        for p in genCollection.AsQueryable() do 
        where (p.Id = obj.Id) 
        select p
        headOrDefault
    }

    person |> should not' (be null)
    person.PersonName |> should equal "test"
    person.Age |> should equal 33
    person.Childs |> Seq.length |> should equal 1

    let child = person.Childs |> Seq.head
    child.ChildName |> should equal "Adrian"
    child.Age |> should equal 3

[<Test>]
let ``It can serialize DimmerSwitch types``() =
    let collection = db.GetCollection<ObjectWithDimmer> "objects"
    let obj = ObjectWithDimmer(Switch = DimMarquee(42, "loser"))
    collection.InsertOne obj

    let collection = db.GetCollection<BsonDocument> "objects"
    let fromDb = collection |> findById obj.Id
    let switch = fromDb.GetElement("Switch")
    switch |> should not' (be null)
    let value = switch.Value.AsBsonDocument.GetElement("DimMarquee").Value
    value.IsBsonArray |> should be True
    let array = value.AsBsonArray
    array.Count |> should equal 2
    array.[0].AsInt32 |> should equal 42
    array.[1].AsString |> should equal "loser"
    
[<Test>]
let ``It can serialize option types``() =
    let collection = db.GetCollection<ObjectWithOptions> "objects"
    let obj = ObjectWithOptions(Age = Some 42)
    collection.InsertOne obj

    let collection = db.GetCollection<BsonDocument> "objects"
    let fromDb = collection |> findById obj.Id
    let age = fromDb.GetElement("Age")
    let v = age.Value
    v.AsInt32 |> should equal 42

[<Test>]
let ``It can serialize option types with None``() =
    let collection = db.GetCollection<ObjectWithOptions> "objects"
    let obj = ObjectWithOptions(Age = None)
    collection.InsertOne obj

    let collection = db.GetCollection<BsonDocument> "objects"
    let fromDb = collection |> findById obj.Id
    let age = fromDb.GetElement("Age")
    let v = age.Value
    v.AsBsonNull |> should equal BsonNull.Value

[<Test>]
let ``It can deserialize option types``() =
    let collection = db.GetCollection<ObjectWithOptions> "objects"
    let document = ObjectWithOptions(Id = newBsonObjectId(), Age = Some 42)
    collection.InsertOne document

    let collection = db.GetCollection<ObjectWithOptions> "objects"
    let fromDb = collection.Find(fun x -> x.Id = document.Id).ToList().First()
    match fromDb.Age with
    | Some 42 -> ()
    | _ -> failwith "expected Some 42 but got something else"

[<Test>]
let ``It can deserialize option types from undefined``() =
    let id = newBsonObjectId()
    let document = BsonDocument([BsonElement("_id", id)])
    let collection = db.GetCollection "objects"
    collection.InsertOne document

    let collection = db.GetCollection<ObjectWithOptions> "objects"
    let fromDb = collection.Find(fun x -> x.Id = id).ToList().First()
    fromDb.Age |> should equal None

[<Test>]
let ``We can integrate serialize & deserialize on DimmerSwitches``() =
    let collection = db.GetCollection<ObjectWithDimmers> "objects"
    let obj = ObjectWithDimmers(Kitchen = Off,
                                Bedroom1 = Dim 42,
                                Bedroom2 = DimMarquee(12, "when I was little..."))
    collection.InsertOne obj

    let fromDb = collection.Find(fun x -> x.Id = obj.Id).ToList().First()
    match fromDb.Kitchen with
    | Off -> ()
    | _ -> failwith "Kitchen light wasn't off"

    match fromDb.Bedroom1 with
    | Dim 42 -> ()
    | _ -> failwith "Bedroom1 light wasn't dim enough"

    match fromDb.Bedroom2 with
    | DimMarquee(12, "when I was little...") -> ()
    | _ -> failwith "Bedroom2 doesn't have the party we thought"
    
[<Test>]
let ``It can serialize record with list`` () =
    let collection = db.GetCollection<RecordWithList> "objects"
    let obj = {
        Id = newBsonObjectId()
        IntVal = 123
        DoubleVal = 1.23
        ListVal = [1;2;3]
        OptionVal = Some 123
    }
    collection.InsertOne obj

    let testCollection = db.GetCollection<BsonDocument> "objects"
    Console.WriteLine((testCollection |> findById obj.Id).ToJson())
    
    let fromDb = collection.Find(fun x -> x.Id = obj.Id).ToList().First()
    obj |> should equal fromDb
    
