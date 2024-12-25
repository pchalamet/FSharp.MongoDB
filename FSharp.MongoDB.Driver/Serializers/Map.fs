namespace MongoDB.Driver.Serializers
open MongoDB.Bson.Serialization.Serializers
open MongoDB.Bson.Serialization

type internal MapSerializer<'K, 'V when 'K : comparison>() =
    inherit SerializerBase<Map<'K, 'V>>()
    
    let contentSerializer = BsonSerializer.LookupSerializer(typeof<System.Collections.Generic.IDictionary<'K, 'V>>)

    override _.Serialize(context, _, value) =
        let dict = value |> Map.toSeq |> dict
        contentSerializer.Serialize(context, dict)

    override _.Deserialize(context, args) =
        let dict = contentSerializer.Deserialize(context, args) :?> System.Collections.Generic.IDictionary<'K, 'V>
        dict |> Seq.map (|KeyValue|) |> Map.ofSeq
