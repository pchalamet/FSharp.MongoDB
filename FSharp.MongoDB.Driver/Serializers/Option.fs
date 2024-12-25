namespace MongoDB.Driver.Serializers
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization
open MongoDB.Bson

type internal OptionSerializer<'T>() =
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
