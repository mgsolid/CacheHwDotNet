using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cache;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

//UPDATE: removing some dead codes
namespace CacheTest
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void Test_Hit()
        {
            var cc = new Cache<int, string>();
            Assert.IsTrue(cc.N_Set == 1024);
            Assert.IsTrue(cc.N_Way == 32);
            Assert.IsTrue(cc.TrySet(100, "XYZ"));
            Assert.IsTrue(cc.TryGet(100, out var val));
            Assert.IsTrue(val == "XYZ");
        }

        [TestMethod]
        public void Test_Miss()
        {
            var cc = new Cache<int, (int userid, string name, string address, int ssn), CachePolicyLRU>(8, 2);
            Assert.IsTrue(cc.TrySet(100, (100, "Aaa Bbb", "aaabbbccc.abcd", 12345)));
            Assert.IsTrue(cc.TryGet(100, out var user));
            Assert.IsTrue(user.userid == 100);
            Assert.IsTrue(user.address == "aaabbbccc.abcd");

            //Cache Miss
            Assert.IsFalse(cc.TryGet(-1, out _));
        }

        [TestMethod]
        public void Test_Config()
        {
            var cc = new Cache<int, int, CachePolicyLRU>(nWay: 257, nSet: 100001);
            Assert.IsTrue(cc.N_Way == 256);
            Assert.IsTrue(cc.N_Set == 100000);
            Assert.IsTrue(cc.TrySet(100, 100));
            Assert.IsTrue(cc.TryGet(100, out int val));
            Assert.IsTrue(val == 100);
        }

        [TestMethod]
        public void Test_LRU()
        {
            var cc = new Cache<int, int>(4, 2);
            Assert.IsTrue(cc.N_Way == 4);
            Assert.IsTrue(cc.Policy.ToLower().Contains("lru"));

            var sequence = new int[] { 0, 2, 4, 6, 4, 6, 8, 11, 12, 14, 15, 16, 0, 4, 10 };
            foreach (var i in sequence)
            {
                Assert.IsTrue(cc.TrySet(i, i));
                Assert.IsTrue(cc.TryGet(i, out int v1));
                Assert.IsTrue(v1 == i);
            }
          
            Assert.IsFalse(cc.TryGet(2, out _));
            Assert.IsTrue(cc.TryGet(10, out int v2));
            Assert.IsTrue(v2 == 10);
            Assert.IsTrue(cc.TryGet(0, out v2));
            Assert.IsTrue(v2 == 0);

            //clear and re-test
            cc.Clear();
            sequence = new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 11, 12, 14, 2, 4, 16, 16, 10, 10 };
            foreach (var i in sequence)
            {
                Assert.IsTrue(cc.TrySet(i, i));
                Assert.IsTrue(cc.TryGet(i, out int v1));
                Assert.IsTrue(v1 == i);
            }

            //LRU
            Assert.IsFalse(cc.TryGet(0, out _));
            Assert.IsFalse(cc.TryGet(6, out _));
            Assert.IsFalse(cc.TryGet(8, out _));

            Assert.IsTrue(cc.TryGet(10, out v2));
            Assert.IsTrue(v2 == 10);
            Assert.IsTrue(cc.TryGet(16, out v2));
            Assert.IsTrue(v2 == 16);          
        }


        [TestMethod]
        public void Test_MRU()
        {
            var cc = new Cache<int, int, CachePolicyMRU>(4, 2);
            Assert.IsTrue(cc.N_Set == 2);

            var sequence = new int[] { 1, 3, 5, 7, 0, 2, 4, 6, 4, 6, 1, 2, 3, 4, 5, 6, 6, 6, 10 };
            foreach (var i in sequence)
            {
                Assert.IsTrue(cc.TrySet(i, i));
                Assert.IsTrue(cc.TryGet(i, out int v1));
                Assert.IsTrue(v1 == i);
            }
                        
            //MRU
            Assert.IsFalse(cc.TryGet(6, out _));
            Assert.IsTrue(cc.TryGet(10, out int v2));
            Assert.IsTrue(v2 == 10);
            Assert.IsTrue(cc.TryGet(2, out v2));
            Assert.IsTrue(v2 == 2);

        }


        [TestMethod]
        public void Test_FIFO()
        {
            var cc = new Cache<int, int?, CachePolicyFIFO>(4, 100);

            var sequence = new int[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 100, 200, 300, 400 };
            foreach (var i in sequence)
            {
                Assert.IsTrue(cc.TrySet(i, i));
                Assert.IsTrue(cc.TryGet(i, out int? v1));
                Assert.IsTrue(v1 == i);
            }
                        
            //FIFO
            Assert.IsFalse(cc.TryGet(500, out _));
            Assert.IsTrue(cc.TryGet(100, out int? v2));
            Assert.IsTrue(v2 == 100);
            Assert.IsTrue(cc.TryGet(300, out v2));
            Assert.IsTrue(v2 == 300);
        }

        [TestMethod]
        public void Test_RR()
        {
            var cc = new Cache<int, int?, CachePolicyRR>(2, 100);

            var sequence = new int[] { 100, 200, 300 };
            foreach (var i in sequence)
            {
                Assert.IsTrue(cc.TrySet(i, i));
                Assert.IsTrue(cc.TryGet(i, out int? v1));
                Assert.IsTrue(v1 == i);
            }

            Assert.IsFalse(cc.TryGet(100, out _) && cc.TryGet(300, out _));
        }

        [TestMethod]
        public void Test_1_WAY()
        {
            var cc = new Cache<int, int, CachePolicyMRU>(nWay: 1, nSet: 2);

            var sequence = new int[] { 1, 3, 5, 1, 1, 1, 1, 3 };
            for (int j = 0; j < sequence.Length; j++)
            {
                var i = sequence[j];
                Assert.IsTrue(cc.TrySet(i, i));
                Assert.IsTrue(cc.TryGet(i, out var v1));
                Assert.IsTrue(v1 == i);

                if (j > 1 && sequence[j] != sequence[j-1])
                    Assert.IsFalse(cc.TryGet(sequence[j - 1], out _));
            }
            Assert.IsTrue(cc.TryGet(3, out int val));
            Assert.IsTrue(val == 3);
        }

        [TestMethod]
        public void Test_Misc()
        {
            var cc = new Cache<int, string, CachePolicyPLRU>(32, 1024);

            var C = 200;
            var rnds = new Random[C];
            

            int hit = 0, miss = 0;
            int[] t = new int[C];
            int ttl = 0, cmp = 0;
            Parallel.For(0, C, i =>
            {
                rnds[i] = new Random(i);
                t[i] = rnds[i].Next(1, C);
                Interlocked.Add(ref ttl, t[i]);
                for (int tt = 0; tt < t[i]; tt ++)
                {
                    var x = rnds[i].Next(int.MaxValue);
                    Assert.IsTrue(cc.TrySet(x, x.ToString()));                    
                    if(cc.TryGet(x, out string v))
                    {
                        Assert.IsTrue(v == x.ToString());
                        Interlocked.Increment(ref hit);
                    }
                    else
                    {
                        Interlocked.Increment(ref miss);
                    }
                    Interlocked.Increment(ref cmp);
                    Trace.WriteLine($"%{(decimal)cmp/ttl*100}, hit:{hit}, miss:{miss}");

                }
            });

            Assert.IsTrue(miss / (hit + miss) < 0.001);
        }

        [TestMethod]
        public void Test_PLRU()
        {
            var cc = new Cache<int, int, CachePolicyPLRU>(32, 1);

            var sequence = new int[32];
            for (int i = 0; i < 32; i++)
                sequence[i] = i;
            for (int j = 0; j < sequence.Length; j++)
            {
                var i = sequence[j];
                Assert.IsTrue(cc.TrySet(i, i));
                Assert.IsTrue(cc.TryGet(i, out var v1));
                Assert.IsTrue(v1 == i);
            }
            Assert.IsTrue(cc.TrySet(33, 33));
            Assert.IsFalse(cc.TryGet(0, out _));
            Assert.IsTrue(cc.TryGet(1, out _));
        }
    }
}
