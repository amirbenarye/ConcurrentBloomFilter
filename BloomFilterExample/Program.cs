using BloomFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilterExample
{
    class Program
    {
        static void Main(string[] args)
        {
            String testString = "trueString";
            String falseString = "falseString";
            ConcurrentBloomFilter<Object> filter = ConcurrentBloomFilter<Object>.CreateWithProbabillity(100,0.1);
            filter.AddItem(testString);
            filter.PossiblyContains(testString);
            Console.WriteLine(filter.ErrorProbability);
            Console.WriteLine("Bloom filter takes " + filter.BitCount + " bits from memory");
            Console.WriteLine("Test for testString : " + filter.PossiblyContains(testString));
            Console.WriteLine("Test for falseString : " + filter.PossiblyContains(falseString));
            Console.ReadKey(true);
        }
    }
}
