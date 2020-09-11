using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AudioChord
{
    /// <summary>
    /// Schedules work round-robin. Every playlists gets a turn.
    /// </summary>
    internal class WorkScheduler
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly Task _worker;
        private readonly ConcurrentQueue<(Queue<StartableTask<ISong>> playlist, CancellationToken token)> _playlists =
            new ConcurrentQueue<(Queue<StartableTask<ISong>> playlist, CancellationToken token)>();

        public WorkScheduler()
        {
            _worker = Task.Run(DoWork);
        }

        public void CreateWorker(Queue<StartableTask<ISong>> backlog, CancellationToken cancellationToken)
        {
            if (backlog.Count == 0)
                // No need to allocate the task
                return;
            
            // Register the work that needs to be done
            _playlists.Enqueue((backlog, cancellationToken));
        }

        private async void DoWork()
        {
            while (true)
            {
                if (_playlists.TryDequeue(out (Queue<StartableTask<ISong>> playlist, CancellationToken token) tuple))
                {
                    (Queue<StartableTask<ISong>> playlist, CancellationToken token) = tuple;

                    if (token.IsCancellationRequested)
                    {
                        // Cancel all the upcoming work
                        foreach (StartableTask<ISong> work in playlist)
                        {
                            work.Cancel();
                        }
                    
                        continue;
                    }

                    StartableTask<ISong> job = playlist.Dequeue();
                
                    try
                    {
                        await job.Start();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                
                    if (playlist.Count > 0)
                        // Push the playlist back to the end of the queue
                        _playlists.Enqueue((playlist, token));
                }
                else
                {
                    await Task.Delay(100);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}