namespace AudioChord
{
    public class MusicServiceConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; } = "localhost";
        public string Database { get; internal set; } = "sharedmusic";
        public bool EnableResync { get; set; } = false;
    }
}