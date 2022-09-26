using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
    public sealed class CacheItem<K, V> 
    {
        public K Key => _key;
        public V Value => _value;

        public CacheItem(K k, V v)
        {
            _key = k;
            _value = v;
        }

        //protected and private
        private readonly K _key;
        private V _value;

    }
}
