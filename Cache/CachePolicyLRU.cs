using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{    
    //UPDATE: using .NET built-in LinkedList collection
    public class CachePolicyLRU : ICachePolicy
    {
        public void BeforeInitCallback<K, V>(int capacity)
        {
            _buckets = new LinkedListNode<int>[capacity];            
            Parallel.For(0, capacity, i => { _buckets[i] = new LinkedListNode<int>(i); });
            _list = new LinkedList<int>();
        }

        public void GetItemCallback<K, V>(CacheItemArguments<K, V> arguments)
        {
            var node = _buckets[arguments.Index];
            if(_list.First != node)
            {
                if (node.List == _list)
                    _list.Remove(node);
                _list.AddFirst(node);
            }
        }

        public void SetItemCallback<K, V>(CacheItemArguments<K, V> arguments)
        {
            GetItemCallback(arguments);
        }

        public virtual void Evict<K, V>(CacheItemArguments<K, V> arguments)
        {
            arguments.Index = _list.Last.Value;
        }

        //protected
        protected LinkedList<int> _list;

        //private        
        private LinkedListNode<int>[] _buckets;
    }
}
