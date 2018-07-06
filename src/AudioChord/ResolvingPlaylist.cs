using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioChord
{
    /// <summary>
    /// A playlist that is still in the process of retrieving all songs it has available
    /// </summary>
    public class ResolvingPlaylist 
    {
        public string Id;

        public List<Task<ISong>> Songs { get; private set; } = new List<Task<ISong>>();

        public ResolvingPlaylist(string id)
        {
            Id = id;
        }
    }
}
