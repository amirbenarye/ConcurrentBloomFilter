using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// an implementaion of public domain algorithm FNV http://www.isthe.com/chongo/tech/comp/fnv/index.html#FNV-reference-source
    /// </summary>
    public static class FNVHashAlgorithm
    {

        /// <summary>
        /// defintion of the fnv prime for 32 bit operations
        /// </summary>
        const uint FNVPrime32bit = 16777619;
        /// <summary>
        /// definintion of the fnv offset for 32 bit operations
        /// </summary>
        const uint FNVOffset32bit = 2166136261;

        /// <summary>
        /// implementation of fnv-1a.
        /// </summary>
        /// <param name="data">a byte array containing data to be hashed</param>
        /// <param name="start">offset in the data array from which hashing should start</param>
        /// <param name="count">total count of bytes to hash</param>
        /// <returns></returns>
        public static uint FNV32_1A(byte[] data, int start, int count)
        {
            uint res = FNVOffset32bit;      // start from offset
            for (int i = 0; i < count; i++)
            {
                res ^= data[start + i];     
                res *= FNVPrime32bit;           // shift by prime last for fnv-1a
            }
            return res;
        }

        /// <summary>
        /// implementation of fnv.
        /// </summary>
        /// <param name="data">a byte array containing data to be hashed</param>
        /// <param name="start">offset in the data array from which hashing should start</param>
        /// <param name="count">total count of bytes to hash</param>
        public static uint FNV32(byte[] data, int start, int count)
        {
            uint res = FNVOffset32bit;   // start from offset
            for (int i = 0; i < count; i++)
            {
                res *= FNVPrime32bit;   // shift by prime first for fnv
                res ^= data[start + i];
            }
            return res;
        }
    }
}
