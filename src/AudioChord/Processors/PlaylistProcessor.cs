using AudioChord.Collections;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace AudioChord.Processors
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

        public async Task<ResolvingPlaylist> ProcessPlaylist(Uri playlistLocation, IProgress<SongProcessStatus> progress, CancellationToken token)
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

                    // Increment the count of already existing songs
                    playlist.ExistingSongs++;
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
                            { return AddProgressReporting(songCollection.DownloadFromYouTubeAsync(processingSongId.SourceId), progress, processingSongId); });

                            backlog.Enqueue(work);

                            // Remove the task from the dictionary when it's done
                            work.Work.ContinueWith((task) => { allWork.TryRemove(processingSongId, out _); });

                            return work.Work;
                        })
                    );
                }   
            }

            // Run the processor on a separate task
            scheduler.CreateWorker(backlog, token);

            return playlist;
        }

        /// <summary>
        /// Add progress reporting when the task completes
        /// </summary>
        /// <param name="task"></param>
        private Task<ISong> AddProgressReporting(Task<ISong> task, IProgress<SongProcessStatus> progress, SongId id)
        {
            task.ContinueWith((previous) =>
            {
                progress?.Report(SongProcessStatus.AsResult(previous));
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            task.ContinueWith((previous) =>
            {
                progress?.Report(SongProcessStatus.AsError(id, previous.Exception));
            }, TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }
    }
}