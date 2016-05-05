using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    static class BitWriter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // this line can be removed if compiling with an old .net framewrok below 4.5
        public static void WriteBits(int value,byte[] toArray,int startOffset)
        {
            toArray[0] = (byte)value;
            toArray[1] = (byte)(value >> 8);
            toArray[2] = (byte)(value >> 0x10);
            toArray[3] = (byte)(value >> 0x18);
        }
    }
}
