using System;
using System.Threading.Tasks;

namespace AudioChord
{
    public class SongProcessStatus
    {
        public SongStatus Status { get; private set; }

        protected SongProcessStatus(SongStatus status)
        {
            Status = status;
        }

        public static SongProcessError AsError(SongId id, AggregateException exceptions)
            => new SongProcessError(id, exceptions);

        public static SongProcessResult AsResult(Task<ISong> result)
            => new SongProcessResult(result);
    }
}