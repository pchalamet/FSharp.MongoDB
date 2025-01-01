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

namespace MongoDB.Bson.Serialization

open System.Reflection
open Microsoft.FSharp.Reflection

/// Convenience functions for interacting with F# types.
module private Helpers =

    let bindingFlags = BindingFlags.Public ||| BindingFlags.NonPublic

#if !NETSTANDARD2_1
    let nrtContext = NullabilityInfoContext()
#endif

    /// <summary>
    /// Returns <c>Some typ</c> when <c>pred typ</c> returns true, and <c>None</c> when
    /// <c>pred typ</c> returns false.
    /// </summary>
    let private whenType pred (typ:System.Type) =
        if pred typ then Some typ
        else None

    /// <summary>
    /// Returns <c>Some typ</c> when <c>typ</c> is a record type, and <c>None</c> otherwise.
    /// </summary>
    let (|IsRecord|_|) = function
        | Null -> None
        | NonNull typ ->
            let isRecord typ = FSharpType.IsRecord(typ, bindingFlags)
            whenType isRecord typ

    /// <summary>
    /// Returns <c>Some typ</c> when <c>typ</c> is a top-level union type or when it represents a
    /// particular union case, and <c>None</c> otherwise.
    /// </summary>
    let (|IsUnion|_|) = function
        | Null -> None
        | NonNull typ ->
            let isUnion typ = FSharpType.IsUnion(typ, bindingFlags)
            whenType isUnion typ

    /// <summary>
    /// Returns true if <c>typ</c> is a generic type with defintion <c>'GenericType</c>.
    /// </summary>
    let private isGeneric<'GenericType> (typ:System.Type) =
        typ.IsGenericType && typ.GetGenericTypeDefinition() = typedefof<'GenericType>

    /// <summary>
    /// Returns <c>Some typ</c> when <c>typ</c> represents a list, and <c>None</c> otherwise.
    /// </summary>
    let (|IsList|_|) typ = whenType isGeneric<_ list> typ

    /// <summary>
    /// Returns <c>Some typ</c> when <c>typ</c> represents a map, and <c>None</c> otherwise.
    /// </summary>
    let (|IsMap|_|) typ = whenType isGeneric<Map<_, _>> typ

    /// <summary>
    /// Returns <c>Some typ</c> when <c>typ</c> is an option type, and <c>None</c> otherwise.
    /// </summary>
    let (|IsOption|_|) typ = whenType isGeneric<_ option> typ

    /// <summary>
    /// Returns <c>Some typ</c> when <c>typ</c> is an voption type, and <c>None</c> otherwise.
    /// </summary>
    let (|IsValueOption|_|) typ = whenType isGeneric<_ voption> typ

    /// <summary>
    /// Returns <c>Some typ</c> when <c>typ</c> represents a set, and <c>None</c> otherwise.
    /// </summary>
    let (|IsSet|_|) typ = whenType isGeneric<Set<_>> typ

    /// <summary>
    /// Creates a generic type <c>'T</c> with generic arguments <c>args</c>.
    /// </summary>
    let mkGeneric<'T> args = typedefof<'T>.MakeGenericType args

    /// <summary>
    /// Creates a generic type <c>'T</c> using the generic arguments of <c>typ</c>.
    /// </summary>
    let mkGenericUsingDef<'T> (typ:System.Type) = typ.GetGenericArguments() |> mkGeneric<'T>

    let mkMemberNullable (memberMap: BsonClassMap) (propertyInfo: PropertyInfo) =
        let memberMap = memberMap.MapMember(propertyInfo)
#if !NETSTANDARD2_1
        let nrtInfo = nrtContext.Create(propertyInfo)
        if nrtInfo.WriteState = NullabilityState.Nullable then
            memberMap.SetDefaultValue(null).SetIsRequired(false) |> ignore
#else
        ()
#endif
