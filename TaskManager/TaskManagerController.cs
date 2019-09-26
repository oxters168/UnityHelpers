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
        private static TaskManagerController taskManagerControllerInScene;
        private List<TaskWrapper> tasks = new List<TaskWrapper>();
        private List<TaskWrapper> runningTasks = new List<TaskWrapper>();
        private List<Action> actions = new List<Action>();

        public bool showDebugMessages;

        private void Awake()
        {
            if (!taskManagerControllerInScene)
                taskManagerControllerInScene = this;
        }
        async void Update()
        {
            if (tasks.Count > 0 && runningTasks.Count < maxConcurrentTasks)
            {
                int i = tasks.Count - 1;
                TaskWrapper currentTask = tasks[i];
                tasks.RemoveAt(i);
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

        public static void RunAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            taskManagerControllerInScene.actions.Insert(0, action);
        }
        public static TaskWrapper RunActionAsync(string name, Action<CancellationTokenSource> action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");
            if (HasTask(name))
                throw new InvalidOperationException("A task already exists with the name " + name);

            TaskWrapper tw = new TaskWrapper(name, action);
            taskManagerControllerInScene.tasks.Insert(0, tw);
            return tw;
        }
        public static TaskWrapper RunActionAsync(string name, Func<Task> action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");
            if (HasTask(name))
                throw new InvalidOperationException("A task already exists with the name " + name);

            TaskWrapper tw = new TaskWrapper(name, action);
            taskManagerControllerInScene.tasks.Insert(0, tw);
            return tw;
        }
        public static TaskWrapper RunActionAsync(Action<CancellationTokenSource> action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            TaskWrapper tw = new TaskWrapper("", action);
            taskManagerControllerInScene.tasks.Insert(0, tw);
            return tw;
        }
        public static TaskWrapper RunActionAsync(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            TaskWrapper tw = new TaskWrapper("", action);
            taskManagerControllerInScene.tasks.Insert(0, tw);
            return tw;
        }
        public static void CancelTask(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");

            if (taskManagerControllerInScene.showDebugMessages)
                Debug.Log("Cancelling task " + name);

            var runningTasks = taskManagerControllerInScene.runningTasks;
            TaskWrapper task = runningTasks.FirstOrDefault(checkedTask => checkedTask.name.Equals(name, StringComparison.Ordinal));
            if (task == null || !task.name.Equals(name, StringComparison.Ordinal))
            {
                var queuedTasks = taskManagerControllerInScene.tasks;
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

            if (taskManagerControllerInScene.tasks.Contains(task))
            {
                taskManagerControllerInScene.tasks.Remove(task);
            }
            else if (taskManagerControllerInScene.runningTasks.Contains(task))
            {
                task.Cancel();
                taskManagerControllerInScene.runningTasks.Remove(task);
            }
            else
                Debug.LogError("Cannot cancel " + task.name + " since it is not queued or running");
        }
        public static bool HasTask(string name)
        {
            var self = taskManagerControllerInScene;
            bool contains = false;
            if (self.tasks.Exists(item => item.name.Equals(name, StringComparison.Ordinal)))
                contains = true;
            else if (self.runningTasks.Exists(item => item.name.Equals(name, StringComparison.Ordinal)))
                contains = true;
            return contains;
        }
    }
}