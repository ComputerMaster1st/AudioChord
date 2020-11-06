using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AudioChord
{
    /// <summary>
    /// A custom serializer for converting <see cref="SongId"/> to a string and back
    /// </summary>
    internal class SongIdSerializer : SerializerBase<SongId>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SongId value)
        {
            IBsonWriter bsonWriter = context.Writer;

            bsonWriter.WriteString(value.ToString());
        }

        public override SongId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            IBsonReader bsonReader = context.Reader;

            return SongId.Parse(bsonReader.ReadString());
        }
    }
}