# ConcurrentBloomFilter
A concurrent implementation of the bloom filter data structure for the .net 4 framework. written in c#

You can learn more about bloom filter from here: https://en.wikipedia.org/wiki/Bloom_filter

Features:
  - Use the default hash function or your own hash function.
  - Completly concurrent implementation. thread safe and fast !
  - Very easy to use and understand
  - Contains many constructors for different uses
  - The structure uses generics and Can work for any .net object
  - Calculate the current probability of a false positive , based on the total items inserted to the filter.
  
  How to use it :
  
            ConcurrentBloomFilter<Object> filter = ConcurrentBloomFilter<Object>.CreateWithProbabillity(100,0.1);
            filter.AddItem(testString);
            filter.PossiblyContains(testString);
            Console.WriteLine(filter.ErrorProbability);
