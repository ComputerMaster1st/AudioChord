using AudioChord.Collections;
using AudioChord.Processors;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AudioChord
{
    internal class PlaylistProcessor
    {
        private SongCollection songCollection;
        private MusicService musicService;
        private WorkScheduler scheduler;

        private ConcurrentDictionary<SongId, Task<ISong>> allWork = new ConcurrentDictionary<SongId, Task<ISong>>();

        public PlaylistProcessor(SongCollection song, MusicService service)
        {
            songCollection = song;
            musicService = service;
            scheduler = new WorkScheduler();
        }

        public async Task<ResolvingPlaylist> ProcessPlaylist(Uri playlistLocation, IProgress<SongStatus> progress)
        {
            YouTubeProcessor processor = new YouTubeProcessor();
            ResolvingPlaylist playlist = new ResolvingPlaylist(ObjectId.GenerateNewId().ToString());

            Queue<StartableTask<ISong>> backlog = new Queue<StartableTask<ISong>>();

            //WARNING: Only one thread should be able to verify if songs are in the database
            foreach(string id in await processor.ParsePlaylistAsync(playlistLocation))
            {
                // Convert the id to a SongId
                SongId songId = new SongId(YouTubeProcessor.ProcessorPrefix, id);

                // Check if the song already exists in the database
                if (songCollection.CheckAlreadyExists(songId))
                {
                    // Add the song that was already found in the database
                    playlist.Songs.Add(musicService.GetSongAsync(songId));

                    // Report that a song already exists
                    progress?.Report(SongStatus.AlreadyExists);
                }
                else
                {
                    // Song does not exist, add a placeholder that gives back the actual song when done
                    playlist.Songs.Add(

                        // Check if a placeholder already exists
                        allWork.GetOrAdd(songId, (processingSongId) => 
                        {
                            // Always add progress reporting, there is a possibility that somebody who want's reports attaches later
                            StartableTask<ISong> work = new StartableTask<ISong>(() =>
                            { return AddProgressReporting(songCollection.DownloadFromYouTubeAsync(id), progress); });

                            backlog.Enqueue(work);
                            return work.Work;
                        })
                    );
                    
                }   
            }

            // Run the processor on a separate task
            scheduler.CreateWorker(backlog);

            return playlist;
        }

        /// <summary>
        /// Add progress reporting when the task completes
        /// </summary>
        /// <param name="task"></param>
        private Task<ISong> AddProgressReporting(Task<ISong> task, IProgress<SongStatus> progress)
        {
            task.ContinueWith((previous) =>
            {
                progress?.Report(SongStatus.Processed);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            task.ContinueWith((previous) =>
            {
                progress?.Report(SongStatus.Errored);
            }, TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }
    }
}