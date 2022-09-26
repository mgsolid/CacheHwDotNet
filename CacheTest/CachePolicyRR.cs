using Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTest
{
    //Random Replacement Policy Cache
    class CachePolicyRR : ICachePolicy
    {   
        HashSet<int> hash = new HashSet<int>();

        public void BeforeInitCallback<K, V>(int capacity)
        {
            for (int i = 0; i < capacity; i++)
                hash.Add(i);
        }

        public void Evict<K, V>(CacheItemArguments<K, V> arguments)
        {
            arguments.Index = new Random().Next(0, hash.Count - 1);
        }

        public void GetItemCallback<K, V>(CacheItemArguments<K, V> arguments)
        {
            //do nothing
        }

        public void SetItemCallback<K, V>(CacheItemArguments<K, V> arguments)
        {
            //do nothing
        }
    }
}
