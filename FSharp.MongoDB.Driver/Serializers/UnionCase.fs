namespace MongoDB.Driver.Serializers
open System
open MongoDB.Bson.IO
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization
open Microsoft.FSharp.Reflection




type internal UnionCaseSerializer<'T>() =
    inherit SerializerBase<'T>()

    let readItems context args (types : Type seq) =
        types
        |> Seq.fold (fun state t ->
            let serializer = BsonSerializer.LookupSerializer(t)
            let item = serializer.Deserialize(context, args)
            item :: state) []
        |> Seq.toArray |> Array.rev

    override _.Serialize(context, args, value) =
        let info, values = FSharpValue.GetUnionFields(value, args.NominalType)
        let writer = context.Writer
        writer.WriteStartDocument()
        writer.WriteName(info.Name)
        writer.WriteStartArray()
        values
        |> Seq.zip (info.GetFields()) 
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
            |> Seq.find (fun case -> case.Name = typeName)
        reader.ReadStartArray()
        let items = readItems context args (unionType.GetFields() |> Seq.map _.PropertyType)
        reader.ReadEndArray()
        reader.ReadEndDocument()
        FSharpValue.MakeUnion(unionType, items) :?> 'T

