using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioChord
{
    internal class WorkScheduler
    {
        private List<Task> workers = new List<Task>();

        public void CreateWorker(Queue<StartableTask<ISong>> backlog)
        {
            if (backlog.Count == 0)
                // No need to allocate the task
                return;

            // Task.Factory.StartNew() has some weird settings according to https://blog.stephencleary.com/2013/08/startnew-is-dangerous.html
            // when using async (NOT while using tasks for parallel code) it's better to use Task.Run

            // The "longrunning" flag is not needed since the CLR is smart enough to mark a task as longrunning
            // if it's taking longer than 0.5 secs
            Task worker = Task.Run(async () =>
            {
                while (backlog.Count > 0)
                {
                    if (backlog.TryDequeue(out var work))
                    {
                        try
                        {
                            await work.Start();
                        }
                        catch (System.Exception)
                        { }
                    }  
                }
            });
            workers.Add(worker);

            // Add a continuation that removes the task when completed
            worker.ContinueWith((completedTask) =>
            {
                workers.Remove(worker);
            });
        }
    }
}
