using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Zenject
{
    public class MonoUpdater : MonoBehaviour
    {
        public Action<float> OnUnityUpdate;

        private void Update()
        {
            if (OnUnityUpdate != null)
            {
                OnUnityUpdate(Time.deltaTime);
            }
        }
    }
 
    /// <summary>
    /// Same as ManualInvoker except update method is called automatically from Unity Update method
    /// </summary>
    public class WorldInvoker : IInvoker, IInitializable
    {
        private List<ManualInvoker.InvokeTask> ignorePauseTasks = new List<ManualInvoker.InvokeTask>();
        private List<int> removedTasks = new List<int>();
        
        private readonly MonoUpdater monoUpdater;
        private readonly ManualInvoker manualInvoker;
        
        public WorldInvoker(
            ManualInvoker manualInvoker,
            MonoUpdater monoUpdater)
        {
            this.monoUpdater = monoUpdater;
            this.manualInvoker = manualInvoker;
        }

        public void Initialize()
        {
            monoUpdater.OnUnityUpdate += OnUnityUpdate;
        }

        private void OnUnityUpdate(float dt)
        {
            foreach (var ignorePauseTask in ignorePauseTasks)
            {
                var remove = manualInvoker.UpdateTask(ignorePauseTask, dt);
                if (removedTasks.Count == 0)
                    removedTasks.Clear();
                
                if (remove)
                    removedTasks.Add(ignorePauseTasks.IndexOf(ignorePauseTask));
            }

            if (removedTasks.Count > 0)
            {
                foreach (var removedTask in removedTasks)
                {
                    ignorePauseTasks.RemoveAt(removedTask);
                }

                removedTasks.Clear();
            }
        }

        public void InvokeRepeating(Action task_, float delay_, float interval_, float cancelTime = 0, bool ignorePause = false)
        {
            if (ignorePause)
            {
                var task = manualInvoker.BorrowTask(task_, delay_, interval_, cancelTime);
                ignorePauseTasks.Add(task);
            }
            else
            {
                manualInvoker.InvokeRepeating(task_, delay_, interval_, cancelTime);
            }
        }

        public void Invoke(Action task_, float delay_, bool ignorePause)
        {
            if (ignorePause)
            {
                var task = manualInvoker.BorrowTask(task_, delay_, 0, delay_);
                ignorePauseTasks.Add(task);
            }
            else
            {
                manualInvoker.Invoke(task_, delay_);
            }
        }

        public void StopInvoke(Action task, float delay)
        {
            manualInvoker.StopInvoke(task, 0);
            for (var index = 0; index < ignorePauseTasks.Count; index++)
            {
                var ignorePauseTask = ignorePauseTasks[index];
                if (ignorePauseTask.Task == task)
                {
                    ignorePauseTasks.RemoveAt(index);
                    break;
                }
            }
        }
    }
}