using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Status
{
    [BsonElement("capture_rate")]
    public int CaptureRate { get; set; }

    [BsonElement("is_legendary")]
    public bool IsLegendary { get; set; }

    [BsonElement("is_mythical")]
    public bool IsMythical { get; set; }
}
