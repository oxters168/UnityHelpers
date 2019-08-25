using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityHelpers
{
    //[Serializable]
    public class TaskWrapper
    {
        public string name { get; private set; }
        //private Task task;
        private CancellationTokenSource cancellationTokenSource;

        //public event TaskCompleteHandler onTaskCompleted;
        //public delegate void TaskCompleteHandler(TaskWrapper caller);

        private TaskWrapper(string _name, CancellationTokenSource _cancellationTokenSource)
        {
            name = _name;
            //task = _task;
            cancellationTokenSource = _cancellationTokenSource;
        }

        public void Cancel()
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();
        }
        public static async Task CreateTask(Action<CancellationTokenSource> action, string name, Action<TaskWrapper> onBegin = null, Action<TaskWrapper> onEnd = null)
        {
            using (var cancelTokenSource = new CancellationTokenSource())
            {
                TaskWrapper tw = new TaskWrapper(name, cancelTokenSource);
                await Task.Run(() => { onBegin?.Invoke(tw); action(cancelTokenSource); onEnd?.Invoke(tw); }, cancelTokenSource.Token);
                //return tw;
            }
        }
        public static async Task CreateTask(Func<Task> task, string name, Action<TaskWrapper> onBegin = null, Action<TaskWrapper> onEnd = null)
        {
            using (var cancelTokenSource = new CancellationTokenSource())
            {
                TaskWrapper tw = new TaskWrapper(name, cancelTokenSource);
                onBegin?.Invoke(tw);
                await Task.Run(task, cancelTokenSource.Token);
                onEnd?.Invoke(tw);
                //return tw;
            }
        }
    }
}