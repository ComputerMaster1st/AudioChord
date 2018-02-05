using System;
using System.Collections.Generic;

namespace Shared.Music.Collections.Models
{
    internal class Playlist : List<Guid>
    {
        public Guid Id { get; private set; } = new Guid();
        public string Name { get; private set; }
    }
}