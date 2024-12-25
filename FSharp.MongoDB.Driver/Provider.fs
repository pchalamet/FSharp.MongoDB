module FSharp.MongoDB.Driver
open System
open MongoDB.Bson.Serialization
open MongoDB.Driver.Serializers
open FSharp.Helpers

module private Provider =

    let private getGenericArgumentOf baseType (typ: Type) =
        if typ.IsGenericType && typ.GetGenericTypeDefinition() = baseType
        then Some <| typ.GetGenericArguments()
        else None

    let inline private createInstance<'T> typ = Activator.CreateInstance(typ) :?> 'T
    let inline private makeGenericType<'T> typ = typedefof<'T>.MakeGenericType typ

    let specificSerializer<'nominal,'serializer> =
        getGenericArgumentOf typedefof<'nominal> >> Option.map (makeGenericType<'serializer> >> createInstance<IBsonSerializer>)
    let listSerializer typ = typ |> specificSerializer<List<_>, ListSerializer<_>>
    let mapSerializer typ = typ |> specificSerializer<Map<_, _>, MapSerializer<_, _>>
    let optionSerializer typ = typ |> specificSerializer<Option<_>, OptionSerializer<_>>
    let valueOptionSerializer typ = typ |> specificSerializer<ValueOption<_>, ValueOptionSerializer<_>>

    let unionCaseSerializer typ =
        let gen = makeGenericType<UnionCaseSerializer<_>> >> createInstance<IBsonSerializer>
        gen [| typ |] |> Some



    type internal FSharpSerializationProvider() =
        let serializers =
            [ SourceConstructFlags.SumType, optionSerializer
              SourceConstructFlags.SumType, valueOptionSerializer
              SourceConstructFlags.ObjectType, mapSerializer
              SourceConstructFlags.SumType, listSerializer
              SourceConstructFlags.SumType, unionCaseSerializer ]

        interface IBsonSerializationProvider with
            member _.GetSerializer(typ : Type) =
                match fsharpType typ with
                | Some flag ->
                    serializers
                    |> List.filter (fst >> (=) flag)
                    |> List.map snd
                    |> List.fold (fun result s -> result |> Option.orElseWith (fun _ -> s typ)) None
                | _ -> None
                |> Option.toObj


let mutable private isRegistered = false
let Register() =
    if not isRegistered then
        BsonSerializer.RegisterSerializationProvider(Provider.FSharpSerializationProvider())
        isRegistered <- true
