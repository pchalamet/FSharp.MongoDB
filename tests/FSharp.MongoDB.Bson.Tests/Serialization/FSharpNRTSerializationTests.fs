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

module FSharpNRTSerialization =

    type Primitive =
        { String : string  | null }

    [<Test>]
    let ``test serialize nullable reference (null) in a record type``() =
        let value =  { String = null }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("String", BsonNull.Value) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize nullable reference (null) in a record type)``() =
        // let doc = BsonDocument([ BsonElement("String", BsonNull.Value) ])
        let doc = BsonDocument()

        let result = deserialize<Primitive> doc
        let expected = { String = null }

        result |> should equal expected

    [<Test>]
    let ``test serialize nullable reference (some) in a record type``() =
        let value =  { String = "A String" }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("String", BsonString "A String") ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize nullable reference (some) in a record type``() =
        let doc = BsonDocument([ BsonElement("String", BsonString "A String") ])

        let result = deserialize<Primitive> doc
        let expected = { String = "A String" }

        result |> should equal expected
