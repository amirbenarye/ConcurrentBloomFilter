using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// interface for a hash function with hash type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHashFunction<T>
    {
        /// <summary>
        /// Hash count bytes for start in the data byte array. The result is stored into Hash
        /// Hash is provided by ref to avoid allocation of big hash objects when it is not neccary
        /// implementations are thread safe by contract
        /// </summary>
        /// <param name="data">the data to hash</param>
        /// <param name="start">start offset into the data array</param>
        /// <param name="count">the number of bytes to hash</param>
        /// <param name="Hash">the final hash result</param>
        void HashBytes(byte[] data, int start, int count, ref T Hash);
    }
}
