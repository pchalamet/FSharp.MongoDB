(* Copyright (c) 2015 MongoDB, Inc.
 *
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

module FSharpOptionSerialization =

    type Primitive =
        { Bool : bool option
          Int : int option
          String : string option
          Float : float option }

    [<Test>]
    let ``test serialize optional primitives (none) in a record type``() =
        let value =  { Bool = None
                       Int = None
                       String = None
                       Float = None }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("Bool", BsonNull.Value)
                                      BsonElement("Int", BsonNull.Value)
                                      BsonElement("String", BsonNull.Value)
                                      BsonElement("Float", BsonNull.Value) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize optional primitives (none) in a record type)``() =
        let doc = BsonDocument()

        let result = deserialize<Primitive> doc
        let expected = { Bool = None
                         Int = None
                         String = None
                         Float = None }

        result |> should equal expected

    [<Test>]
    let ``test serialize optional primitives (some) in a record type``() =
        let value =  { Bool = Some false
                       Int = Some 0
                       String = Some "0.0"
                       Float = Some 0.0 }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("Bool", BsonBoolean false)
                                      BsonElement("Int", BsonInt32 0)
                                      BsonElement("String", BsonString "0.0")
                                      BsonElement("Float", BsonDouble 0.0) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize optional primitives (some) in a record type``() =
        let doc = BsonDocument([ BsonElement("Bool", BsonBoolean true)
                                 BsonElement("Int", BsonInt32 1)
                                 BsonElement("String", BsonString "1.0")
                                 BsonElement("Float", BsonDouble 1.0) ])

        let result = deserialize<Primitive> doc
        let expected = { Bool = Some true
                         Int = Some 1
                         String = Some "1.0"
                         Float = Some 1.0 }

        result |> should equal expected
