using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
    public class CacheItemArguments<K, V>
    {
        public CacheItem<K, V> CacheItem { get; set; }  //Cache Item Accessed
        public int Index { get; set; }  //Cache Line Index
    }
}
