namespace FSharp.MongoDB.Bson.Tests.Serialization

open CSharpDataModels

open FsUnit
open NUnit.Framework

open MongoDB.Bson

module IsomorphicSerialization =

    type Pair =
        { First: int
          Second: string option }

    [<RequireQualifiedAccess>]
    type Value =
        | IntValue of Value:int
        | StringValue of Value: string
        | PairValue of Value:Pair

    [<CLIMutable>]
    type RecordDataModel =
        { Id: ObjectId
            
          Int: int
          IntOpt: int option
          
          String: string
          StringOpt: string option
          
          Array: int array
          ArrayOpt: int array option
          
          Value: Value
          ValueOpt: Value option 
          
          ValueArray: Value array
          ValueArrayOpt: Value array option
          
          Record: Pair
          RecordOpt: Pair option
          
          Map: Map<string, int> }

    let ModelSome() =
        let csModel = 
            let map =
                let map = System.Collections.Generic.Dictionary<string, int>()
                map.Add("1", 1)
                map.Add("2", 2)
                map
            
            RecordDataModel(
                Id = ObjectId.GenerateNewId(),
                Int = 42,
                IntOpt = 666,
                String = "String",
                StringOpt = "StringOpt",
                Array = [| 1; 2; 3 |],
                ArrayOpt = [| 5; 6; 7; 8 |],
                Value = CSharpDataModels.Value.IntValue(42),
                ValueOpt = CSharpDataModels.Value.StringValue("ValueStringOpt"),
                ValueArray = [| CSharpDataModels.Value.IntValue(42)
                                CSharpDataModels.Value.StringValue("String")
                                CSharpDataModels.Value.PairValue(CSharpDataModels.Pair(First = 99, Second = "SecondPair")) |],
                ValueArrayOpt = [| CSharpDataModels.Value.IntValue(101) |],
                Record = CSharpDataModels.Pair(First = 1, Second = "Second"),
                RecordOpt = CSharpDataModels.Pair(First = -1, Second = "SecondOpt"),
                Map = map)

        let fsModel =
            { Id = csModel.Id
              Int = 42
              IntOpt = Some 666
              String = "String"
              StringOpt = Some "StringOpt"
              Array = [| 1; 2; 3 |] 
              ArrayOpt = Some [| 5; 6; 7; 8 |]
              Value = Value.IntValue 42
              ValueOpt = Some <| Value.StringValue "ValueStringOpt"
              ValueArray = [| Value.IntValue 42; Value.StringValue "String"; Value.PairValue { First = 99; Second = Some "SecondPair" } |]
              ValueArrayOpt = Some [| Value.IntValue 101 |]
              Record = { First = 1; Second = Some "Second" }
              RecordOpt = Some { First = -1; Second = Some "SecondOpt" }
              Map = Map [ "1", 1; "2", 2 ] }

        csModel, fsModel

    let ModelNone() =
        let csModel = 
            let map =
                let map = System.Collections.Generic.Dictionary<string, int>()
                map.Add("1", 1)
                map.Add("2", 2)
                map
            
            RecordDataModel(
                Id = ObjectId.GenerateNewId(),
                Int = 42,
                String = "String",
                Array = [| 1; 2; 3 |],
                Value = CSharpDataModels.Value.IntValue(42),
                ValueArray = [| CSharpDataModels.Value.IntValue(42)
                                CSharpDataModels.Value.StringValue("String")
                                CSharpDataModels.Value.PairValue(CSharpDataModels.Pair(First = 99, Second = "SecondPair")) |],
                Record = CSharpDataModels.Pair(First = 1, Second = "Second"),
                Map = map)

        let fsModel =
            { Id = csModel.Id
              Int = 42
              IntOpt = None
              String = "String"
              StringOpt = None
              Array = [| 1; 2; 3 |] 
              ArrayOpt = None
              Value = Value.IntValue 42
              ValueOpt = None
              ValueArray = [| Value.IntValue 42; Value.StringValue "String"; Value.PairValue { First = 99; Second = Some "SecondPair" } |]
              ValueArrayOpt = None
              Record = { First = 1; Second = Some "Second" }
              RecordOpt = None
              Map = Map [ "1", 1; "2", 2 ] }

        csModel, fsModel

    [<Test>]
    let ``Isomorphic Some cs / fs``() =
        let csModel, fsModel = ModelSome()

        let csDoc = serialize csModel
        let fsDoc = serialize fsModel
        
        printfn $"{csDoc}"
        printfn $"{fsDoc}"
        
        csDoc |> should equal fsDoc

    [<Test>]
    let ``Isomorphic None cs / fs``() =
        let csModel, fsModel = ModelNone()

        let csDoc = serialize csModel
        let fsDoc = serialize fsModel
        
        csDoc |> should equal fsDoc



    // [<Test>]
    // let ``Isomorphic Some fs -> cs``() =
    //     let csModel, fsModel = ModelSome()
    //
    //     let doc = serialize fsModel
    //     
    //     let result = deserialize doc typeof<CSharpDataModels.RecordDataModel>
    //
    //     result |> should equal csModel
    //
    //
    // [<Test>]
    // let ``Isomorphic None fs -> cs``() =
    //     let csModel, fsModel = ModelNone()
    //
    //     let doc = serialize fsModel
    //     let result = deserialize doc typeof<CSharpDataModels.RecordDataModel>
    //
    //     result |> should equal csModel
