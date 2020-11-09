using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class NumberIterator
    {
        private readonly object Locker;

        private long value;
        public long Value
        {
            get
            {
                lock (Locker)
                {
                    value++;
                    return value;
                }
            }
        }

        public NumberIterator()
        {
            Locker = new object();
        }
    }
}
