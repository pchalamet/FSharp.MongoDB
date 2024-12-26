namespace MongoDB.Driver.Serializers
open System
open MongoDB.Bson.IO
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization
open Microsoft.FSharp.Reflection




type internal UnionCaseSerializer<'T>() =
    inherit SerializerBase<'T>()

    override _.Serialize(context, args, value) =
        let info, values = FSharpValue.GetUnionFields(value, args.NominalType)
        let writer = context.Writer
        writer.WriteStartDocument()
        writer.WriteString("_t", info.Name)
        values
        |> Seq.zip (info.GetFields()) 
        |> Seq.iter (fun (field, value) ->
            let itemSerializer = BsonSerializer.LookupSerializer(field.PropertyType)
            writer.WriteName(field.Name)
            itemSerializer.Serialize(context, args, value))
        writer.WriteEndDocument()

    override _.Deserialize(context, args) =
        let reader = context.Reader
        reader.ReadStartDocument()
        let typeName = reader.ReadString("_t")
        let unionType =
            FSharpType.GetUnionCases(args.NominalType) 
            |> Seq.find (fun case -> case.Name = typeName)
        let items =
            unionType.GetFields()
            |> Array.map (fun prop ->
                let serializer = BsonSerializer.LookupSerializer(prop.PropertyType)
                reader.ReadName(prop.Name)
                let item = serializer.Deserialize(context, args)
                item)
        reader.ReadEndDocument()
        FSharpValue.MakeUnion(unionType, items) :?> 'T
