using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AudioChord
{
    internal class WorkScheduler
    {
        private readonly List<Task> _workers = new List<Task>();

        public void CreateWorker(Queue<StartableTask<ISong>> backlog, CancellationToken cancellationToken)
        {
            if (backlog.Count == 0)
                // No need to allocate the task
                return;

            // Task.Factory.StartNew() has some weird settings according to https://blog.stephencleary.com/2013/08/startnew-is-dangerous.html
            // when using async (NOT while using tasks for parallel code) it's better to use Task.Run

            // The "longrunning" flag is not needed since the CLR is smart enough to mark a task as longrunning
            // if it's taking longer than 0.5 secs
            // ReSharper disable once MethodSupportsCancellation
            Task worker = Task.Run(async () =>
            {
                while (backlog.Count > 0)
                {
                    var work = backlog.Dequeue();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // current work will not be done, itll be cancelled instead
                        work.Cancel();
                        // Continue the loop, the remaining work will be marked as cancelled
                        continue;
                    }

                    try
                    {
                        await work.Start();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            });

            _workers.Add(worker);

            // Add a continuation that removes the task when completed
            // ReSharper disable once MethodSupportsCancellation
            worker.ContinueWith(completedTask => _workers.Remove(worker));
        }
    }
}
