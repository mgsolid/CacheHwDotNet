using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache
{
    //UPDATE: supporting parameters as generic type of callback functions
    //TODO: delegation
    public interface ICachePolicy
    {   
        void BeforeInitCallback<K, V>(int capacity);

        void GetItemCallback<K, V>(CacheItemArguments<K, V> arguments);

        void SetItemCallback<K, V>(CacheItemArguments<K, V> arguments);

        void Evict<K, V>(CacheItemArguments<K, V> arguments);
    }
}
