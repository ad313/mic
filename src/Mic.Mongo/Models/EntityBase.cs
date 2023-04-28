using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mic.Mongo.Models
{
    /// <summary>
    /// 基类
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// 映射Mongo生成的Id
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }
    }
}