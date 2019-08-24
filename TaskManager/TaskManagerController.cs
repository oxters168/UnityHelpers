﻿using UnityEngine;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UnityHelpers
{
    public class TaskManagerController : MonoBehaviour
    {
        private static TaskManagerController taskManagerControllerInScene;
        private List<TaskWrapper> tasks = new List<TaskWrapper>();
        private List<(string, Action)> asyncActions = new List<(string, Action)>();
        private List<(string, Func<Task>)> funkyTasks = new List<(string, Func<Task>)>();
        private List<Action> actions = new List<Action>();

        private void Awake()
        {
            if (!taskManagerControllerInScene)
                taskManagerControllerInScene = this;
        }
        async void Update()
        {
            if (asyncActions.Count > 0)
            {
                int i = asyncActions.Count - 1;
                string taskName = asyncActions[i].Item1;
                Action action = asyncActions[i].Item2;
                asyncActions.RemoveAt(i);
                await TaskWrapper.CreateTask(action, taskName, (task) => { SteamController.LogToConsole("Running task " + taskName); tasks.Add(task); }, (task) => { SteamController.LogToConsole("Completed task " + taskName); tasks.Remove(task); });
            }
            if (funkyTasks.Count > 0)
            {
                int i = funkyTasks.Count - 1;
                string taskName = funkyTasks[i].Item1;
                Func<Task> action = funkyTasks[i].Item2;
                funkyTasks.RemoveAt(i);
                await TaskWrapper.CreateTask(action, taskName, (task) => { SteamController.LogToConsole("Running task " + taskName); tasks.Add(task); }, (task) => { SteamController.LogToConsole("Completed task " + taskName); tasks.Remove(task); });
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
        public static void RunActionAsync(string name, Action action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");
            if (HasTask(name))
                throw new InvalidOperationException("A task already exists with the name " + name);

            taskManagerControllerInScene.asyncActions.Insert(0, (name, action));
        }
        public static void RunActionAsync(string name, Func<Task> action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");
            if (action == null)
                throw new ArgumentNullException("Action cannot be null");
            if (HasTask(name))
                throw new InvalidOperationException("A task already exists with the name " + name);

            taskManagerControllerInScene.funkyTasks.Insert(0, (name, action));
        }
        public static void CancelTask(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty or null");

            var tasks = taskManagerControllerInScene.tasks;
            TaskWrapper task = tasks.FirstOrDefault(checkedTask => checkedTask.name.Equals(name, StringComparison.Ordinal));
            if (task == null)
                throw new KeyNotFoundException("Could not find task with name " + name);

            task.Cancel();
            tasks.Remove(task);
        }
        public static bool HasTask(string name)
        {
            var self = taskManagerControllerInScene;
            bool contains = false;
            if (self.asyncActions.Exists(item => item.Item1.Equals(name, StringComparison.Ordinal)))
                contains = true;
            else if (self.funkyTasks.Exists(item => item.Item1.Equals(name, StringComparison.Ordinal)))
                contains = true;
            else if (self.tasks.Exists(task => task.name.Equals(name, StringComparison.Ordinal)))
                contains = true;
            return contains;
        }
    }
}