using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Parallel
{
    public class FrameTaskController<T>
    {
        public int NumberAllocatedThreads { get; }

        internal bool DataAdditionHasEnded { get; private set; }

        internal readonly object Locker;

        private readonly List<FrameTask<T>> FrameTasks;

        private readonly Action<Action<T>> TaskCreator;

        public FrameTaskController(int NumberAllocatedThreads, Action<Action<T>> TaskCreator, Action<T> Frame)
        {
            if (TaskCreator == null)
                throw new Exception("The task creator cannot be null");

            if (NumberAllocatedThreads <= 0)
                throw new Exception("The allocated number of threads cannot be <= 0");

            this.TaskCreator = TaskCreator;
            this.NumberAllocatedThreads = NumberAllocatedThreads;
            Locker = new object();
            FrameTasks = new List<FrameTask<T>>();

            InitFrameTasks(Frame);
        }

        public List<int> GetFrameThreadIds()
        {
            return FrameTasks
                .Select(x => x.TaskThread.ManagedThreadId)
                .ToList();
        }

        private void InitFrameTasks(Action<T> Frame)
        {
            for (var i = 0; i < NumberAllocatedThreads; ++i)
            {
                FrameTasks.Add(new FrameTask<T>(this, Frame));
            }
        }

        private void ObjAdder(T Obj)
        {
            var minObj = FrameTasks[0];
            int min = minObj.ObjsCount;

            foreach (var i in FrameTasks)
            {
                if (i.ObjsCount < min)
                {
                    min = i.ObjsCount;
                    minObj = i;
                }
            }

            minObj.EnqueueObj(Obj);
        }

        public void Run()
        {
            if (DataAdditionHasEnded)
                throw new Exception("The process has already been started");

            FrameTasks.ForEach(x => x.Start());

            try
            {
                TaskCreator(new Action<T>(ObjAdder));

                DataAdditionHasEnded = true;
            
                FrameTasks.ForEach(x => { if (x.TaskThread.IsAlive) x.TaskThread.Join(); });
            }
            catch (Exception e)
            {
                throw e;
            }
            finally 
            {
                FrameTasks.ForEach(x =>  x.ErrorFlag = true);
            }
        }

        public async Task RunAsync() 
        {
            await Task.Run(() => Run());
        }
    }
}
