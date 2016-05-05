using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{   
    /// <summary>
    /// A hash function that generates k diffrent hashes for a single element
    /// T is the type of the generated hashes
    /// </summary>
    public interface IKHashFunction<T>
    {   
        /// <summary>
        /// Generate k diffrent hashed for the specified byte array. count bytes are hashed from the index start of the byte array.
        /// The result is applied to the Hash array starting at the given offset
        /// implementation must be thread safe by contract
        /// </summary>
        /// <param name="data">byte array of data to be hashed</param>
        /// <param name="start">the start offset to the data array</param>
        /// <param name="count">total bytes that should be hashed</param>
        /// <param name="k">the count of diffrent hashes that should be generated for the specified data. k must be lower than or equal to HashCount</param>
        /// <param name="Hash">The result array. the k hashes will be inserted to this array at the end of this method</param>
        /// <param name="offset">an offset to the Hash array. the first hash will be inserted to Hash[offset] the last will be inserted to Hash[offset+k-1]</param>
        void HashBytes(byte[] data, int start, int count,int k,T[] Hash,int offset);
        /// <summary>
        /// returns the maximum number of hashes that can be generated using this function.
        /// </summary>
        int HashCount { get; }
    }
}
