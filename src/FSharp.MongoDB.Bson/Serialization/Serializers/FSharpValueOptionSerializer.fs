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

namespace FSharp.MongoDB.Bson.Serialization.Serializers

open System.Diagnostics
open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

/// <summary>
/// Serializer for F# voption types that writes the value in the <c>ValueSome</c> case and <c>null</c> in
/// the <c>None</c> case.
/// </summary>
type FSharpValueOptionSerializer<'T>() =
    inherit SerializerBase<'T voption>()

    let serializer = lazy (BsonSerializer.LookupSerializer<'T>())

    override _.Serialize (context, args, value) =
        let writer = context.Writer

        match value with
        | ValueSome x -> serializer.Value.Serialize(context, args, x :> obj)
        | ValueNone -> writer.WriteNull()

    override _.Deserialize (context, args) =
        let reader = context.Reader

        match reader.GetCurrentBsonType() with
        | BsonType.Null -> reader.ReadNull(); ValueNone
        | _ -> ValueSome (serializer.Value.Deserialize(context, args))