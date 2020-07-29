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
        public class InvokerTaskInfo
        {
            public InvokerTask Task;
            public float Delay;
            public float Interval;
            public float LastInvokeTime; // this is time incremented by dt when the task was actually ran
            public float NextInvokeTime; // this is multiples of Interval
            public float TotalRunTime;
            public bool started;
            public float cancelTime;
            public bool active;

            public InvokerTaskInfo()
            {
                Reset();
            }

            public void Reset()
            {
                Task = null;
                Delay = 0;
                Interval = 0;
                LastInvokeTime = 0;
                NextInvokeTime = 0;
                started = false;
                cancelTime = 0;
                TotalRunTime = 0;
                active = true;
            }
        }

        private class InvokeTasksCache
        {
            private List<InvokerTaskInfo> _invokeTasks = new List<InvokerTaskInfo>();

            public InvokerTaskInfo GetCleanTask()
            {
                InvokerTaskInfo taskInfo;
                if ((taskInfo = _invokeTasks.FirstOrDefault(t => t.active == false)) == null)
                {
                    taskInfo = new InvokerTaskInfo();
                    _invokeTasks.Add(taskInfo);
                }
                else
                {
                    taskInfo.Reset();
                }

                return taskInfo;
            }

            public void StopTask(InvokerTaskInfo taskInfo)
            {
                taskInfo.active = false;
            }
        }

        private List<InvokerTaskInfo> _tasks = new List<InvokerTaskInfo>();
        private InvokeTasksCache _tasksCache = new InvokeTasksCache();
        private List<InvokerTaskInfo> _removedTasks = new List<InvokerTaskInfo>();

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

        public bool UpdateTask(InvokerTaskInfo invokerTaskInfo, float dt)
        {
            bool removed = false;
            
            invokerTaskInfo.TotalRunTime += dt;
            
            if (!invokerTaskInfo.started)
            {
                if (invokerTaskInfo.TotalRunTime >= invokerTaskInfo.Delay)
                {
                    invokerTaskInfo.started = true;
                    invokerTaskInfo.Task(invokerTaskInfo.TotalRunTime - invokerTaskInfo.Delay);
                    invokerTaskInfo.TotalRunTime -= invokerTaskInfo.Delay;
                    invokerTaskInfo.LastInvokeTime = invokerTaskInfo.TotalRunTime;
                    invokerTaskInfo.NextInvokeTime = invokerTaskInfo.Interval;
                }
            }
            else
            {
                if (invokerTaskInfo.TotalRunTime > invokerTaskInfo.NextInvokeTime)
                {
                    invokerTaskInfo.Task(invokerTaskInfo.TotalRunTime - invokerTaskInfo.LastInvokeTime);
                    invokerTaskInfo.LastInvokeTime = invokerTaskInfo.TotalRunTime;
                    invokerTaskInfo.NextInvokeTime += invokerTaskInfo.Interval;
                }
            }

            if ((invokerTaskInfo.TotalRunTime >= invokerTaskInfo.cancelTime) &&
                (invokerTaskInfo.cancelTime > 0 && invokerTaskInfo.Interval != 0 || invokerTaskInfo.Interval == 0))
                removed = true;

            return removed;
        }

        public InvokerTaskInfo BorrowTask()
        {
            return _tasksCache.GetCleanTask();
        }
        
        /// <summary>
        /// You can borrow task here and the use this class UpdateTask function to update it with dt.
        /// This class won't be part of tasks that belong to this class so you can still update the whole class using Update
        /// </summary>
        public InvokerTaskInfo BorrowTask(InvokerTask task_, float delay_, float interval_, float cancelTime = 0)
        {
            var invokeTask = _tasksCache.GetCleanTask();
            invokeTask.Delay = delay_;
            invokeTask.Interval = interval_;
            invokeTask.Task = task_;
            invokeTask.started = false;
            invokeTask.cancelTime = cancelTime;

            return invokeTask;
        }        

        public void ReturnTask(InvokerTaskInfo taskInfo)
        {
            StopTask(taskInfo);
        }

        public void InvokeRepeating(InvokerTask task_, float delay_, float interval_, float cancelTime = 0)
        {
            _tasks.Add(BorrowTask(task_, delay_, interval_, cancelTime));
        }

        /// <summary>
        /// Invokes just once
        /// </summary>
        public void Invoke(InvokerTask task_, float delay_)
        {
            // using cancelTime = delay can work because record are remove at the end of Update loop
            InvokeRepeating(task_, delay_, 0, delay_);
        }

        /// <summary>
        /// Wont break if task is not actually Invoking
        /// </summary>
        public void StopInvoke(InvokerTask task, float delay)
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

        private void StopTask(InvokerTaskInfo taskInfo)
        {
            _tasks.Remove(taskInfo);
            _tasksCache.StopTask(taskInfo);        
        }

        #endregion
    }
    

}

