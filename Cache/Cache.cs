using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    //For Demo Only
    internal static class CACHE_CONFIGURATION
    {
        public const int DEFAULT_N_WAY = 32;
        public const int DEFAULT_N_SET = 1024;
        public const int MAX_N_WAY = 256;
        public const int MAX_N_SET = 100000;
    }

    public class Cache<K, V> : Cache<K, V, CachePolicyLRU>  //Default Policy
    {
        public Cache() { }
        public Cache(int nWay = 32, int nSet = 1024) : base(nWay, nSet) { }
    }

    public class Cache<K, V, P> where P : ICachePolicy, new()
    {     
        //Public Members
        public int N_Way => _nWay;
        public int N_Set => _nSet;
        public string Policy => typeof(P).ToString();

        public Cache() : this(CACHE_CONFIGURATION.DEFAULT_N_WAY, CACHE_CONFIGURATION.DEFAULT_N_SET)
        {
        }

        public Cache(int nWay = CACHE_CONFIGURATION.DEFAULT_N_WAY, 
            int nSet = CACHE_CONFIGURATION.DEFAULT_N_SET)
        {
            _nWay = Math.Min(nWay, CACHE_CONFIGURATION.MAX_N_WAY);
            _nSet = Math.Min(nSet, CACHE_CONFIGURATION.MAX_N_SET);
            _array = new CacheSet<K, V, P>[_nSet];
            Parallel.For(0, _nSet, i => { _array[i] = CreateCacheSet(); });
        }

        public bool TrySet(K key, V value)
        {
            var index = GetIndexInternal(key);
            var cacheSet = GetCacheSet(key);
            return cacheSet.TrySet(key, value);
        }

        public bool TryGet(K key, out V value)
        {
            var index = GetIndexInternal(key);
            var cacheSet = GetCacheSet(key);
            return cacheSet.TryGet(key, out value);
        }

        public void Clear()
        {
            Parallel.For(0, _nSet, i => { _array[i] = CreateCacheSet(); });
        }
        
        //Private Members
        private readonly int _nWay;
        private readonly int _nSet;
        private readonly CacheSet<K, V, P>[] _array;

        private int GetIndexInternal(K key)
        {
            var hash = key.GetHashCode();
            if(hash < 0)
            {
                hash = (hash == Int32.MinValue) ? 0 : -hash;
            }
            return hash;
        }

        private CacheSet<K, V, P> GetCacheSet(K key)
        {
            var index = GetIndexInternal(key) % _nSet;
            return Interlocked.CompareExchange(ref _array[index], CreateCacheSet(), null);
        }

        private CacheSet<K, V, P> CreateCacheSet()
        {
            return new CacheSet<K, V, P>(_nWay);
        }
    }
}