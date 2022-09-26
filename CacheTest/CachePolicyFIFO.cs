using Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheTest
{
    public class CachePolicyFIFO : ICachePolicy
    {
        public CachePolicyFIFO()
        {
            hash = new HashSet<int>();
            queue = new Queue<int>();
        }


        HashSet<int> hash;
        Queue<int> queue;

        public void BeforeInitCallback<K, V>(int capacity)
        {
            //do nothing.
        }

        public void GetItemCallback<K, V>(CacheItemArguments<K, V> arguments)
        {
            //do nothing.
        }

        public void SetItemCallback<K, V>(CacheItemArguments<K, V> arguments )
        {
            var index = arguments.Index;
            if (!hash.Contains(index))
            {
                hash.Add(index);
                queue.Enqueue(index);
            }
        }

        public void Evict<K, V>(CacheItemArguments<K, V> arguments)
        {
            var idx = queue.Dequeue();
            hash.Remove(idx);
            arguments.Index = idx;
        }
    }
}
