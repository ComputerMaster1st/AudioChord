using System;

namespace Shared.Music.Collections
{
    internal class PlaylistMeta
    {
        public Guid Id { get; private set; } = new Guid();
        public string Name { get; private set; }
    }
}