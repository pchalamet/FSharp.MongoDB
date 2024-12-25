module FSharp.MongoDB.Driver.Serializers.Tests
open FsUnit
open NUnit.Framework
open MongoDB.Bson
open MongoDB.Driver

let mutable client: MongoClient = Unchecked.defaultof<MongoClient>
let mutable db: IMongoDatabase = Unchecked.defaultof<IMongoDatabase>

type Record =
    { A: int
      B: string option }

type Value =
    | None
    | Int of int
    | Float of float
    | String of string
    | Record of Record

[<CLIMutable>]
type CollectionItem =
    { Id: ObjectId
      Int: int
    //   IntOption: int option

      String: string
    //   StringOption: string option

      Record: Record
    //   RecordOption: Record option

      List: int list
    //   ListOption: int list option
    //   RecordListOption: Record list option 

      Set: Set<string>
      Map: Map<string, int>
      MapOption: Map<string, int> option
      
      DU: Value }
    //   DUList: Value list }


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
let ``Test complex record Some``() =
    let obj =
        { Id = ObjectId.GenerateNewId()
          Int = 42
        //   IntOption = Some 666
          String = "toto"
        //   StringOption = Some "tata"
          Record = { A = 42; B = Some "titi" }
        //   RecordOption = Some { A = 666; B = Some "tutu" }
          List = [ 1; 2; 3 ]
        //   ListOption = Some [ 4; 5; 6; 7 ]
        //   RecordListOption = Some [ { A = 42; B = Some "titi" }; { A = 666; B = Some "tutu" } ]
          Set = Set [ "1"; "2"; "3" ]
          Map = Map [ "tata", 1; "titi", 2; "tutu", 3 ]
          MapOption = Some <| Map ["toto", 42 ]
          DU = Record { A = 1; B = Some "tata" } }
        //   DUList = [ Int 42; Float 1.23; Record { A = 1; B = Some "tata" } ] }

    let collection = db.GetCollection<CollectionItem> "CollectionItem"
    collection.InsertOne(obj)

    let fromDb = collection.Find(fun x -> x.Id = obj.Id).First()
    ()
    // fromDb |> should equal obj
