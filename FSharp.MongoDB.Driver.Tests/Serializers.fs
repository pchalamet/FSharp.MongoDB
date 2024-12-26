module FSharp.MongoDB.Driver.Serializers.Tests
open FsUnit
open NUnit.Framework
open MongoDB.Bson
open MongoDB.Driver

let mutable client: MongoClient = Unchecked.defaultof<MongoClient>
let mutable db: IMongoDatabase = Unchecked.defaultof<IMongoDatabase>

type TheRecord =
    { A: int
      B: string option }

[<RequireQualifiedAccess>]
type Value =
    | None
    | Int of a:int * b:int
    | Float of float
    | String of string
    | TheRecord of toto:TheRecord

type Class() =
    member val Id : ObjectId = ObjectId.GenerateNewId() with get, set
    member val List : int list = [] with get, set


[<CLIMutable>]
type CollectionItem =
    { Id: ObjectId
      Int: int
      IntOption: int option

      String: string
      StringOption: string option

      Record: TheRecord
    //   RecordOption: TheRecord option

      List: int list
      ListOption: int list option
      RecordListOption: TheRecord list option 

      Set: Set<string>
      Map: Map<string, int>
      MapOption: Map<string, int> option
      
      DU: Value
      DUList: Value list }


[<OneTimeSetUp>]
let init() =
    let connectionString = "mongodb://localhost"
    let dbname = "FSharp-MongoDB-Driver"
    client <- new MongoClient(connectionString)
    client.DropDatabase(dbname)
    db <- client.GetDatabase(dbname)
    FSharp.MongoDB.Driver.Register()

[<OneTimeTearDown>]
let teardown() =
    client.Dispose()

[<Test>]
let ``Roundtrip complex record with Some``() =
    let obj =
        { Id = ObjectId.GenerateNewId()
          Int = 42
          IntOption = Some 666
          String = "toto"
          StringOption = Some "tata"
          Record = { A = 42; B = Some "titi" }
        //   RecordOption = Some { A = 666; B = Some "tutu" }
          List = [ 1; 2; 3 ]
          ListOption = Some [ 4; 5; 6; 7 ]
          RecordListOption = Some [ { A = 42; B = Some "titi" }; { A = 666; B = Some "tutu" } ]
          Set = Set [ "1"; "2"; "3" ]
          Map = Map [ "tata", 1; "titi", 2; "tutu", 3 ]
          MapOption = Some <| Map ["toto", 42 ]
          DU = Value.Int (42, 666)  // TheRecord { A = 1; B = Some "tata" } // // 
          DUList = [ Value.Int (42, 33); Value.Float 1.23 ] }

    let collection = db.GetCollection<CollectionItem> "CollectionItem"
    collection.InsertOne(obj)

    let fromDb = collection.Find(fun x -> x.Id = obj.Id).First()
    fromDb |> should equal obj

[<Test>]
let ``Roundtrip complex record with None``() =
    let obj =
        { Id = ObjectId.GenerateNewId()
          Int = 42
          IntOption = None
          String = "toto"
          StringOption = None
          Record = { A = 42; B = Some "titi" }
        //   RecordOption = Some { A = 666; B = Some "tutu" }
          List = [ 1; 2; 3 ]
          ListOption = None
          RecordListOption = None
          Set = Set [ "1"; "2"; "3" ]
          Map = Map [ "tata", 1; "titi", 2; "tutu", 3 ]
          MapOption = None
          DU = Value.Int (42, 666)  // TheRecord { A = 1; B = Some "tata" } // // 
          DUList = [ Value.Int (42, 33); Value.Float 1.23 ] }

    let collection = db.GetCollection<CollectionItem> "CollectionItem"
    collection.InsertOne(obj)

    let fromDb = collection.Find(fun x -> x.Id = obj.Id).First()
    fromDb |> should equal obj
