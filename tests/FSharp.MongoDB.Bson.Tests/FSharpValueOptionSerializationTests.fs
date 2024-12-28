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

module FSharpValueOptionSerialization =

    type Primitive =
        { Bool : bool voption
          Int : int voption
          String : string voption
          Float : float voption }

    [<Test>]
    let ``test serialize value optional primitives (valuenone) in a record type``() =
        let value =  { Bool = ValueNone
                       Int = ValueNone
                       String = ValueNone
                       Float = ValueNone }

        let result = serialize value
        let expected = BsonDocument()

        result |> should equal expected

    [<Test>]
    let ``test deserialize value optional primitives (valuenone) in a record type)``() =
        let doc = BsonDocument()

        let result = deserialize doc typeof<Primitive>
        let expected = { Bool = ValueNone
                         Int = ValueNone
                         String = ValueNone
                         Float = ValueNone }

        result |> should equal expected

    [<Test>]
    let ``test serialize value optional primitives (valuesome) in a record type``() =
        let value =  { Bool = ValueSome false
                       Int = ValueSome 0
                       String = ValueSome "0.0"
                       Float = ValueSome 0.0 }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("Bool", BsonBoolean false)
                                      BsonElement("Int", BsonInt32 0)
                                      BsonElement("String", BsonString "0.0")
                                      BsonElement("Float", BsonDouble 0.0) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize value optional primitives (value some) in a record type``() =
        let doc = BsonDocument([ BsonElement("Bool", BsonBoolean true)
                                 BsonElement("Int", BsonInt32 1)
                                 BsonElement("String", BsonString "1.0")
                                 BsonElement("Float", BsonDouble 1.0) ])

        let result = deserialize doc typeof<Primitive>
        let expected = { Bool = ValueSome true
                         Int = ValueSome 1
                         String = ValueSome "1.0"
                         Float = ValueSome 1.0 }

        result |> should equal expected
