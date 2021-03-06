﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Parallel
{
    internal class FrameTask<T>
    {
        public Thread TaskThread { get; }

        public bool ErrorFlag { get; set; }

        public Exception ThrownException { get; private set; }

        private readonly Action<T> Frame;

        private readonly Queue<T> ObjsQueue;

        private readonly FrameTaskController<T> TaskController;

        public event Action OnExcept;

        public int ObjsCount
        {
            get
            {
                int count;
                lock (TaskController.Locker)
                {
                    count = ObjsQueue.Count;
                }

                return count;
            }
        }

        public FrameTask(FrameTaskController<T> TaskController, Action<T> Frame)
        {
            this.TaskController = TaskController;
            this.Frame = Frame;
            ObjsQueue = new Queue<T>();
            TaskThread = new Thread(Process)
            {
                IsBackground = true
            };
        }

        public void EnqueueObj(T Obj)
        {
            lock (TaskController.Locker)
            {
                ObjsQueue.Enqueue(Obj);
            }
        }

        public void Start()
        {
            TaskThread.Start();
        }

        private T DequeueObj()
        {
            T obj;

            lock (TaskController.Locker)
            {
                obj = ObjsQueue.Dequeue();
            }

            return obj;
        }

        private void Process()
        {
            while ((!TaskController.DataAdditionHasEnded || ObjsCount > 0) && !ErrorFlag)
            {
                if (ObjsCount == 0)
                    Thread.Sleep(1);

                while (ObjsCount > 0 && !ErrorFlag)
                {
                    var obj = DequeueObj();

                    try
                    {
                        Frame?.Invoke(obj);
                    }
                    catch (Exception ex)
                    {
                        ThrownException = ex;
                        OnExcept?.Invoke();
                    }
                }
            }
        }
    }
}
