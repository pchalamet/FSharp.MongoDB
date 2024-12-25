namespace MongoDB.Driver.Serializers
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization

type internal ListSerializer<'T>() =
    inherit SerializerBase<List<'T>>()

    let contentSerializer = BsonSerializer.LookupSerializer(typeof<'T[]>)

    override _.Serialize(context, _, value) =
        let list = value |> List.toArray
        contentSerializer.Serialize(context, list)

    override _.Deserialize(context, args) =
        let list = contentSerializer.Deserialize(context, args) :?>'T[]
        list |> List.ofArray
