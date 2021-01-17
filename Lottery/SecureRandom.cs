using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Lottery
{
   public class SecureRandom
    {
        private readonly static double DOUBLE_UNIT = 1.0 / (1L << 53);
        RandomNumberGenerator generator;
        public SecureRandom(RandomNumberGenerator generator)
        {
            this.generator = generator;
        }
        //产生bits位的随机整数
        // bits>=1
        protected uint Next(int bits)
        {
            byte[] bs = new byte[4];
            generator.GetBytes(bs);
            uint x = BitConverter.ToUInt32(bs, 0);
            return x >> (32 - bits);
        }
        //产生bits位的随机整数
        //bits>=1
        protected ulong Next2(int bits)
        {
            byte[] bs = new byte[8];
            generator.GetBytes(bs);
            ulong x = BitConverter.ToUInt64(bs, 0);
            return x >> (64 - bits);
        }
 
 
        protected int BitLength(int x)
        {
            int len = 0;
            while (x > 0)
            {
                len++;
                x >>= 1;
            }
            return len;
        }
        protected int BitLength(long x)
        {
            int len = 0;
            while (x > 0)
            {
                len++;
                x >>= 1;
            }
            return len;
        }
 
        public int NextInt()
        {
            return (int)Next(32);
        }
        //max>=1,不包括max
        public int NextInt(int max)
        {
            if (max <= 0) throw new ArgumentException("max <= 0");
            int len = BitLength(max);
            uint x = Next(len);
            while (x >= max)
            {
                x = Next(len);
            }
            return (int)x;
        }
        //不包括max
        public int NextInt(int min, int max)
        {
            return NextInt(max - min) + min;
        }
        //max>=1,不包括max
        public int FastNextInt(int max)
        {
            byte[] bs = new byte[4];
            generator.GetBytes(bs);
            int value = BitConverter.ToInt32(bs, 0);
            value = value % max;
            if (value < 0) value = -value;
            return value;
        }
        //不包括max
        public int FastNextIntint(int min, int max)
        {
            return FastNextInt(max - min) + min;
        }
        public long NextLong()
        {
            return ((long)(Next(32)) << 32) + Next(32);
        }
        //max>=1,不包括max
        public long NextLong(long max)
        {
            if (max <= 0) throw new ArgumentException("max <= 0");
            int len = BitLength(max);
            ulong x = Next2(len);
            while (x >= (ulong)max)
            {
                x = Next2(len);
            }
            return (long)x;
        }
        //不包括max
        public long NextLong(long min, long max)
        {
            return NextLong(max - min) + min;
        }
        //max>=1,不包括max
        public long FastNextLong(long max)
        {
            byte[] bs = new byte[8];
            generator.GetBytes(bs);
            long value = BitConverter.ToInt64(bs, 0);
            value = value % max;
            if (value < 0) value = -value;
            return value;
        }
        //不包括max
        public long FastNextLong(long min, long max)
        {
            return FastNextLong(max - min) + min;
        }
        public bool NextBoolean()
        {
            return Next(1) != 0;
        }
        //此方法是java源码中拷贝，此算法得到的浮点数的随机性未考究
        public float NextFloat()
        {
            return Next(24) / ((float)(1 << 24));
        }
        //此方法是java源码中拷贝，此算法得到的浮点数的随机性未考究
        public double NextDouble()
        {
            return (((long)(Next(26)) << 27) + Next(27)) * DOUBLE_UNIT;
        }
 
        //数组长度需要一致
        protected bool GreaterThanOrEqual(byte[] a, byte[] b)
        {
            //if (a.Length > b.Length) return true;
            //if (a.Length < b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] > b[i]) return true;
                if (a[i] < b[i]) return false;
            }
            return true;//全等
        }
 
        protected byte[] Next3(int bits)
        {
            int len = (bits + 7) >> 3;
            byte[] bs = new byte[len];
            generator.GetBytes(bs);
            bs[0] >>= (8 - bits) & 0x7;
            return bs;
        }
        protected int BitLength(byte[] x)
        {
            int i = 0;
            for (; i < x.Length && x[i] == 0; i++) ;
            return i == 8 ? 0 : BitLength(x[i]) + (x.Length - i - 1) * 8;
        }
 
        public byte[] NextBigInt(byte[] max)
        {
            int bitLen = BitLength(max);
            if(bitLen==0) throw new ArgumentException("max == 0");
            byte[] max_ = new byte[(bitLen + 7) >>3];
            Array.Copy(max, max.Length - max_.Length, max_, 0, max_.Length);
            byte[] x = Next3(bitLen);
            while (GreaterThanOrEqual(x,max_))
            {
                x = Next3(bitLen);
            }
            return x;
        }
    }

}
