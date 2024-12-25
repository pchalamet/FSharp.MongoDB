namespace MongoDB.Driver.Serializers
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization

type internal ListSerializer<'T>() =
    inherit SerializerBase<List<'T>>()

    let contentSerializer = BsonSerializer.LookupSerializer(typeof<System.Collections.Generic.IEnumerable<'T>>)

    override _.Serialize(context, _, value) =
        contentSerializer.Serialize(context, value)

    override _.Deserialize(context, args) =
        let list = contentSerializer.Deserialize(context, args) :?> System.Collections.Generic.IEnumerable<'T>
        list |> List.ofSeq
