using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Stats
{
    [BsonElement("PV")]
    public Stat PV { get; set; }

    [BsonElement("Attaque")]
    public Stat Attaque { get; set; }

    [BsonElement("Défense")]
    public Stat Defense { get; set; }

    [BsonElement("Attaque Spé.")]
    public Stat AttaqueSpe { get; set; }

    [BsonElement("Défense Spé.")]
    public Stat DefenseSpe { get; set; }

    [BsonElement("Vitesse")]
    public Stat Vitesse { get; set; }
}
