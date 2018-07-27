using System;
using System.Threading.Tasks;

namespace AudioChord
{
    /// <summary>
    /// Hold off work (wrapped in a task) to do later on
    /// </summary>
    /// <typeparam name="TResult">The return type of the task</typeparam>
    public class StartableTask<TResult>
    {
        private Func<Task<TResult>> work;
        private TaskCompletionSource<TResult> awaiter = new TaskCompletionSource<TResult>();

        /// <summary>
        /// Waits asynchronously until the job is either completed, faulted, or cancelled
        /// </summary>ver
        public Task<TResult> Work { get; }

        public StartableTask(Func<Task<TResult>> job)
        {
            work = job;
            Work = awaiter.Task;
        }

        public Task<TResult> Start()
        {
            //retrieve the work that we need to do
            Task<TResult> task = work.Invoke();

            //set the result if we successfully got the result back
            task.ContinueWith((previous) => { awaiter.SetResult(previous.Result); }, TaskContinuationOptions.OnlyOnRanToCompletion);

            //raise an exception is we got an exception
            task.ContinueWith((previous) => { awaiter.SetException(previous.Exception); }, TaskContinuationOptions.OnlyOnFaulted);

            //set to cancelled if cancelled
            task.ContinueWith((previous) => { awaiter.SetCanceled(); }, TaskContinuationOptions.OnlyOnCanceled);

            return task;
        }
    }
}
