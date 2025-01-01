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

namespace MongoDB.Bson.Serialization.Conventions

open Microsoft.FSharp.Reflection
open MongoDB.Bson.Serialization.Conventions
open MongoDB.Bson.Serialization.Helpers

/// <summary>
/// Convention for F# record types that initializes a <c>BsonClassMap</c> by mapping the record
/// type's constructor and fields.
/// </summary>
type FSharpRecordConvention() =
    inherit ConventionBase()

    interface IClassMapConvention with
        member _.Apply classMap =
            match classMap.ClassType with
            | IsRecord typ ->
                let fields = FSharpType.GetRecordFields(typ, bindingFlags)
                let names = fields |> Array.map _.Name

                // Map the constructor of the record type.
                let ctor = FSharpValue.PreComputeRecordConstructorInfo(typ, bindingFlags)
                classMap.MapConstructor(ctor, names) |> ignore

                // Map each field of the record type.
                fields |> Array.iter (mapMemberNullable classMap)
            | _ -> ()
