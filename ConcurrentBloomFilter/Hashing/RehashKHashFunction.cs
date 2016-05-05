using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// A k hash function that creates new hashes by rehashing the same value over and over.
    /// the i'th item of this hash function is the re hash of the i-1'th item of this function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RehashKHashFunction<T> : IKHashFunction<T>
    {
        IRehasableHashFunction<T> mBaseHash;
        /// <summary>
        /// Will initialize this instance with a base hash function.
        /// </summary>
        /// <param name="baseHash"></param>
        public RehashKHashFunction(IRehasableHashFunction<T> baseHash)
        {
            if (baseHash == null)
                throw new ArgumentNullException();
            mBaseHash = baseHash;
        }

        public void HashBytes(byte[] data, int start, int count, int k, T[] Hash, int offset)
        {
            if(k <= 0)
                return;
            T current = default(T);
            mBaseHash.HashBytes(data, start, count, ref current);  // hash the first value
            Hash[offset] = current;
            for(int i=1; i<k; i++)
            {
                current = mBaseHash.Rehash(current);
                Hash[offset + i] = current;
            }
        }

        public int HashCount
        {
            get { return int.MaxValue; }    // rehash function can create an infinite number of hashes
        }
    }
}
