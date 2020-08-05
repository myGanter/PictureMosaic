using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class BinaryHeap<T> : IEnumerable<T>
    {
        private readonly IComparer<T> Comparer;
        private readonly List<T> Data;

        public T this[int Index] => Data[Index];

        public BinaryHeap(IComparer<T> Comparer) 
        {
            if (Comparer == null)
                throw new Exception("Comparer is null");

            Data = new List<T>();
            this.Comparer = Comparer;
        }

        public void Add(T Item) 
        {           
            Data.Insert(GetInsertIndex(Item), Item);
        }

        public Tuple<int, int> GetNeighborsIndexes(T Item) 
        {
            var i1 = GetInsertIndex(Item);

            if (i1 == Data.Count) 
            {
                if (i1 == 0)
                    return Tuple.Create(-1, -1);

                return Tuple.Create(i1 - 1, -1);
            }

            if (i1 == 0) 
            {
                if (Data.Count == 0)
                    return Tuple.Create(-1, -1);

                return Tuple.Create(-1, i1);
            }

            var compVal = Comparer.Compare(Item, Data[i1]);
            if (compVal > 0)
            {
                return Tuple.Create(i1, i1 + 1);
            }
            else 
            {
                return Tuple.Create(i1 - 1, i1);
            }
        }

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int GetInsertIndex(T Item) 
        {
            var l = 0;
            var r = Data.Count;
            var mid = (l + r) / 2;

            while (l != r)
            {
                var compVal = Comparer.Compare(Item, Data[mid]);
                if (compVal > 0)
                {
                    l = mid + 1;
                    mid = (l + r) / 2;
                }
                else
                {
                    r = mid;
                    mid = (l + r) / 2;
                }
            }

            return mid;
        }
    }
}
