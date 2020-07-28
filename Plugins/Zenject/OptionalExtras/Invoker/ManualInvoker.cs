//  <author>Pavel Fadrhonc</author>
//  <email>pavel.fadrhonc@gmail.com</email>
//  <summary> Allows to InvokeOnce or InvokeRepeating methods that are not part of MonoBehaviours.
// You have to call Update method yourself with delta time allowing for more control that WorldInvoker.</summary>

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public class ManualInvoker
    {
        public class InvokeTask
        {
            public Action Task;
            public float Delay;
            public float Interval;
            public float TimeSinceLastInvoke;
            public float TotalRunTime;
            public bool started;
            public float cancelTime;
            public bool active;

            public InvokeTask()
            {
                Reset();
            }

            public void Reset()
            {
                Task = null;
                Delay = 0;
                Interval = 0;
                TimeSinceLastInvoke = 0;
                started = false;
                cancelTime = 0;
                TotalRunTime = 0;
                active = true;
            }
        }

        private class InvokeTasksCache
        {
            private List<InvokeTask> _invokeTasks = new List<InvokeTask>();

            public InvokeTask GetCleanTask()
            {
                InvokeTask task;
                if ((task = _invokeTasks.FirstOrDefault(t => t.active == false)) == null)
                {
                    task = new InvokeTask();
                    _invokeTasks.Add(task);
                }
                else
                {
                    task.Reset();
                }

                return task;
            }

            public void StopTask(InvokeTask task)
            {
                task.active = false;
            }
        }

        private List<InvokeTask> _tasks = new List<InvokeTask>();
        private InvokeTasksCache _tasksCache = new InvokeTasksCache();
        private List<InvokeTask> _removedTasks = new List<InvokeTask>();

        public void Update(float dt)
        {
            _removedTasks.Clear();

            for (int index = 0; index < _tasks.Count; index++)
            {
                var invokeTask = _tasks[index];
                

                if (UpdateTask(invokeTask, dt))
                    _removedTasks.Add(invokeTask);
            }

            // removing has to be at the end for Invoke to work properly
            for (int index = 0; index < _removedTasks.Count; index++)
            {
                var removedTask = _removedTasks[index];
                StopTask(removedTask);
            }
        }

        #region PUBLIC METHODS

        public bool UpdateTask(InvokeTask invokeTask, float dt)
        {
            bool removed = false;
            
            invokeTask.TotalRunTime += dt;
            
            if (!invokeTask.started)
            {
                if (Mathf.Approximately(invokeTask.TotalRunTime, invokeTask.Delay) ||
                    invokeTask.TotalRunTime > invokeTask.Delay)
                {
                    invokeTask.started = true;
                    invokeTask.TimeSinceLastInvoke += dt;
                    invokeTask.Task();
                }
            }
            else
            {
                invokeTask.TimeSinceLastInvoke += dt;
                if (Mathf.Approximately(invokeTask.TimeSinceLastInvoke, invokeTask.Interval) ||
                    invokeTask.TimeSinceLastInvoke > invokeTask.Interval)
                {
                    invokeTask.Task();
                    invokeTask.TimeSinceLastInvoke = invokeTask.TimeSinceLastInvoke - invokeTask.Interval;
                }
            }

            if ((Mathf.Approximately(invokeTask.TotalRunTime, invokeTask.cancelTime) ||
                 invokeTask.TotalRunTime > invokeTask.cancelTime) &&
                (invokeTask.cancelTime > 0 && invokeTask.Interval != 0 || invokeTask.Interval == 0))
                removed = true;

            return removed;
        }

        public InvokeTask BorrowTask()
        {
            return _tasksCache.GetCleanTask();
        }
        
        /// <summary>
        /// You can borrow task here and the use this class UpdateTask function to update it with dt.
        /// This class won't be part of tasks that belong to this class so you can still update the whole class using Update
        /// </summary>
        public InvokeTask BorrowTask(Action task_, float delay_, float interval_, float cancelTime = 0)
        {
            var invokeTask = _tasksCache.GetCleanTask();
            invokeTask.Delay = delay_;
            invokeTask.Interval = interval_;
            invokeTask.Task = task_;
            invokeTask.started = false;
            invokeTask.cancelTime = cancelTime;

            return invokeTask;
        }        

        public void ReturnTask(InvokeTask task)
        {
            StopTask(task);
        }

        public void InvokeRepeating(Action task_, float delay_, float interval_, float cancelTime = 0)
        {
            _tasks.Add(BorrowTask(task_, delay_, interval_, cancelTime));
        }

        /// <summary>
        /// Invokes just once
        /// </summary>
        public void Invoke(Action task_, float delay_)
        {
            // using cancelTime = delay can work because record are remove at the end of Update loop
            InvokeRepeating(task_, delay_, 0, delay_);
        }

        /// <summary>
        /// Wont break if task is not actually Invoking
        /// </summary>
        public void StopInvoke(Action task, float delay)
        {
            var invTask = _tasks.Find(t => t.Task == task);
            if (invTask == null) return;
            if (delay == 0)
                StopTask(invTask);
            else
                invTask.cancelTime = invTask.TotalRunTime + delay;
        }

        #endregion

        #region PRIVATE / PROTECTED METHODS

        private void StopTask(InvokeTask task)
        {
            _tasks.Remove(task);
            _tasksCache.StopTask(task);        
        }

        #endregion
    }
    

}

