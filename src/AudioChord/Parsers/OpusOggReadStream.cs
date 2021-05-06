using System;
using System.IO;
using System.Text;

namespace AudioChord.Parsers
{
    /// <summary>
    ///     Provides functionality to decode a basic .opus Ogg file, decoding the audio packets individually and returning
    ///     them. Tags are also parsed if present.
    ///     Note that this currently assumes the input file only has 1 elementary stream; anything more advanced than that will
    ///     probably not work.
    /// </summary>
    public class OpusOggReadStream : IDisposable
    {
        private readonly Stream _inputStream;
        private bool _endOfStream;
        private byte[] _nextDataPacket;
        private IPacketProvider _packetProvider;
        private OggContainerReader _reader;

        /// <summary>
        ///     Builds an Ogg file reader that decodes Opus packets from the given input stream, using a
        ///     specified output sample rate and channel count. The given decoder will be used as-is
        ///     and return the decoded PCM buffers directly.
        /// </summary>
        /// <param name="oggFileInput">The input stream for an Ogg formatted .opus file. The stream will be read from immediately</param>
        public OpusOggReadStream(Stream oggFileInput)
        {
            _inputStream = oggFileInput ?? throw new ArgumentNullException("oggFileInput");
            if (!Initialize()) _endOfStream = true;
        }

        /// <summary>
        ///     Gets the tags that were parsed from the OpusTags Ogg packet, or NULL if no such packet was found.
        /// </summary>
        public OpusTags Tags { get; private set; }

        /// <summary>
        ///     Returns true if there is still another data packet to be decoded from the current Ogg stream.
        ///     Note that this decoder currently only assumes that the input has 1 elementary stream with no splices
        ///     or other fancy things.
        /// </summary>
        public bool HasNextPacket => !_endOfStream;

        /// <summary>
        ///     If an error happened either in stream initialization, reading, or decoding, the message will appear here.
        /// </summary>
        public string LastError { get; private set; }

        public void Dispose()
        {
            ((IDisposable) _reader).Dispose();
        }

        /// <summary>
        ///     Reads the next packet from the Ogg stream and returns it.
        ///     If there are no more packets to decode, this returns NULL. If an error occurs, this also returns
        ///     NULL and puts the error message into the LastError field
        /// </summary>
        /// <returns>The next opus audio packet (or frame) in the stream, or NULL</returns>
        public byte[] RetrieveNextPacket()
        {
            if (_nextDataPacket == null || _nextDataPacket.Length == 0)
                _endOfStream = true;

            if (_endOfStream)
                return null;

            byte[] result = _nextDataPacket;

            //Search for the next packet in the stream and queue it up
            QueueNextPacket();
            return result;
        }

        /// <summary>
        ///     Creates an opus decoder and reads from the ogg stream until a data packet is encountered,
        ///     queuing it up for future decoding. Tags are also parsed if they are encountered.
        /// </summary>
        /// <returns>True if the stream is valid and ready to be decoded</returns>
        private bool Initialize()
        {
            try
            {
                _reader = new OggContainerReader(_inputStream, true);
                if (!_reader.Init())
                {
                    LastError = "Could not initialize stream";
                    return false;
                }

                if (_reader.StreamSerials.Length == 0)
                {
                    LastError = "Initialization failed: No elementary streams found in input file";
                    return false;
                }

                int streamSerial = _reader.StreamSerials[0];
                _packetProvider = _reader.GetStream(streamSerial);
                QueueNextPacket();

                return true;
            }
            catch (Exception e)
            {
                LastError = "Unknown initialization error: " + e.Message;
                return false;
            }
        }

        /// <summary>
        ///     Looks for the next opus data packet in the Ogg stream and queues it up.
        ///     If the end of stream has been reached, this does nothing.
        /// </summary>
        private void QueueNextPacket()
        {
            if (_endOfStream) return;

            DataPacket packet = _packetProvider.GetNextPacket();
            if (packet == null || packet.IsEndOfStream)
            {
                _endOfStream = true;
                _nextDataPacket = null;
                return;
            }

            byte[] buffer = new byte[packet.Length];
            packet.Read(buffer, 0, packet.Length);
            packet.Done();

            if (buffer.Length > 8 && "OpusHead" == Encoding.UTF8.GetString(buffer, 0, 8))
            {
                QueueNextPacket();
            }
            else if (buffer.Length > 8 && "OpusTags" == Encoding.UTF8.GetString(buffer, 0, 8))
            {
                if (OpusTags.TryParsePacket(buffer, buffer.Length, out var tags))
                    Tags = tags;

                QueueNextPacket();
            }
            else
            {
                _nextDataPacket = buffer;
            }
        }
    }
}