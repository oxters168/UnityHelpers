﻿using UnityEngine;

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
        public bool showDebugMessages = true;

        private static bool taskManagerCreated;
        private static TaskManagerController taskManagerControllerInScene;

        private static List<TaskWrapper> queuedTasks = new List<TaskWrapper>();
        private static List<TaskWrapper> runningTasks = new List<TaskWrapper>();
        private static List<Action> actions = new List<Action>();

        //private void Awake()
        //{
        //    if (!taskManagerControllerInScene)
        //        taskManagerControllerInScene = this;
        //}
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
        public static TaskWrapper RunActionAsync(string name, Action<CancellationToken> action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");
            if (HasTask(name))
                throw new InvalidOperationException("A task already exists with the name " + name);

            TaskWrapper tw = new TaskWrapper(name, action);
            queuedTasks.Insert(0, tw);
            CheckManagerExists();
            return tw;
        }
        public static TaskWrapper RunActionAsync(string name, Func<CancellationToken, Task> action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");
            if (HasTask(name))
                throw new InvalidOperationException("A task already exists with the name " + name);

            TaskWrapper tw = new TaskWrapper(name, action);
            queuedTasks.Insert(0, tw);
            CheckManagerExists();
            return tw;
        }
        public static TaskWrapper RunActionAsync(Action<CancellationToken> action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            TaskWrapper tw = new TaskWrapper("", action);
            queuedTasks.Insert(0, tw);
            CheckManagerExists();
            return tw;
        }
        public static TaskWrapper RunActionAsync(Func<CancellationToken, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");

            TaskWrapper tw = new TaskWrapper("", action);
            queuedTasks.Insert(0, tw);
            CheckManagerExists();
            return tw;
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
            if (string.IsNullOrEmpty(taskName))
                throw new ArgumentNullException("Task name cannot be null or empty");

            return queuedTasks.Exists(item => item.name.Equals(taskName, StringComparison.Ordinal));
        }
        public static bool IsQueued(TaskWrapper task)
        {
            if (task == null)
                throw new ArgumentNullException("Task cannot be null");

            return queuedTasks.Contains(task);
        }
        public static bool IsRunning(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
                throw new ArgumentNullException("Task name cannot be null or empty");

            return runningTasks.Exists(item => item.name.Equals(taskName, StringComparison.Ordinal));
        }
        public static bool IsRunning(TaskWrapper task)
        {
            if (task == null)
                throw new ArgumentNullException("Task cannot be null");

            return runningTasks.Contains(task);
        }
        public static bool HasTask(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
                throw new ArgumentNullException("Task name cannot be null or empty");

            return IsQueued(taskName) || IsRunning(taskName);
        }
        public static bool HasTask(TaskWrapper task)
        {
            if (task == null)
                throw new ArgumentNullException("Task cannot be null");

            return IsQueued(task) || IsRunning(task);
        }
    }
}