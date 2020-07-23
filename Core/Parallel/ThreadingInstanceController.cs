using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Parallel
{
    public static class ThreadingInstanceController<T>
    {
        private static readonly object Locker;

        private static readonly Dictionary<int, T> Cache;

        private static Func<T> FactoryMethod;

        static ThreadingInstanceController()
        {
            Cache = new Dictionary<int, T>();
            Locker = new object();
        }

        public static void SetFactoryMethod(Func<T> Factory)
        {
            if (Factory == null)
                throw new Exception("The factory method cannot be null");

            lock (Locker)
            {
                FactoryMethod = Factory;
            }
        }

        public static T GetInstance()
        {
            var thId = Thread.CurrentThread.ManagedThreadId;

            return GetInstance(thId);
        }

        public static T GetInstance(int ThreadId)
        {
            if (FactoryMethod == null)
                throw new Exception("The factory method cannot be null");

            T instance;

            lock (Locker)
            {
                if (!Cache.TryGetValue(ThreadId, out instance))
                {
                    instance = FactoryMethod.Invoke();

                    Cache.Add(ThreadId, instance);
                }
            }

            return instance;
        }

        public static List<T> GetInstances(IEnumerable<int> Ids)
        {
            if (Ids == null)
                throw new Exception("The collection of Ids cannot be null");

            return Ids
                .Select(x => GetInstance(x))
                .ToList();
        }
    }
}
