using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    //For Demo Only.
    internal static class CACHE_SET_CONFIGURATION
    {   
        public const int READ_TIMEOUT = 1000;
        public const int WRITE_TIMEOUT = 1000;
    }

    public class CacheSet<K, V, P> : IDisposable where P : ICachePolicy, new()
    {
        internal CacheSet(int way)
        {
            _way = way > 0 && way <= CACHE_CONFIGURATION.MAX_N_WAY ? way : CACHE_CONFIGURATION.DEFAULT_N_WAY;
            _items = new CacheItem<K, V>[_way];            
            _rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _policy = new P();
            _args = new CacheItemArguments<K, V>();
            
            _policy.BeforeInitCallback<K, V>(_way);
        }

        internal bool TryGet(K key, out V value)
        {
            //1-Way Set Associative
            if (_items.Count() <= 1)
            {
                if (_rwlock.TryEnterReadLock(CACHE_SET_CONFIGURATION.READ_TIMEOUT))
                {
                    try
                    {
                        if (_items[0] != null && _items[0].Key.Equals(key))
                        {
                            value = _items[0].Value;
                            return true;
                        }
                    }
                    finally
                    {
                        _rwlock.ExitReadLock();
                    }
                }
                value = default(V);
                return false;
            }
            //N-Way Set Associative
            else
            {
                return Get(key, out value);
            }
        }

        internal bool TrySet(K key, V value)
        {
            if(_items.Count() <= 1)
            {
                if(_rwlock.TryEnterWriteLock(CACHE_SET_CONFIGURATION.WRITE_TIMEOUT))
                {
                    try
                    {
                        _items[0] = new CacheItem<K, V>(key, value);
                        return true;
                    }
                    finally
                    {
                        _rwlock.ExitWriteLock();
                    }
                }
                return false;
            }
            else
            {
                return Set(key, value);
            }
         
        }

        //private                
        private readonly int _way;
        private readonly CacheItem<K, V>[] _items;
        private readonly ReaderWriterLockSlim _rwlock;
        private readonly ICachePolicy _policy;
        private readonly CacheItemArguments<K, V> _args;

        private bool Get(K key, out V value)
        {
            if (_rwlock.TryEnterUpgradeableReadLock(CACHE_SET_CONFIGURATION.READ_TIMEOUT))
            {
                try
                {
                    for (int i = 0; i < _items.Count(); i++)
                    {
                        if (_items[i] != null && _items[i].Key.Equals(key))
                        {
                            value = _items[i].Value;
                            //TODO: check delegation callback == null
                            if (_rwlock.TryEnterWriteLock(CACHE_SET_CONFIGURATION.WRITE_TIMEOUT))
                            {
                                try
                                {
                                    _policy.GetItemCallback(SetArguments(i, _items[i]));
                                }
                                finally
                                {
                                    _rwlock.ExitWriteLock();
                                }
                            }
                            return true;
                        }
                    }
                }
                finally
                {
                    _rwlock.ExitUpgradeableReadLock();
                }
            }

            value = default(V);
            return false;
        }

        private bool Set(K key, V value)
        {
            if (_rwlock.TryEnterUpgradeableReadLock(CACHE_SET_CONFIGURATION.READ_TIMEOUT))
            {
                try
                {
                    for (int i = 0; i < _items.Count(); i++)
                    {
                        if (_items[i] == null || (_items[i].Key.Equals(key)))
                        {
                            if (_rwlock.TryEnterWriteLock(CACHE_SET_CONFIGURATION.WRITE_TIMEOUT))
                            {
                                try
                                {
                                    _items[i] = new CacheItem<K, V>(key, value);
                                    _policy.SetItemCallback(SetArguments(i, _items[i]));
                                    return true;
                                }
                                finally
                                {
                                    _rwlock.ExitWriteLock();
                                }
                            }
                        }
                    }

                    //eviction
                    if (_rwlock.TryEnterWriteLock(CACHE_SET_CONFIGURATION.WRITE_TIMEOUT))
                    {
                        try
                        {
                            _policy.Evict(_args);
                            _items[_args.Index] = new CacheItem<K, V>(key, value);
                            _policy.SetItemCallback(SetArguments(_args.Index, _items[_args.Index]));
                            return true;
                        }
                        finally
                        {
                            _rwlock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _rwlock.ExitUpgradeableReadLock();
                }
            }
            return false;
        }

        private CacheItemArguments<K, V> SetArguments(int index, CacheItem<K, V> item)
        {
            _args.CacheItem = item;
            _args.Index = index;
            return _args;
        }
        

        //dispose
        public void Dispose()
        {
            _rwlock.Dispose();
        }

    }
}
