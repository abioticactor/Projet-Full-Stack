using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Generation
{
    [BsonElement("name_fr")]
    public string NameFr { get; set; }

    [BsonElement("name_en")]
    public string NameEn { get; set; }
}
