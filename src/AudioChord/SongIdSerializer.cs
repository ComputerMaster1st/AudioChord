using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AudioChord
{
    /// <summary>
    /// A custom serializer for converting <see cref="SongId"/> to a string and back
    /// </summary>
    class SongIdSerializer : SerializerBase<SongId>
    {
        public SongIdSerializer() : base()
        { }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SongId value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteString(value.ToString());
        }

        public override SongId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            return SongId.Parse(bsonReader.ReadString());
        }
    }
}
