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
open MongoDB.Bson.IO
open MongoDB.Bson.Serialization

open FSharp.MongoDB.Bson.Serialization

[<AutoOpen>]
module Helpers =

    let serialize<'T> (value: 'T) =
        FSharpValueSerializer.register()

        let doc = BsonDocument()
        use writer = new BsonDocumentWriter(doc, BsonDocumentWriterSettings.Defaults)
        let typ = typeof<'T>
        let args = BsonSerializationArgs(typ, true, true)
        BsonSerializer.Serialize(writer, value, args = args)
        doc

    let deserialize doc (typ:System.Type) =
        FSharpValueSerializer.register()

        use reader = new BsonDocumentReader(doc, BsonDocumentReaderSettings.Defaults)
        BsonSerializer.Deserialize(reader, typ) |> unbox
