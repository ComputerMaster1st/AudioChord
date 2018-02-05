using System;
using System.Collections.Generic;

namespace Shared.Music.Collections.Models
{
    internal class Playlist : List<SongMeta>
    {
        public Guid Id { get; private set; } = new Guid();
        public string Name { get; private set; }

        public SongMeta GetSong(Guid Id)
        {
            foreach (SongMeta meta in this)
            {
                if (meta.Id == Id) return meta;
            }

            return null;
        }
    }
}