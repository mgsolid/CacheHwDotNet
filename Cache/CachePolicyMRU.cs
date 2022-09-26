using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
    public class CachePolicyMRU : CachePolicyLRU
    {
        public override void Evict<K, V>(CacheItemArguments<K, V> args)
        {
            args.Index = _list.First.Value;
        }
    }
}
