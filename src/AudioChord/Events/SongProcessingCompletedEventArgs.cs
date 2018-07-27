using System;

namespace AudioChord.Events
{
    public class SongProcessingCompletedEventArgs : EventArgs
    {
        public bool TaskCompleted { get; } = true;
    }
}