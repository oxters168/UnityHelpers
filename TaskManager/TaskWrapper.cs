using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnityHelpers
{
    //[Serializable]
    public class TaskWrapper
    {
        public string name { get; private set; }
        private CancellationTokenSource cancellationTokenSource;

        private Action<CancellationToken> cancellableAction;
        private Func<CancellationToken, Task> funkyTask;

        public bool cancelled { get; private set; }

        /// <summary>
        /// Only create if you are not using TaskManagerController, or else call TaskManagerController's RunActionAsync.
        /// </summary>
        /// <param name="_name">The name of the task</param>
        /// <param name="_cancellableAction">The task itself</param>
        public TaskWrapper(string _name, Action<CancellationToken> _cancellableAction)
        {
            name = _name;
            //task = _task;
            //cancellationTokenSource = _cancellationTokenSource;
            cancellableAction = _cancellableAction;
        }
        /// <summary>
        /// Only create if you are not using TaskManagerController, or else call TaskManagerController's RunActionAsync.
        /// </summary>
        /// <param name="_name">The name of the task</param>
        /// <param name="_funkyTask">The task itself</param>
        public TaskWrapper(string _name, Func<CancellationToken, Task> _funkyTask)
        {
            name = _name;
            funkyTask = _funkyTask;
        }

        /// <summary>
        /// Only call this if you are not using TaskManagerController, or else you can use TaskManagerController's CancelTask function
        /// </summary>
        public void Cancel()
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                UnityEngine.Debug.Log("Sending cancel request through cancellation token source");
                cancellationTokenSource.Cancel();
                cancelled = true;
            }
        }
        /// <summary>
        /// Only call this if you are not using TaskManagerController
        /// </summary>
        /// <param name="onBegin">Action to do when the task begins</param>
        /// <param name="onEnd">Action to do when the task ends</param>
        /// <returns></returns>
        public async Task Start(Action<TaskWrapper> onBegin = null, Action<TaskWrapper> onEnd = null)
        {
            if (cancellableAction != null)
            {
                using (cancellationTokenSource = new CancellationTokenSource())
                {
                    await Task.Run(() => { onBegin?.Invoke(this); cancellableAction(cancellationTokenSource.Token); onEnd?.Invoke(this); }, cancellationTokenSource.Token);
                }
            }
            else if (funkyTask != null)
            {
                using (cancellationTokenSource = new CancellationTokenSource())
                {
                    onBegin?.Invoke(this);
                    await Task.Run(() => { return funkyTask(cancellationTokenSource.Token); }, cancellationTokenSource.Token);
                    onEnd?.Invoke(this);
                }
            }
        }
    }
}