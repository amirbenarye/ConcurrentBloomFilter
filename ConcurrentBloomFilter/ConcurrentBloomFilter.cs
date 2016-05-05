using BloomFilter.Hashing;
using BloomFilter.Hashing.FNV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// A probebalistic memory efficient data structure.
    /// this implementation is thread safe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentBloomFilter<T>
    {
        /// <summary>
        /// creates an instance of the concurrent bloom filter
        /// </summary>
        /// <param name="capacity">The total amount of elements expected in the bloom filter</param>
        /// <param name="falsePositiveProbability">The desired probability to get a false positive</param>
        /// <returns></returns>
        public static ConcurrentBloomFilter<T> CreateWithProbabillity(int capacity,double falsePositiveProbability)
        {
            ulong totalBits = getTotalBits(capacity, falsePositiveProbability);
            double bitsPerElement = ((double)totalBits)/(double)capacity;
            int hashNumber= (int)( bitsPerElement*OptimalHashFunctionCount);
            return new ConcurrentBloomFilter<T>(totalBits,GenerateDefaultHash(),hashNumber);
        }
        /// <summary>
        /// instanciate a new bloom filter with an intented capacity and with specified number of bits per element
        /// the maximum allowed bits for this implementation is 2^32. so capacity*bitsPerElement must be under 2^32.
        /// a chunk of memory the size of capacity*bitsPerElement is allocated imidiatly after calling this constructor. 
        /// this constructor creates a bloom filter with FNV-1A hashing and calculates optimal hash count
        /// </summary>
        /// <param name="capacity">the intended amount of elements for this bloom filter. all memory is allocated imidiatly</param>
        /// <param name="bitsPerElement">the number of bits for each element of this bloom filter</param>       
        public ConcurrentBloomFilter(int capacity, int bitsPerElement)
            : this(capacity, bitsPerElement, (int)(bitsPerElement * OptimalHashFunctionCount))      
        {

        }
        /// <summary>
        /// instanciate a new bloom filter with an intented capacity and with specified number of bits per element
        /// the maximum allowed bits for this implementation is 2^32. so capacity*bitsPerElement must be under 2^32.
        /// a chunk of memory the size of capacity*bitsPerElement is allocated imidiatly after calling this constructor. 
        /// this constructor creates a bloom filter with FNV-1A hashing
        /// </summary>
        /// <param name="capacity">the intended amount of elements for this bloom filter. all memory is allocated imidiatly</param>
        /// <param name="bitsPerElement">the number of bits for each element of this bloom filter</param>
        /// <param name="totalHashes">the number of hashes for each opertation in this filter.</param>
        public ConcurrentBloomFilter(int capacity, int bitsPerElement, int totalHahses)
            :this(capacity,bitsPerElement,GenerateDefaultHash(),totalHahses)
        {

        }
        /// <summary>
        /// instanciate a new bloom filter with an intented capacity and with specified number of bits per element
        /// the maximum allowed bits for this implementation is 2^32. so capacity*bitsPerElement must be under 2^32.
        /// a chunk of memory the size of capacity*bitsPerElement is allocated imidiatly after calling this constructor.
        /// </summary>
        /// <param name="capacity">the intended amount of elements for this bloom filter. all memory is allocated imidiatly</param>
        /// <param name="bitsPerElement">the number of bits for each element of this bloom filter</param>
        /// <param name="hashFunction">The hash function used by this bloom filter</param>
        /// <param name="totalHashes">the number of hashes for each opertation in this filter. this value
        /// must be lower than or equal to hashFunction.HashCount</param>
        public ConcurrentBloomFilter(int capacity,int bitsPerElement,IKHashFunction<uint> hashFunction,int totalHashes)
            : this( ((ulong)capacity)*((ulong)bitsPerElement),hashFunction,totalHashes)
        {

        }

        /// <summary>
        /// instanciate a new bloom filter with a the specified bit count
        /// the maximum allowed bits for this implementation is 2^32. 
        /// a chunk of memory the size of bitCount is allocated imidiatly after calling this constructor.
        /// </summary>
        /// <param name="bitCount">the amount of bits used by this bloom filter. all memory is allocated imidiatly</param>
        /// <param name="hashFunction">The hash function used by this bloom filter</param>
        /// <param name="totalHashes">the number of hashes for each opertation in this filter. this value
        /// must be lower than or equal to hashFunction.HashCount</param>
        public ConcurrentBloomFilter(ulong bitCount,IKHashFunction<uint> hashFunction,int totalHashes)
        {
            if (hashFunction == null)
                hashFunction = new RehashKHashFunction<uint>(new FNV1A32HashFunction());    // the default hash function is FNV1A
            if (totalHashes > hashFunction.HashCount)
                throw new IndexOutOfRangeException("totalHashes is to large for the provided hashFunction");
            mHashes = totalHashes;
            mHashFunction = hashFunction;
            if(bitCount > uint.MaxValue)
                throw new OutOfMemoryException("This implementation of bloom's filter is limited to a max of 2^32 bits");
            mBitCount = (uint)bitCount;
            uint dataSize = (mBitCount / BitsPerArrayItem) + 1;
            mData = new int[dataSize];
        }
        /// <summary>
        /// will add the given item to the bloom filter.
        /// this implementation is thread safe. however changes made to the filter in one thread my not be imidiatly available to other threads due to multiproccessor caching
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(T item)
        {
            ValidateHashArray();

            int value = item.GetHashCode(); // for ease of implemnetation. we use GetHashCode as an initial hash for the item
            BitWriter.WriteBits(value, mWorkBytes, 0);  // write the hash to a byte array for further proccessing
            mHashFunction.HashBytes(mWorkBytes, 0, 4, mHashes, mHashArray, 0);  //hash the first hash and generate k diffrent hashes into mHashArray

            for (int i = 0; i < mHashes; i++)   // set all relevant bits of the bloom filter
            {
                uint hash = mHashArray[i] % mBitCount;
                SetBit(hash);
            }

            Interlocked.Increment(ref mCount);  // Increment the count for the purpose of error calculation
        }
        /// <summary>
        /// total bits used by this bloom filter
        /// </summary>
        public uint BitCount
        {
            get { return mBitCount; }
        }
        /// <summary>
        /// will validate that the hash array for the current thread is not null and that it can store enough hashes for this
        /// bloom filter. the hash array may be cleared at the end of the operation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // this line can be removed if compiling with an old .net framewrok below 4.5
        void ValidateHashArray()
        {
            if(mHashArray == null || mHashArray.Length < mHashes)
                mHashArray = new uint[mHashes];
            if (mWorkBytes == null)
                mWorkBytes = new byte[4];
        }
        /// <summary>
        /// applies a mask on an array item in an interlocked manner
        /// the mask is apllied using the | operator
        /// </summary>
        /// <param name="arrayIndex"></param>
        /// <param name="mask"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // this line can be removed if compiling with an old .net framewrok below 4.5
        void SetMask(uint arrayIndex,int mask)
        {
            int data, newData;
            do
            {
                data = mData[arrayIndex];
                if ((data & mask) == mask)  /// do not attempt to synchronize write if the selected mask is already set
                    return;
                newData = data | mask;
            }
            while (Interlocked.CompareExchange(ref mData[arrayIndex], newData, data) != data);
        }
        /// <summary>
        /// Will return true if all the bits of mask are set in the given array index
        /// </summary>
        /// <param name="arrayIndex">an index to the mData array</param>
        /// <param name="mask">the mask that should be checked</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  // this line can be removed if compiling with an old .net framewrok below 4.5
        bool GetMask(uint arrayIndex,int mask)
        {
            int data = mData[arrayIndex];
            if ((data & mask) == mask)  // return true if the mask is set
                return true;
            return false;
        }

        /// <summary>
        /// will get the value of the bit in the specified bit index
        /// </summary>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        bool GetBit(uint bitIndex)
        {
            uint arrayIndex = bitIndex / BitsPerArrayItem;
            int mask = (int)(bitIndex % BitsPerArrayItem);
            mask = 1 << mask;
            return GetMask(arrayIndex, mask);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitIndex"></param>
        void SetBit(uint bitIndex)
        {
            uint arrayIndex = bitIndex / BitsPerArrayItem;
            int mask = (int)(bitIndex % BitsPerArrayItem);
            mask = 1<<mask;
            SetMask(arrayIndex, mask);
        }
        /// <summary>
        /// Will return true if the bloom filter possibly contains the speicified element.
        /// false if the bloom filter defintly does not contain the element
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool PossiblyContains(T item)
        {
            ValidateHashArray();

            int value = item.GetHashCode(); // for ease of implemnetation. we use GetHashCode as an initial hash for the item
            BitWriter.WriteBits(value, mWorkBytes, 0);  // write the hash to a byte array for further proccessing
            mHashFunction.HashBytes(mWorkBytes, 0, 4, mHashes, mHashArray, 0);  //hash the first hash and generate k diffrent hashes into mHashArray

            for (int i = 0; i < mHashes; i++)   // set all relevant bits of the bloom filter
            {
                uint hash = mHashArray[i] % mBitCount;
                if (GetBit(hash) == false)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// will return the count of items added to the bloom filter
        /// </summary>
        public int Count
        {
            get { return mCount; }
        }
        /// <summary>
        /// will calculate the probabilty for a a false positive on this bloom filter
        /// </summary>
        /// <returns></returns>
        double CalcProbabillity()
        {   
            double m = BitCount;
            double k = mHashes;
            double n = mCount;
            double res = Math.Pow(1.0 -  Math.Pow(1.0 - (1.0 / m), k * n), k);
            return res;
        }

        /// <summary>
        /// will return the probiblity of PossiblyContains returning a false positive
        /// this implementation
        /// </summary>
        public double ErrorProbability
        {
            get
            {
                return CalcProbabillity();
            }
        }


        #region Members
        static double OptimalHashFunctionCount = Math.Log(2, Math.E);
        const int BitsPerArrayItem = 8 * 4; // mData is an int array so every item in it is 4*8 bits
        [ThreadStatic]
        static uint[] mHashArray;
        [ThreadStatic]
        static byte[] mWorkBytes;
        volatile int mCount = 0;

        uint mBitCount;
        int mHashes;
        int[] mData;
        IKHashFunction<uint> mHashFunction;
        #endregion
        #region Static Helper Methods
        static ulong getTotalBits(int capacity, double falsePositiveProbability)
        {
            if (falsePositiveProbability < 0.0 || falsePositiveProbability > 1.0)
                throw new ArgumentException("Probability must be in range [0,1]");
            double count = -capacity * Math.Log(falsePositiveProbability, Math.E) / (OptimalHashFunctionCount * OptimalHashFunctionCount);
            return (ulong)count;
        }
        static IKHashFunction<uint> GenerateDefaultHash()
        {
            return new RehashKHashFunction<uint>(new FNV1A32HashFunction());
        }
        #endregion
    }

}
