namespace MongoDB.Driver.Serializers
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization

type internal ListSerializer<'T>() =
    inherit SerializerBase<List<'T>>()

    let contentSerializer = BsonSerializer.LookupSerializer(typeof<System.Collections.Generic.IList<'T>>)

    override _.Serialize(context, _, value) =
        let list = value |> System.Collections.Generic.List<'T>
        contentSerializer.Serialize(context, list)

    override _.Deserialize(context, args) =
        let list = contentSerializer.Deserialize(context, args) :?> System.Collections.Generic.IList<'T>
        list |> List.ofSeq
