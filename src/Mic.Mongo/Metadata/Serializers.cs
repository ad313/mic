using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mic.Mongo.Metadata
{
    public class BsonDecimalSerializer : SerializerBase<decimal>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, decimal value)
        {
            context.Writer.WriteDecimal128(value);
        }
        public override decimal Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return (decimal)context.Reader.ReadDecimal128();
        }
    }

    public class BsonDecimalNullableSerializer : SerializerBase<decimal?>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, decimal? value)
        {
            if (value != null)
            {
                context.Writer.WriteDecimal128(value.Value);
            }
            else
            {
                context.Writer.WriteNull();
            }
        }

        public override decimal? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType == BsonType.Null)
            {
                return null;
            }
            
            return (decimal?)context.Reader.ReadDecimal128();
        }
    }
}