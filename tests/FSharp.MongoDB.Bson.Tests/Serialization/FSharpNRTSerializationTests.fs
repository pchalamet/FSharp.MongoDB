(*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *)

namespace FSharp.MongoDB.Bson.Tests.Serialization

open MongoDB.Bson
open FsUnit
open NUnit.Framework

module FSharpNRTSerialization =

    type UnionCase =
        | Tuple of Int:int * String:(string | null)

    type UnionCaseNotNull =
        | TupleNotNull of Int:int * String:string

    type Primitive =
        { String : string  | null
          UnionCase: UnionCase
          Int: int }

    type RecordNoNull =
        { StringNotNull : string 
          Int: int }

    type UnionCaseNoNull =
        { UnionCaseNotNull : UnionCaseNotNull 
          Int: int }


    [<Test>]
    let ``test serialize nullable reference (null) in a record type``() =
        let value =  { String = null
                       UnionCase = Tuple(Int = 42, String = null)
                       Int = 42 }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("String", BsonNull.Value)
                                      BsonElement("UnionCase", BsonDocument([ BsonElement("_t", BsonString "Tuple")
                                                                              BsonElement("Int", BsonInt32 42)
                                                                              BsonElement("String", BsonNull.Value) ]))
                                      BsonElement("Int", BsonInt32 42) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize nullable reference (null) in a record type)``() =
        // FIXME: once .net 9.0.200 is released, String can be omitted
        let doc = BsonDocument([ BsonElement("String", BsonNull.Value)
                                 BsonElement("UnionCase", BsonDocument([ BsonElement("_t", BsonString "Tuple")
                                                                         BsonElement("Int", BsonInt32 42)
                                                                         BsonElement("String", BsonNull.Value) ]))
                                 BsonElement("Int", BsonInt32 42) ])

        let result = deserialize<Primitive> doc
        let expected = { String = null
                         UnionCase = Tuple(Int = 42, String = null)
                         Int = 42 }

        result |> should equal expected

    [<Test>]
    let ``test serialize nullable reference (some) in a record type``() =
        let value =  { String = "A String"
                       UnionCase = Tuple(Int = 42, String = "Another String")
                       Int = 42 }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("String", BsonString "A String")
                                      BsonElement("UnionCase", BsonDocument([ BsonElement("_t", BsonString "Tuple")
                                                                              BsonElement("Int", BsonInt32 42)
                                                                              BsonElement("String", BsonString "Another String") ]))
                                      BsonElement("Int", BsonInt32 42) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize nullable reference (some) in a record type``() =
        let doc = BsonDocument([ BsonElement("String", BsonString "A String")
                                 BsonElement("UnionCase", BsonDocument([ BsonElement("_t", BsonString "Tuple")
                                                                         BsonElement("Int", BsonInt32 42)
                                                                         BsonElement("String", BsonString "Another String") ]))
                                 BsonElement("Int", BsonInt32 42) ])

        let result = deserialize<Primitive> doc
        let expected = { String = "A String"
                         UnionCase = Tuple(Int = 42, String = "Another String")
                         Int = 42 }

        result |> should equal expected

    [<Test>]
    let ``test deserialize with missing non-null reference in record shall fail``() =
        let doc = BsonDocument([ BsonElement("Int", BsonInt32 42) ])

        (fun () -> deserialize<RecordNoNull> doc |> ignore)
        |> should throw typeof<BsonSerializationException>

    [<Test>]
    let ``test deserialize with missing non-null reference in union case shall fail``() =
        let doc = BsonDocument([ BsonElement("Int", BsonInt32 42) ])

        (fun () -> deserialize<UnionCaseNoNull> doc |> ignore)
        |> should throw typeof<BsonSerializationException>
