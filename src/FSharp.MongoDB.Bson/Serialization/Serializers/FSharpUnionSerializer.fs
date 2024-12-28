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

namespace MongoDB.Bson.Serialization.Serializers

open Microsoft.FSharp.Reflection
open MongoDB.Bson.IO
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Conventions
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization.Helpers

/// <summary>
/// Serializer for F# discriminated unions.
/// </summary>
type FSharpUnionSerializer<'T>() =
    inherit SerializerBase<'T>()

    let typ = typeof<'T>
    let unionCases =
        FSharpType.GetUnionCases(typ, bindingFlags)
        |> Seq.map (fun unionCase -> (unionCase.Name, unionCase))
        |> dict

    let serializers =
        let mkClassMapSerializer (caseType:System.Type) =
            let classMap = BsonClassMap.LookupClassMap caseType
            let serializerType = typedefof<BsonClassMapSerializer<_>>.MakeGenericType [| caseType |]
            System.Activator.CreateInstance(serializerType, classMap) :?> IBsonSerializer

        // 8.5.4. Compiled Form of Union Types for Use from Other CLI Languages
        //   A compiled union type U has [o]ne CLI nested type U.C for each non-null union case C.
        //   However, a compiled union type that has only one case does not have a nested type.
        //   Instead, the union type itself plays the role of the case type.
        if unionCases.Count = 1 then
            unionCases.Keys
            |> Seq.take 1
            |> Seq.map (fun caseName -> (caseName, mkClassMapSerializer typ))
        else
            typ.GetNestedTypes bindingFlags
            |> Seq.filter ((|IsUnion|_|) >> Option.isSome)
            |> Seq.map (fun caseType -> (caseType.Name, mkClassMapSerializer caseType))
        |> dict

    let discriminatorConvention = ScalarDiscriminatorConvention "_t"

    let rec getDiscriminator (reader:IBsonReader) =
        let fieldName = reader.ReadName()
        if fieldName = discriminatorConvention.ElementName then
            reader.ReadString()
        else
            reader.SkipValue()
            getDiscriminator reader

    override _.Serialize (context, args, value) =
        let writer = context.Writer
        let (unionCase, fields) = FSharpValue.GetUnionFields(value, typ, bindingFlags)

        match fields with
        | [| |] ->
            // Handle when `typ` is a null union case, i.e. the union case has no fields.
            writer.WriteStartDocument()
            writer.WriteName discriminatorConvention.ElementName
            writer.WriteString unionCase.Name
            writer.WriteEndDocument()
        | _ ->
            // Otherwise, defer serialization to the class map.
            let serializer = serializers.[unionCase.Name]
            serializer.Serialize(context, args, value)

    override _.Deserialize (context, args) =
        let reader = context.Reader
        let mark = reader.GetBookmark()
        reader.ReadStartDocument()

        let caseName = getDiscriminator reader
        let unionCase = unionCases.[caseName]
        match unionCase.GetFields() with
        | [| |] ->
            // Handle when `typ` is a null union case, i.e. the union case has no fields.
            reader.ReadEndDocument()
            FSharpValue.MakeUnion(unionCase, [| |], bindingFlags) :?> 'T
        | _ ->
            // Otherwise, defer deserialization to the class map.
            reader.ReturnToBookmark mark
            let serializer = serializers.[unionCase.Name]
            serializer.Deserialize(context, args) :?> 'T
