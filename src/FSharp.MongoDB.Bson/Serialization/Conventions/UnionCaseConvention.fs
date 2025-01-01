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

open System
open System.Linq.Expressions
open System.Reflection
open Microsoft.FSharp.Reflection
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Conventions
open MongoDB.Bson.Serialization.Helpers

/// <summary>
/// Convention for non-null union cases of F# discriminated unions that initializes a
/// <c>BsonClassMap</c> by mapping the union case's constructor and fields.
/// </summary>
type UnionCaseConvention() =
    inherit ConventionBase()

    let tryGetUnionCase (typ:System.Type) =
        // 8.5.4. Compiled Form of Union Types for Use from Other CLI Languages
        //   A compiled union type U has [o]ne CLI nested type U.C for each non-null union case C.
        //   [...] However, a compiled union type that has only one case does not have a nested
        //   type. Instead, the union type itself plays the role of the case type.
        match (typ, typ.DeclaringType) with
        | (IsUnion _, IsUnion unionTyp) ->
            // Get the union case corresponding to the nested type of the discriminated union.
            FSharpType.GetUnionCases(unionTyp, bindingFlags)
            |> Array.tryFind (fun unionCase -> unionCase.Name = typ.Name)
        | (IsUnion unionTyp, _) ->
            // Get the only union case of the singleton discriminated union.
            FSharpType.GetUnionCases(unionTyp, bindingFlags)
            |> (function [| unionCase |] -> Some unionCase | _ -> None)
        | _ -> None

        // Only return the union case if it is actually a non-null union case.
        //   1. For example, the type A = | B is a singleton discriminated union, where B is a null
        //      union case because it has no fields.
        //   2. Additionally, a discriminated union may have a nested type for a null union case
        //      depending on the number of cases and whether UseNullAsTrueValue is applied. See
        //      https://github.com/Microsoft/visualfsharp/issues/711 for more details.
        |> (function
            | Some unionCase when unionCase.GetFields().Length > 0 -> Some unionCase
            | _ -> None)

    let mkDelegate (meth:MethodInfo) =
        let types =
            [| for param in meth.GetParameters() do
                   yield param.ParameterType
               yield meth.ReturnType |]
        Expression.GetDelegateType types

    let mapUnionCase (classMap:BsonClassMap) (unionCase:UnionCaseInfo) =
        let fields = unionCase.GetFields()
        let names = fields |> Array.map _.Name

        classMap.SetDiscriminator unionCase.Name
        classMap.SetDiscriminatorIsRequired true

        // Map the constructor of the union case.
        let ctor = FSharpValue.PreComputeUnionConstructorInfo(unionCase, bindingFlags)
        let del = Delegate.CreateDelegate(mkDelegate ctor, ctor)
        classMap.MapCreator(del, names) |> ignore

        // Map each field of the union case.
        fields |> Array.iter (mapMemberNullable classMap)

    interface IClassMapConvention with
        member _.Apply classMap =
            tryGetUnionCase classMap.ClassType
            |> Option.iter (mapUnionCase classMap)
