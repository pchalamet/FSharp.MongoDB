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

namespace FSharp.MongoDB.Bson.Serialization

open System
open Microsoft.FSharp.Reflection

open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Conventions

open FSharp.MongoDB.Bson.Serialization.Conventions
open FSharp.MongoDB.Bson.Serialization.Helpers
open FSharp.MongoDB.Bson.Serialization.Serializers

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module FSharpValueSerializer =

    /// <summary>
    /// Provides (de)serialization of F# data types, including lists, maps, options, records, sets,
    /// and discriminated unions.
    /// </summary>
    type private FSharpValueSerializationProvider() =

        interface IBsonSerializationProvider with

            member __.GetSerializer typ =
                let mkSerializer = function
                | Some (typ:System.Type) -> System.Activator.CreateInstance typ :?> IBsonSerializer
                | None -> null

                match typ with
                | IsList typ -> Some (mkGenericUsingDef<FSharpListSerializer<_>> typ)
                | IsMap typ -> Some (mkGenericUsingDef<FSharpMapSerializer<_, _>> typ)
                | IsValueOption typ -> Some (mkGenericUsingDef<FSharpValueOptionSerializer<_>> typ)
                | IsOption typ -> Some (mkGenericUsingDef<FSharpOptionSerializer<_>> typ)
                | IsSet typ -> Some (mkGenericUsingDef<FSharpSetSerializer<_>> typ)
                | IsUnion typ -> Some (mkGeneric<FSharpUnionSerializer<_>> [| typ |])
                | _ -> None
                |> mkSerializer

    let private addConvention name pred convention =
        let pack = { new IConventionPack with
            member __.Conventions = Seq.singleton convention }
        ConventionRegistry.Register(name, pack, (fun typ -> pred typ |> Option.isSome))

    let mutable private registered = false

    /// <summary>
    /// Registers the serializers and conventions for F# data types, including lists, maps, options,
    /// records, sets, and discriminated unions.
    /// </summary>
    [<CompiledName("Register")>]
    let register() =
        if not registered then
            registered <- true

            IgnoreIfNoneConvention()
            |> addConvention "__fsharp_option_type__" (Some)

            FSharpRecordConvention()
            |> addConvention "__fsharp_record_type__" (|IsRecord|_|)

            UnionCaseConvention()
            |> addConvention "__fsharp_union_type__" (|IsUnion|_|)

            FSharpValueSerializationProvider()
            |> BsonSerializer.RegisterSerializationProvider
