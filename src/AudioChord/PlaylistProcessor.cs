using AudioChord.Collections;
using AudioChord.Processors;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace AudioChord
{
    internal class PlaylistProcessor
    {
        private SongCollection songCollection;
        private MusicService musicService;

        private ConcurrentQueue<Task<ISong>> backLog = new ConcurrentQueue<Task<ISong>>();

        //use 2 tasks for now. this can be changed later on
        private Task[] processors = new Task[2];

        public PlaylistProcessor(SongCollection song, MusicService service)
        {
            songCollection = song;
            musicService = service;

            for(int i = 0; i != processors.Length; i++)
            {
                processors[i] = Task.Factory.StartNew(async () =>
                {
                    while(true)
                    {
                        if(!backLog.TryDequeue(out Task<ISong> work))
                            //try agian after a while (dont wanna overload the lock)
                            await Task.Delay(250);
                        else
                        {
                            //start processing the songs
                            work.Start();
                            await work;
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        public async Task<ResolvingPlaylist> PreProcessPlaylist(Uri playlistLocation)
        {
            YouTubeProcessor processor = new YouTubeProcessor();

            // retrieve all the video id's from the playlist
            List<string> videoIds = await processor.ParsePlaylistAsync(playlistLocation);

            ResolvingPlaylist playlist = new ResolvingPlaylist(ObjectId.GenerateNewId().ToString());

            foreach(string id in videoIds)
            {
                ISong result = await musicService.GetSongAsync(id);
                if (result is null)
                {
                    //song does not exist, add a placeholder that gives back the actual song when done

                    Task<ISong> work = new Task<ISong>(() =>
                    {
                        return songCollection.DownloadFromYouTubeAsync(id)
                            .GetAwaiter()
                            .GetResult();
                        //start the work for the next upcoming tasks
                    });

                    playlist.Songs.Add(work);
                    backLog.Enqueue(work);
                }
                else
                {
                    //add the song that was already found in the database
                    playlist.Songs.Add(Task.FromResult(result));
                }   
            }

            return playlist;
        }

    }
}