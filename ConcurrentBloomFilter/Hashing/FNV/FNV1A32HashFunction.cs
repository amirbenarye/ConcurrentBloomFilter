using BloomFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing.FNV
{
    public class FNV1A32HashFunction : Rehashable32Function
    {
        public override void HashBytes(byte[] data, int start, int count, ref uint Hash)
        {
            Hash = FNVHashAlgorithm.FNV32_1A(data, start, count);
        }

    }
}
