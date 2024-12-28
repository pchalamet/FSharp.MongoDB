namespace CSharpDataModels;
using MongoDB.Bson;

public record Pair
{
    public required int First { get; init; }
    public required string? Second { get; init; }
}


public record Value
{
    public record IntValue(int Value) : Value;
    public record StringValue(string Value) : Value;
    public record PairValue(Pair Value) : Value;
}


public record RecordDataModel
{
    public ObjectId Id { get; init; }

    public required int Int { get; init; }
    public int? IntOpt { get; init; }

    public required string String { get; init; }
    public string? StringOpt { get; init; }

    public required int[] Array { get; init; }
    public int[]? ArrayOpt { get; init; }

    public required Value Value { get; init; }
    public Value? ValueOpt { get; init; }

    public required Value[] ValueArray { get; init; }
    public Value[]? ValueArrayOpt { get; init; }

    public required Pair Record { get; init; }
    public Pair? RecordOpt { get; init; }

    public required Dictionary<string, int> Map { get; init; }
}
