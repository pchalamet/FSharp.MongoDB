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

namespace FSharp.MongoDB.Bson.Serialization.Conventions

open Microsoft.FSharp.Reflection
open MongoDB.Bson.Serialization.Conventions

open FSharp.MongoDB.Bson.Serialization.Helpers

/// <summary>
/// Convention for F# option types that writes the value in the <c>Some</c> case and omits the field
/// in the <c>None</c> case.
/// </summary>
type IgnoreIfNoneConvention() =
    inherit ConventionBase()

    interface IMemberMapConvention with
        member _.Apply memberMap =
            let setDefaultValue name typ =
                let unionCase = FSharpType.GetUnionCases(typ) |> Array.find (fun case -> case.Name = name)
                let none = FSharpValue.MakeUnion(unionCase, [||])
                memberMap.SetDefaultValue none |> ignore
                
            match memberMap.MemberType with
            | IsOption _ ->
                // Option uses UseNullAsTrueValue
                memberMap.SetDefaultValue null |> ignore
                memberMap.SetIgnoreIfDefault true |> ignore
            | IsValueOption _ ->
                mkGenericUsingDef<_ voption> memberMap.MemberType |> setDefaultValue "ValueNone"
                memberMap.SetIgnoreIfDefault true |> ignore
            | _ -> ()
