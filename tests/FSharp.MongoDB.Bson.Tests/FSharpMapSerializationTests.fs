(* Copyright (c) 2013 MongoDB, Inc.
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

open FsUnit
open MongoDB.Bson

open NUnit.Framework

module FSharpMapSerialization =

    type Primitive =
        { Bool : Map<string, bool>
          Int : Map<string, int>
          String : Map<string, string>
          Float : Map<string, float> }

    [<Test>]
    let ``test serialize an empty map``() =
        let value = { Bool = Map.empty<string, bool>
                      Int = Map.empty<string, int>
                      String = Map.empty<string, string>
                      Float = Map.empty<string, float> }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("Bool", BsonDocument())
                                      BsonElement("Int", BsonDocument())
                                      BsonElement("String", BsonDocument())
                                      BsonElement("Float", BsonDocument()) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize an empty map``() =
        let doc = BsonDocument([ BsonElement("Bool", BsonDocument())
                                 BsonElement("Int", BsonDocument())
                                 BsonElement("String", BsonDocument())
                                 BsonElement("Float", BsonDocument()) ])

        let result = deserialize doc typeof<Primitive>
        let expected = { Bool = Map.empty<string, bool>
                         Int = Map.empty<string, int>
                         String = Map.empty<string, string>
                         Float = Map.empty<string, float> }

        result |> should equal expected

    [<Test>]
    let ``test serialize a map of one element``() =
        let value = { Bool = Map.ofList<string, bool> [ ("a", false) ]
                      Int = Map.ofList<string, int> [ ("a", 0) ]
                      String = Map.ofList<string, string> [ ("a", "0.0") ]
                      Float = Map.ofList<string, float> [ ("a", 0.0) ] }

        let result = serialize value
        let expected = BsonDocument([ BsonElement("Bool", BsonDocument("a", BsonBoolean false))
                                      BsonElement("Int", BsonDocument("a", BsonInt32 0))
                                      BsonElement("String", BsonDocument("a", BsonString "0.0"))
                                      BsonElement("Float", BsonDocument("a", BsonDouble 0.0)) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize a map of one element``() =
        let doc = BsonDocument([ BsonElement("Bool", BsonDocument("a", BsonBoolean false))
                                 BsonElement("Int", BsonDocument("a", BsonInt32 0))
                                 BsonElement("String", BsonDocument("a", BsonString "0.0"))
                                 BsonElement("Float", BsonDocument("a", BsonDouble 0.0)) ])

        let result = deserialize doc typeof<Primitive>
        let expected = { Bool = Map.ofList<string, bool> [ ("a", false) ]
                         Int = Map.ofList<string, int> [ ("a", 0) ]
                         String = Map.ofList<string, string> [ ("a", "0.0") ]
                         Float = Map.ofList<string, float> [ ("a", 0.0) ] }

        result |> should equal expected

    [<Test>]
    let ``test serialize a map of multiple elements``() =
        let value =
            { Bool = Map.ofList<string, bool> [ ("a", false); ("b", true); ("c", false) ]
              Int = Map.ofList<string, int> [ ("a", 0); ("b", 1); ("c", 2) ]
              String = Map.ofList<string, string> [ ("a", "0.0"); ("b", "1.0"); ("c", "2.0") ]
              Float = Map.ofList<string, float> [ ("a", 0.0); ("b", 1.0); ("c", 2.0) ] }

        let result = serialize value
        let expected =
            BsonDocument(
                [ BsonElement("Bool", BsonDocument([ BsonElement("a", BsonBoolean false)
                                                     BsonElement("b", BsonBoolean true)
                                                     BsonElement("c", BsonBoolean false) ]))
                  BsonElement("Int", BsonDocument([ BsonElement("a", BsonInt32 0)
                                                    BsonElement("b", BsonInt32 1)
                                                    BsonElement("c", BsonInt32 2) ]))
                  BsonElement("String", BsonDocument([ BsonElement("a", BsonString "0.0")
                                                       BsonElement("b", BsonString "1.0")
                                                       BsonElement("c", BsonString "2.0") ]))
                  BsonElement("Float", BsonDocument([ BsonElement("a", BsonDouble 0.0)
                                                      BsonElement("b", BsonDouble 1.0)
                                                      BsonElement("c", BsonDouble 2.0) ])) ])

        result |> should equal expected

    [<Test>]
    let ``test deserialize a map of multiple elements``() =
        let doc =
            BsonDocument(
                [ BsonElement("Bool", BsonDocument([ BsonElement("a", BsonBoolean false)
                                                     BsonElement("b", BsonBoolean true)
                                                     BsonElement("c", BsonBoolean false) ]))
                  BsonElement("Int", BsonDocument([ BsonElement("a", BsonInt32 0)
                                                    BsonElement("b", BsonInt32 1)
                                                    BsonElement("c", BsonInt32 2) ]))
                  BsonElement("String", BsonDocument([ BsonElement("a", BsonString "0.0")
                                                       BsonElement("b", BsonString "1.0")
                                                       BsonElement("c", BsonString "2.0") ]))
                  BsonElement("Float", BsonDocument([ BsonElement("a", BsonDouble 0.0)
                                                      BsonElement("b", BsonDouble 1.0)
                                                      BsonElement("c", BsonDouble 2.0) ])) ])

        let result = deserialize doc typeof<Primitive>
        let expected =
            { Bool = Map.ofList<string, bool> [ ("a", false); ("b", true); ("c", false) ]
              Int = Map.ofList<string, int> [ ("a", 0); ("b", 1); ("c", 2) ]
              String = Map.ofList<string, string> [ ("a", "0.0"); ("b", "1.0"); ("c", "2.0") ]
              Float = Map.ofList<string, float> [ ("a", 0.0); ("b", 1.0); ("c", 2.0) ] }

        result |> should equal expected
