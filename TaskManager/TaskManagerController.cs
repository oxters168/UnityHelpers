using UnityEngine;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace UnityHelpers
{
    public class TaskManagerController : MonoBehaviour
    {
        public int maxConcurrentTasks = 5;
        public bool showDebugMessages;

        private static bool taskManagerCreated;
        private static TaskManagerController taskManagerControllerInScene;

        private static List<TaskWrapper> queuedTasks = new List<TaskWrapper>();
        private static List<TaskWrapper> runningTasks = new List<TaskWrapper>();
        private static List<Action> actions = new List<Action>();

        async void Update()
        {
            if (queuedTasks.Count > 0 && runningTasks.Count < maxConcurrentTasks)
            {
                int i = queuedTasks.Count - 1;
                TaskWrapper currentTask = queuedTasks[i];
                queuedTasks.RemoveAt(i);
                await currentTask.Start((task) =>
                {
                    if (showDebugMessages)
                        Debug.Log("Running task " + task.name);
                    runningTasks.Add(task);
                }, (task) =>
                {
                    if (showDebugMessages)
                        Debug.Log("Completed task " + task.name);
                    runningTasks.Remove(task);
                });
            }
            if (actions.Count > 0)
            {
                int i = actions.Count - 1;
                Action action = actions[i];
                actions.RemoveAt(i);
                Debug.Assert(action != null);
                action?.Invoke();
            }
        }

        private static void CheckManagerExists()
        {
            if (!taskManagerCreated)
            {
                taskManagerCreated = true;
                taskManagerControllerInScene = new GameObject("Task Manager").AddComponent<TaskManagerController>();
            }
        }

        public static void RunAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            actions.Insert(0, action);
            CheckManagerExists();
        }

        public static TaskWrapper CreateTask(string name, Action<CancellationToken> action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            return new TaskWrapper(name, action);
        }
        public static TaskWrapper CreateTask(string name, Func<CancellationToken, Task> action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            return new TaskWrapper(name, action);
        }
        public static TaskWrapper CreateTask(Action<CancellationToken> action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            return new TaskWrapper("", action);
        }
        public static TaskWrapper CreateTask(Func<CancellationToken, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            return new TaskWrapper("", action);
        }

        public static TaskWrapper RunActionAsync(string name, Action<CancellationToken> action)
        {
            TaskWrapper tw = CreateTask(name, action);
            QueueTask(tw);
            return tw;
        }
        public static TaskWrapper RunActionAsync(string name, Func<CancellationToken, Task> action)
        {
            TaskWrapper tw = CreateTask(name, action);
            QueueTask(tw);
            return tw;
        }
        public static TaskWrapper RunActionAsync(Action<CancellationToken> action)
        {
            TaskWrapper tw = CreateTask(action);
            QueueTask(tw);
            return tw;
        }
        public static TaskWrapper RunActionAsync(Func<CancellationToken, Task> action)
        {
            TaskWrapper tw = CreateTask(action);
            QueueTask(tw);
            return tw;
        }
        public static void QueueTask(TaskWrapper task)
        {
            if (HasTask(task))
                throw new InvalidOperationException("The given task is already queued or running");
            else if (!string.IsNullOrEmpty(task.name) && HasTask(task.name))
                throw new InvalidOperationException("A queued or running task already exists with the name " + task.name);

            queuedTasks.Insert(0, task);
            CheckManagerExists();
        }

        public static void CancelTask(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");

            if (taskManagerControllerInScene.showDebugMessages)
                Debug.Log("Cancelling task " + name);

            //var runningTasks = taskManagerControllerInScene.runningTasks;
            TaskWrapper task = runningTasks.FirstOrDefault(checkedTask => checkedTask.name.Equals(name, StringComparison.Ordinal));
            if (task == null || !task.name.Equals(name, StringComparison.Ordinal))
            {
                //var queuedTasks = taskManagerControllerInScene.queuedTasks;
                int taskIndex = queuedTasks.FindIndex(checkedTask => checkedTask.name.Equals(name, StringComparison.Ordinal));
                if (taskIndex >= 0)
                    queuedTasks.RemoveAt(taskIndex);
            }
            else
            {
                task.Cancel();
                runningTasks.Remove(task);
            }
        }
        public static void CancelTask(TaskWrapper task)
        {
            if (task == null)
                throw new ArgumentException("Task cannot be null");

            if (taskManagerControllerInScene.showDebugMessages)
                Debug.Log("Cancelling task " + task.name);

            if (queuedTasks.Contains(task))
            {
                if (taskManagerControllerInScene.showDebugMessages)
                    Debug.Log("Removing " + task.name + " from queued tasks");
                queuedTasks.Remove(task);
            }
            else if (runningTasks.Contains(task))
            {
                if (taskManagerControllerInScene.showDebugMessages)
                    Debug.Log(task.name + " is currently running, attempting to cancel then remove");

                task.Cancel();
                runningTasks.Remove(task);
            }
            else
                Debug.LogError("Cannot cancel " + task.name + " since it is not queued or running");
        }
        public static bool IsQueued(string taskName)
        {
            //if (string.IsNullOrEmpty(taskName))
            //    throw new ArgumentNullException("Task name cannot be null or empty");

            return !string.IsNullOrEmpty(taskName) && queuedTasks.Exists(item => item.name.Equals(taskName, StringComparison.Ordinal));
        }
        public static bool IsQueued(TaskWrapper task)
        {
            //if (task == null)
            //    throw new ArgumentNullException("Task cannot be null");

            return task != null && queuedTasks.Contains(task);
        }
        public static bool IsRunning(string taskName)
        {
            //if (string.IsNullOrEmpty(taskName))
            //    throw new ArgumentNullException("Task name cannot be null or empty");

            return !string.IsNullOrEmpty(taskName) && runningTasks.Exists(item => item.name.Equals(taskName, StringComparison.Ordinal));
        }
        public static bool IsRunning(TaskWrapper task)
        {
            //if (task == null)
            //    throw new ArgumentNullException("Task cannot be null");

            return task != null && runningTasks.Contains(task);
        }
        public static bool HasTask(string taskName)
        {
            //if (string.IsNullOrEmpty(taskName))
            //    throw new ArgumentNullException("Task name cannot be null or empty");

            return !string.IsNullOrEmpty(taskName) && (IsQueued(taskName) || IsRunning(taskName));
        }
        public static bool HasTask(TaskWrapper task)
        {
            //if (task == null)
            //    throw new ArgumentNullException("Task cannot be null");

            return task != null && (IsQueued(task) || IsRunning(task));
        }
    }
}