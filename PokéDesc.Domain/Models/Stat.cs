using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Stat
{
    [BsonElement("value")]
    public int Value { get; set; }

    [BsonElement("name_en")]
    public string NameEn { get; set; }
}
