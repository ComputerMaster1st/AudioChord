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

        private ConcurrentQueue<StartableTask<ISong>> backLog = new ConcurrentQueue<StartableTask<ISong>>();

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
                        if(!backLog.TryDequeue(out StartableTask<ISong> work))
                            //try agian after a while (dont wanna overload the lock)
                            await Task.Delay(250);
                        else
                        {
                            //process the song
                            await work.Start();
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        public async Task<ResolvingPlaylist> PreProcessPlaylist(Uri playlistLocation, IProgress<SongStatus> progress)
        {
            YouTubeProcessor processor = new YouTubeProcessor();

            // retrieve all the video id's from the playlist
            List<string> videoIds = await processor.ParsePlaylistAsync(playlistLocation);

            ResolvingPlaylist playlist = new ResolvingPlaylist(ObjectId.GenerateNewId().ToString());

            foreach(string id in videoIds)
            {
                ISong result = await musicService.GetSongAsync(new SongId(YouTubeProcessor.ProcessorPrefix, id));
                if (result is null)
                {
                    //song does not exist, add a placeholder that gives back the actual song when done
                    StartableTask<ISong> work = new StartableTask<ISong>(() =>
                    {
                        Task<ISong> task = songCollection.DownloadFromYouTubeAsync(id);

                        // Add progress reporting when the task completes
                        task.ContinueWith((previous) =>
                        {
                            progress?.Report(SongStatus.Processed);
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);

                        task.ContinueWith((previous) => 
                        {
                            progress?.Report(SongStatus.Errorred);
                        }, TaskContinuationOptions.OnlyOnFaulted);

                        return task;
                    });

                    playlist.Songs.Add(work.Work);
                    backLog.Enqueue(work);
                }
                else
                {
                    //add the song that was already found in the database
                    playlist.Songs.Add(Task.FromResult(result));

                    // Report that a song already exists
                    progress?.Report(SongStatus.AlreadyExists);
                }   
            }

            return playlist;
        }

    }
}