using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// A hash method that can be rehased. this is useful for creating numrous hashes for a single element.
    /// </summary>
    /// <typeparam name="T">The type fo the hash</typeparam>
    public interface IRehasableHashFunction<T> : IHashFunction<T>
    {
        /// <summary>
        /// Will create a new hash by rehashin a previous one.
        /// if T is a reference type this method must create a new instance.
        /// implementation must be thread safe by contract
        /// </summary>
        /// <param name="hash">the previos hash</param>
        /// <returns>the new hash</returns>
        T Rehash(T hash);
    }
}
