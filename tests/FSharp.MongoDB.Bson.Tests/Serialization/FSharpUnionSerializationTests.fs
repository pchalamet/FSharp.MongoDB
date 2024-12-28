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

module FSharpUnionSerialization =

    type Primitive =
       | Bool of bool
       | Int of int
       | String of string
       | Float of float

    [<Test>]
    let ``test serialize primitives in a union case``() =
        [ (Bool false, BsonDocument([ BsonElement("_t", BsonString "Bool")
                                      BsonElement("Item", BsonBoolean false) ]))
          (Int 0, BsonDocument([ BsonElement("_t", BsonString "Int")
                                 BsonElement("Item", BsonInt32 0) ]))
          (String "0.0", BsonDocument([ BsonElement("_t", BsonString "String")
                                        BsonElement("Item", BsonString "0.0") ]))
          (Float 0.0, BsonDocument([ BsonElement("_t", BsonString "Float")
                                     BsonElement("Item", BsonDouble 0.0) ])) ]
        |> List.iter (fun (value, expected) ->
            let result = serialize value
            result |> should equal expected)

    [<Test>]
    let ``test deserialize primitives in a union case``() =
        [ (Bool true, BsonDocument([ BsonElement("_t", BsonString "Bool")
                                     BsonElement("Item", BsonBoolean true) ]))
          (Int 1, BsonDocument([ BsonElement("_t", BsonString "Int")
                                 BsonElement("Item", BsonInt32 1) ]))
          (String "1.0", BsonDocument([ BsonElement("_t", BsonString "String")
                                        BsonElement("Item", BsonString "1.0") ]))
          (Float 1.0, BsonDocument([ BsonElement("_t", BsonString "Float")
                                     BsonElement("Item", BsonDouble 1.0) ])) ]
        |> List.iter (fun (expected, doc) ->
            let result = deserialize<Primitive> doc
            result |> should equal expected)

    module Arity =

        type Number =
           | Zero
           | One of int
           | Two of int * int
           | Three of int * int * int

        [<Test>]
        let ``test serialize a union case with arity 0``() =
            let value = Zero

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "Zero") ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a union case with arity 0``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "Zero") ])

            let result = deserialize<Number> doc
            let expected = Zero

            result |> should equal expected

        [<Test>]
        let ``test serialize a union case with arity 1``() =
            let value = One 1

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "One")
                                          BsonElement("Item", BsonInt32 1) ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a union case with arity 1``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "One")
                                     BsonElement("Item", BsonInt32 1) ])

            let result = deserialize<Number> doc
            let expected = One 1

            result |> should equal expected

        [<Test>]
        let ``test serialize a union case with arity 2``() =
            let value = Two (1, 2)

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "Two")
                                          BsonElement("Item1", BsonInt32 1)
                                          BsonElement("Item2", BsonInt32 2) ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a union case with arity 2``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "Two")
                                     BsonElement("Item1", BsonInt32 1)
                                     BsonElement("Item2", BsonInt32 2) ])

            let result = deserialize<Number> doc
            let expected = Two (1, 2)

            result |> should equal expected

        [<Test>]
        let ``test serialize a union case with arity 3``() =
            let value = Three (1, 2, 3)

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "Three")
                                          BsonElement("Item1", BsonInt32 1)
                                          BsonElement("Item2", BsonInt32 2)
                                          BsonElement("Item3", BsonInt32 3) ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a union case with arity 3``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "Three")
                                     BsonElement("Item1", BsonInt32 1)
                                     BsonElement("Item2", BsonInt32 2)
                                     BsonElement("Item3", BsonInt32 3) ])

            let result = deserialize<Number> doc
            let expected = Three (1, 2, 3)

            result |> should equal expected

    module NullUnion =

        type Letter =
           | A
           | B
           | C
           | D

        [<Test>]
        let ``test serialize a null union case``() =
            [ (A, BsonDocument([ BsonElement("_t", BsonString "A") ]))
              (B, BsonDocument([ BsonElement("_t", BsonString "B") ]))
              (C, BsonDocument([ BsonElement("_t", BsonString "C") ]))
              (D, BsonDocument([ BsonElement("_t", BsonString "D") ])) ]
            |> List.iter (fun (value, expected) ->
                let result = serialize value
                result |> should equal expected)

        [<Test>]
        let ``test deserialize a null union case``() =
            [ (A, BsonDocument([ BsonElement("_t", BsonString "A") ]))
              (B, BsonDocument([ BsonElement("_t", BsonString "B") ]))
              (C, BsonDocument([ BsonElement("_t", BsonString "C") ]))
              (D, BsonDocument([ BsonElement("_t", BsonString "D") ])) ]
            |> List.iter (fun (expected, doc) ->
                let result = deserialize<Letter> doc
                result |> should equal expected)

    module Singleton =

        [<RequireQualifiedAccess>]
        type Only0 = | Case

        [<RequireQualifiedAccess>]
        type Only1 = | Case of int

        [<RequireQualifiedAccess>]
        type Only2 = | Case of int * int

        [<RequireQualifiedAccess>]
        type Only3 = | Case of int * int * int

        [<Test>]
        let ``test serialize a singleton union type with arity 0``() =
            let value = Only0.Case

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "Case") ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a singleton union type with arity 0``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "Case") ])

            let result = deserialize<Only0> doc
            let expected = Only0.Case

            result |> should equal expected

        [<Test>]
        let ``test serialize a singleton union type with arity 1``() =
            let value = Only1.Case 1

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "Case")
                                          BsonElement("Item", BsonInt32 1) ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a singleton union type with arity 1``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "Case")
                                     BsonElement("Item", BsonInt32 1) ])

            let result = deserialize<Only1> doc
            let expected = Only1.Case 1

            result |> should equal expected

        [<Test>]
        let ``test serialize a singleton union type with arity 2``() =
            let value = Only2.Case (1, 2)

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "Case")
                                          BsonElement("Item1", BsonInt32 1)
                                          BsonElement("Item2", BsonInt32 2) ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a singleton union type with arity 2``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "Case")
                                     BsonElement("Item1", BsonInt32 1)
                                     BsonElement("Item2", BsonInt32 2) ])

            let result = deserialize<Only2> doc
            let expected = Only2.Case (1, 2)

            result |> should equal expected

        [<Test>]
        let ``test serialize a singleton union type with arity 3``() =
            let value = Only3.Case (1, 2, 3)

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "Case")
                                          BsonElement("Item1", BsonInt32 1)
                                          BsonElement("Item2", BsonInt32 2)
                                          BsonElement("Item3", BsonInt32 3) ])

            result |> should equal expected

        [<Test>]
        let ``test deserialize a singleton union type with arity 3``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "Case")
                                     BsonElement("Item1", BsonInt32 1)
                                     BsonElement("Item2", BsonInt32 2)
                                     BsonElement("Item3", BsonInt32 3) ])

            let result = deserialize<Only3> doc
            let expected = Only3.Case (1, 2, 3)

            result |> should equal expected

    module BindingFlags =

        type internal InternalUnion =
           | Null
           | NonNull of int

        [<Test>]
        let ``test serialize an internal null union case``() =
            let value = Null

            let result = serialize value
            let expected = BsonDocument("_t", BsonString "Null")

            result |> should equal expected

        [<Test>]
        let ``test deserialize an internal null union case``() =
            let doc = BsonDocument("_t", BsonString "Null")

            let result = deserialize<InternalUnion> doc
            let expected = Null

            result |> should equal expected

        [<Test>]
        let ``test serialize an internal non-null union case``() =
            let value = NonNull 0

            let result = serialize value
            let expected = BsonDocument([ BsonElement("_t", BsonString "NonNull")
                                          BsonElement("Item", BsonInt32 0) ])
            result |> should equal expected

        [<Test>]
        let ``test deserialize an internal non-null union case``() =
            let doc = BsonDocument([ BsonElement("_t", BsonString "NonNull")
                                     BsonElement("Item", BsonInt32 1) ])

            let result = deserialize<InternalUnion> doc
            let expected = NonNull 1

            result |> should equal expected
