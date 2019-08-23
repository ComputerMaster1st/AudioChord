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
        private readonly Func<Task<TResult>> _work;
        private readonly TaskCompletionSource<TResult> _awaiter = new TaskCompletionSource<TResult>();

        /// <summary>
        /// Waits asynchronously until the job is either completed, faulted, or cancelled
        /// </summary>ver
        public Task<TResult> Work { get; }

        public StartableTask(Func<Task<TResult>> job)
        {
            _work = job;
            Work = _awaiter.Task;
        }

        public Task<TResult> Start()
        {
            // No need to do work when we are cancelled, faulted or completed
            if (_awaiter.Task.IsCompleted)
                return Work;

            //retrieve the work that we need to do
            Task<TResult> task = _work.Invoke();

            //set the result if we successfully got the result back
            task.ContinueWith(previous => { _awaiter.SetResult(previous.Result); },
                TaskContinuationOptions.OnlyOnRanToCompletion);

            //raise an exception is we got an exception
            task.ContinueWith(previous => { _awaiter.SetException(previous.Exception); },
                TaskContinuationOptions.OnlyOnFaulted);

            //set to cancelled if cancelled
            task.ContinueWith(previous => { _awaiter.SetCanceled(); }, TaskContinuationOptions.OnlyOnCanceled);

            return task;
        }

        /// <summary>
        /// Attempt to cancel the execution of this work
        /// </summary>
        internal void Cancel()
        {
            _awaiter.TrySetCanceled();
        }
    }
}