namespace MongoDB.Bson.Serialization.Conventions

open MongoDB.Bson.Serialization.Conventions
open System.Reflection

/// <summary>
/// Convention for F# option/voption types that writes the value in the <c>Some</c> case and omits the field
/// in the <c>None</c> case.
/// </summary>
type NullableReferenceTypeConvention() =
    inherit ConventionBase()

    static let nrtContext = NullabilityInfoContext()

    interface IMemberMapConvention with
        member _.Apply memberMap =
            printfn $"Considering nullability for {memberMap.ElementName} for {memberMap.MemberInfo.DeclaringType}"
            match memberMap.MemberInfo with
            | :? PropertyInfo as propInfo -> 
                let nrtInfo = nrtContext.Create(propInfo)
                printfn $"Info = {nrtInfo.WriteState}"
                if nrtInfo.WriteState = NullabilityState.Nullable then
                    printfn $"Enabling null"
                    memberMap.SetDefaultValue(null) |> ignore
                else
                    printfn $"Not null"
            | :? FieldInfo as fieldInfo ->
                let nrtInfo = nrtContext.Create(fieldInfo)
                if nrtInfo.WriteState = NullabilityState.Nullable then
                    printfn $"Enabling null"
                    memberMap.SetDefaultValue(null) |> ignore
                else
                    printfn $"Not null"
            | _ -> ()
