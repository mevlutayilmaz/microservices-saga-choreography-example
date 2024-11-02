using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stock.API.Entities
{
    public class Stock
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement(Order = 0)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement(Order = 1)]
        public string ProductId { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        [BsonElement(Order = 2)]
        public int Count { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement(Order = 3)]
        public DateTime CreatedDate { get; set; }
    }
}
