using System.Collections.Generic;
using System.Linq;

namespace Spaccanavi.Utilities.Collections
{
    public sealed class ReadOnlyQueue<T>
    {
        private readonly T[] data;

        private int index = 0;
        public int Count { get; }



        /* Constructor */

        public ReadOnlyQueue(IEnumerable<T> items)
        {
            Count = items.Count();
            data = new T[Count];

            int i = 0;
            foreach (T item in items)
                data[i++] = item;
        }



        public T Dequeue()
        {
            T result = data[index];

            if (++index >= Count)
                index = 0;

            return result;
        }

        public T[] GetInnerArray()
            => data;
    }
}
