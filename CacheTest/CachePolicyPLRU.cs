using Cache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTest
{
    //Bits PLRU
    class CachePolicyPLRU : ICachePolicy
    {
        BitVector32[] _flags;
        int _flawed;
        
        public void BeforeInitCallback<K, V>(int capacity)
        {
            int num = (capacity - 1) / 32 + 1;
            _flags = new BitVector32[num];
            Parallel.For(0, num, i => { _flags[i] = new BitVector32(0); });
            _flawed = 0;
        }

        public void Evict<K, V>(CacheItemArguments<K, V> arguments)
        {
            arguments.Index = GetLeastUnsetBit();
        }

        public void GetItemCallback<K, V>(CacheItemArguments<K, V> arguments)
        {
            _flags[arguments.Index / 32][1 << (arguments.Index % 32)] = true;
            if (IsAllTrue())
            {
                Reset();
                _flags[arguments.Index / 32][1 << (arguments.Index % 32)] = true;
            }
        }

        public void SetItemCallback<K, V>(CacheItemArguments<K, V> arguments)
        {
            GetItemCallback(arguments);
        }

        private bool IsAllTrue()
        {
            _flawed = 0;
            foreach (var f in _flags)
            {
                if (f.Data != ~0)
                {   
                    return false;
                }
                _flawed++;
            }
            return true;
        }

        private void Reset()
        {
            Parallel.For(0, _flags.Count(), i => { _flags[i][~0] = false; });
            _flawed = 0;
        }

        private int GetLeastUnsetBit()
        {
            int unset = 0;
            int data = ~(_flags[_flawed].Data);
            while (data % 2 == 0)
            {
                data >>= 1;
                unset++;
            }

            return _flawed * 32 + unset;

        }
    }
}
