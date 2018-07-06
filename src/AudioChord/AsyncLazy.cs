using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AudioChord
{
    public class AsyncLazy<T> : Lazy<System.Threading.Tasks.Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
        base(() => Task.Factory.StartNew(valueFactory))
        { }

        public AsyncLazy(Func<System.Threading.Tasks.Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
        { }

        public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); }
    }
}