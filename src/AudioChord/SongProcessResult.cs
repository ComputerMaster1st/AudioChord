using System.Threading.Tasks;

namespace AudioChord
{
    public class SongProcessResult : SongProcessStatus
    {
        public Task<ISong> Result { get; private set; }

        public SongProcessResult(Task<ISong> result) : base(SongStatus.Processed)
        {
            Result = result;
        }
    }
}