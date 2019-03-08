using System;
using System.Collections.Generic;
using System.Text;

namespace AudioChord
{
    /// <summary>
    /// A set of tags that are read from / written to an Opus ogg file
    /// </summary>
    public class OpusTags
    {
        public string Comment { get; set; } = string.Empty;
        public IDictionary<string, string> Fields { get; } = new Dictionary<string, string>();

        internal static bool TryParsePacket(byte[] packet, int packetLength, out OpusTags tags)
        {
            tags = null;

            if (packetLength < 8)
                return false;

            if (!"OpusTags".Equals(Encoding.UTF8.GetString(packet, 0, 8)))
                return false;

            OpusTags returnVal = new OpusTags();
            int cursor = 8;
            int nextFieldLength = BitConverter.ToInt32(packet, cursor);
            cursor += 4;
            if (nextFieldLength > 0)
            {
                returnVal.Comment = Encoding.UTF8.GetString(packet, cursor, nextFieldLength);
                cursor += nextFieldLength;
            }

            int numTags = BitConverter.ToInt32(packet, cursor);
            cursor += 4;
            for (int c = 0; c < numTags; c++)
            {
                nextFieldLength = BitConverter.ToInt32(packet, cursor);
                cursor += 4;
                if (nextFieldLength > 0)
                {
                    string tag = Encoding.UTF8.GetString(packet, cursor, nextFieldLength);
                    cursor += nextFieldLength;
                    int eq = tag.IndexOf('=');
                    if (eq > 0)
                    {
                        string key = tag.Substring(0, eq);
                        string val = tag.Substring(eq + 1);
                        returnVal.Fields[key] = val;
                    }
                }
            }

            tags = returnVal;
            return true;
        }
    }
}
