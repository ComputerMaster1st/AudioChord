using System;

namespace AudioChord
{
    public class SongProcessError : SongProcessStatus
    {
        public AggregateException Exceptions { get; private set; }
        public SongId Id { get; private set; }

        public SongProcessError(SongId id, params Exception[] exceptions) : base(SongStatus.Errored)
        {
            Id = id;
            Exceptions = new AggregateException(exceptions);
        }

        public SongProcessError(SongId id, AggregateException exceptions) : base(SongStatus.Errored)
        {
            Id = id;
            Exceptions = exceptions;
        }
    }
}
