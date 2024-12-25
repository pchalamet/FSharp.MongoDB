namespace MongoDB.FSharp

open System
open Microsoft.FSharp.Reflection
open MongoDB.Bson.IO
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

module Serializers =
    open MongoDB.Bson

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

        let contentSerializer = BsonSerializer.LookupSerializer(typeof<System.Collections.Generic.IEnumerable<'T>>)

        override this.Serialize(context, _, value) =
            let list = value :> System.Collections.Generic.IEnumerable<'T>
            contentSerializer.Serialize(context, list)

        override this.Deserialize(context, args) =
            let list = contentSerializer.Deserialize(context, args) :?> System.Collections.Generic.IEnumerable<'T>
            list |> List.ofSeq

    let fsharpType (typ : Type) =
        typ.GetCustomAttributes(typeof<CompilationMappingAttribute>, true) 
        |> Seq.cast<CompilationMappingAttribute>
        |> Seq.map(fun t -> t.SourceConstructFlags)
        |> Seq.tryHead

    let createClassMapSerializer (type': Type) (classMap: BsonClassMap) =
        let concreteType = type'.MakeGenericType(classMap.ClassType)
        let ctor = concreteType.GetConstructor([| typeof<BsonClassMap> |])
        ctor.Invoke([| classMap |]) :?> IBsonSerializer

    type RecordSerializerBase(classMap : BsonClassMap) =
        let classMapSerializer = classMap |> createClassMapSerializer typedefof<BsonClassMapSerializer<_>>

        let getter = 
            match classMap.IdMemberMap with
            | null -> None
            | mm -> Some(mm.Getter)

        let idProvider = classMapSerializer :?> IBsonIdProvider

        member val _ClassMapSerializer = classMapSerializer
        
        interface IBsonSerializer with
            member _.ValueType = classMap.ClassType
            
            member _.Serialize(context, args, value) = classMapSerializer.Serialize(context, args, value)
            member _.Deserialize(context, args) = classMapSerializer.Deserialize(context, args)


        interface IBsonDocumentSerializer  with
            member this.TryGetMemberSerializationInfo(memberName, serializationInfo) = 
                let m = classMap.AllMemberMaps |> Seq.tryFind (fun x -> x.MemberName = memberName)
                match m with
                | Some(x) ->
                    serializationInfo <- BsonSerializationInfo(x.ElementName, x.GetSerializer(), x.MemberType)
                    true        
                | None -> 
                    raise <| ArgumentOutOfRangeException($"Class has no member called %s{memberName}")
                

        interface IBsonIdProvider with
            member this.GetDocumentId(document : Object, id : Object byref, nominalType : Type byref, idGenerator : IIdGenerator byref) =
                match getter with
                | Some(i) -> 
                    id <- i.DynamicInvoke(([document] |> Array.ofList))
                    idProvider.GetDocumentId(document, ref id, ref nominalType, ref idGenerator)
                | None -> false

            member this.SetDocumentId(document : Object, id : Object) = idProvider.SetDocumentId(document, id)

    type RecordSerializer<'T>(classMap : BsonClassMap) =
        inherit RecordSerializerBase(classMap)
        
        member private my.Serializer = my._ClassMapSerializer :?> IBsonSerializer<'T>

        interface IBsonSerializer<'T> with
            member my.Serialize(context: BsonSerializationContext, args: BsonSerializationArgs, value: 'T) =
                my.Serializer.Serialize(context, args, value)
            member my.Deserialize(context, args) = my.Serializer.Deserialize(context, args)
    
    type UnionCaseSerializer<'T>() =
        inherit SerializerBase<'T>()

        let readItems context args (types : Type seq) =
            types |> Seq.fold(fun state t ->
                let serializer = BsonSerializer.LookupSerializer(t)
                let item = serializer.Deserialize(context, args)
                item :: state
            ) []
            |> Seq.toArray |> Array.rev

        override this.Serialize(context, args, value) =
            let writer = context.Writer
            writer.WriteStartDocument()
            let info, values = FSharpValue.GetUnionFields(value, args.NominalType)
            writer.WriteName("_t")
            writer.WriteString(info.Name)
            writer.WriteName("_v")
            writer.WriteStartArray()
            values |> Seq.zip(info.GetFields()) |> Seq.iter (fun (field, value) ->
                let itemSerializer = BsonSerializer.LookupSerializer(field.PropertyType)
                itemSerializer.Serialize(context, args, value)
            )
            writer.WriteEndArray()
            writer.WriteEndDocument()
                
        override this.Deserialize(context, args) =
            let reader = context.Reader
            reader.ReadStartDocument()
            reader.ReadName("_t")
            let typeName = reader.ReadString()
            let unionType = 
                FSharpType.GetUnionCases(args.NominalType) 
                |> Seq.where (fun case -> case.Name = typeName) |> Seq.head
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

    let optionSerializer (typ: Type) =
        if typ.IsGenericType && typ.GetGenericTypeDefinition() = typedefof<Option<_>> then
            match typ.GetGenericArguments() with
            | [| t |] as args ->
                let serializer = makeGenericType<OptionSerializer<_>>
                    // if t.IsValueType then makeGenericType<OptionValueSerializer<_>>
                    // else makeGenericType<OptionReferenceSerializer<_>>

                args
                |> serializer
                |> createInstance<IBsonSerializer>
                |> Some
            | _ -> None
        else
            None

    let unionCaseSerializer typ =
        let gen = makeGenericType<UnionCaseSerializer<_>> >> createInstance<IBsonSerializer>
        gen [| typ |] |> Some

    type FsharpSerializationProvider(useOptionNull) =
        let serializers =
          seq {
              if useOptionNull then yield SourceConstructFlags.SumType, optionSerializer
              yield SourceConstructFlags.ObjectType, mapSerializer
              yield SourceConstructFlags.SumType, listSerializer
              yield SourceConstructFlags.SumType, unionCaseSerializer
          } |> List.ofSeq

        interface IBsonSerializationProvider with
            member this.GetSerializer(typ : Type) =
                let tp = fsharpType typ
                printfn $"FSHARPTYPE {typ.FullName} ==> {tp}"

                let serializer =
                    match fsharpType typ with
                    | Some flag ->
                        serializers |> Seq.filter (fst >> (=) flag)
                                    |> Seq.map snd
                                    |> Seq.fold (fun result s -> result |> Option.orElseWith (fun _ -> s typ)) None
                    | _ -> None
                match serializer with
                | Some serializer -> serializer
                | _ -> null

    let mutable isRegistered = false
    
    type RegistrationOption = {
        UseOptionNull: bool
    }
    let defaultRegistrationOption = { UseOptionNull=true }

    /// Registers all F# serializers
    let RegisterWithOptions(opt) =
        if not isRegistered then
            BsonSerializer.RegisterSerializationProvider(FsharpSerializationProvider(opt.UseOptionNull))
            isRegistered <- true

type Serializers() =
    static member Register(?opts: Serializers.RegistrationOption) =
        Serializers.RegisterWithOptions(opts |> Option.defaultValue Serializers.defaultRegistrationOption)
