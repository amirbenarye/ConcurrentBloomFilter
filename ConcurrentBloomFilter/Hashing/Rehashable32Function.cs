using BloomFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// base class for rehashable 32 bit hash function. This class provides rehashing functionallity leaving the hashing itself
    /// for derived classes
    /// </summary>
    public abstract class Rehashable32Function : IRehasableHashFunction<uint>
    {
        [ThreadStatic]
        static byte[] mWorkBytes;   // work bytes that will store the 32bit for rehashing

        public abstract void HashBytes(byte[] data, int start, int count, ref uint Hash);

        public uint Rehash(uint hash)
        {
            if (mWorkBytes == null)
                mWorkBytes = new byte[4];
            BitWriter.WriteBits((int)hash, mWorkBytes, 0);
            HashBytes(mWorkBytes, 0, 4, ref hash);
            return hash;
        }
    }
}
