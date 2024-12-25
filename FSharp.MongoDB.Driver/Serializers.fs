namespace FSharp.MongoDB.Driver

open System
open Microsoft.FSharp.Reflection
open MongoDB.Bson
open MongoDB.Bson.IO
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

module Serializers =

    type OptionSerializer<'T>() =
        inherit SerializerBase<Option<'T>>()

        override _.Serialize(context, _, value) =
            match value with
            | None ->
                context.Writer.WriteNull()
            | _ -> 
                let contentSerializer = BsonSerializer.LookupSerializer(typeof<'T>)
                contentSerializer.Serialize(context, value.Value)

        override _.Deserialize(context, args) =
            match context.Reader.CurrentBsonType with
            | BsonType.Null ->
                context.Reader.ReadNull()
                None
            | _ ->
                let contentSerializer = BsonSerializer.LookupSerializer(typeof<'T>)
                let obj = contentSerializer.Deserialize(context, args) :?> 'T
                Some obj


    type MapSerializer<'K, 'V when 'K : comparison>() =
        inherit SerializerBase<Map<'K, 'V>>()
        
        let contentSerializer = BsonSerializer.LookupSerializer(typeof<System.Collections.Generic.IDictionary<'K, 'V>>)

        override _.Serialize(context, _, value) =
            let dict = value |> Map.toSeq |> dict
            contentSerializer.Serialize(context, dict)

        override _.Deserialize(context, args) =
            let dict = contentSerializer.Deserialize(context, args) :?> System.Collections.Generic.IDictionary<'K, 'V>
            dict |> Seq.map (|KeyValue|) |> Map.ofSeq


    type ListSerializer<'T>() =
        inherit SerializerBase<List<'T>>()

        let contentSerializer = BsonSerializer.LookupSerializer(typeof<'T[]>)

        override _.Serialize(context, _, value) =
            let list = value |> List.toArray
            contentSerializer.Serialize(context, list)

        override _.Deserialize(context, args) =
            let list = contentSerializer.Deserialize(context, args) :?>'T[]
            list |> List.ofArray


    let fsharpType (typ : Type) =
        typ.GetCustomAttributes(typeof<CompilationMappingAttribute>, true) 
        |> Seq.cast<CompilationMappingAttribute>
        |> Seq.map(fun t -> t.SourceConstructFlags)
        |> Seq.tryHead


    type UnionCaseSerializer<'T>() =
        inherit SerializerBase<'T>()

        let readItems context args (types : Type seq) =
            types
            |> Seq.fold(fun state t ->
                let serializer = BsonSerializer.LookupSerializer(t)
                let item = serializer.Deserialize(context, args)
                item :: state) []
            |> Seq.toArray |> Array.rev

        override _.Serialize(context, args, value) =
            let writer = context.Writer
            writer.WriteStartDocument()
            let info, values = FSharpValue.GetUnionFields(value, args.NominalType)
            writer.WriteName(info.Name)
            writer.WriteStartArray()
            values 
            |> Seq.zip(info.GetFields()) 
            |> Seq.iter (fun (field, value) ->
                let itemSerializer = BsonSerializer.LookupSerializer(field.PropertyType)
                itemSerializer.Serialize(context, args, value))
            writer.WriteEndArray()
            writer.WriteEndDocument()

        override _.Deserialize(context, args) =
            let reader = context.Reader
            reader.ReadStartDocument()
            let typeName = reader.ReadName()
            let unionType = 
                FSharpType.GetUnionCases(args.NominalType) 
                |> Seq.where (fun case -> case.Name = typeName)
                |> Seq.head
            reader.ReadStartArray()
            let items = readItems context args (unionType.GetFields() |> Seq.map(fun f -> f.PropertyType))
            reader.ReadEndArray()
            reader.ReadEndDocument()
            FSharpValue.MakeUnion(unionType, items) :?> 'T


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

    let unionCaseSerializer typ =
        let gen = makeGenericType<UnionCaseSerializer<_>> >> createInstance<IBsonSerializer>
        gen [| typ |] |> Some

    type FsharpSerializationProvider(useOptionNull) =
        let serializers =
            [ if useOptionNull then SourceConstructFlags.SumType, optionSerializer
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

    let mutable isRegistered = false

    type RegistrationOption = { UseOptionNull: bool }
    let defaultRegistrationOption = { UseOptionNull = true }

    // Registers all F# serializers
    let RegisterWithOptions(opt) =
        if not isRegistered then
            BsonSerializer.RegisterSerializationProvider(FsharpSerializationProvider(opt.UseOptionNull))
            isRegistered <- true


type Serializers() =
    static member Register(?opts: Serializers.RegistrationOption) =
        Serializers.RegisterWithOptions(opts |> Option.defaultValue Serializers.defaultRegistrationOption)

