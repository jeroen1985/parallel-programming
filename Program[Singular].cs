using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Singular
{
    class Program
    {
        delegate int del(int x);

        public static Dictionary<int, int> Dictionary = new Dictionary<int, int>();

        public static Dictionary<int, int> DictionaryCopy1;

        public static Dictionary<int, int> DictionaryCopy2;

        static void Main(string[] args)
        {
            var stopWatch = Stopwatch.StartNew();

            del randNumber = (x) => x * new Random().Next(1, 900000);

            for (int i = 1; i < 1000; i++)
            {
                int randomNumber = randNumber(i);
                Dictionary.Add(i, randomNumber);
                Console.WriteLine("Added <{0},{1}> on thread {2}", i, randomNumber, Thread.CurrentThread.ManagedThreadId);
                for (int z = 0; z < 5000; z++)
                {
                    Console.WriteLine(z);
                    //just keeping busy 
                }
            }

            DictionaryCopy1 = new Dictionary<int, int>(Dictionary);
            DictionaryCopy2 = new Dictionary<int, int>(Dictionary);

            long initLoadingTime = stopWatch.ElapsedMilliseconds;

            // two sequential independent task

            for (int i = 1; i < 1000; i++)
            {
                int orginalValue;
                if(DictionaryCopy1.TryGetValue(i, out orginalValue))
                {
                    DictionaryCopy1[i] = orginalValue + 5;
                }
                for (int z = 0; z < 5000; z++)
                {
                    Console.WriteLine(z);
                    //just keeping busy 
                }
            }

            for (int i = 1; i < 1000; i++)
            {
                int orginalValue;
                if (DictionaryCopy2.TryGetValue(i, out orginalValue))
                {
                    DictionaryCopy2[i] = orginalValue - 10;
                }
                for (int z = 0; z < 5000; z++)
                {
                    Console.WriteLine(z);
                    //just keeping busy 
                }
            }

            FinalCheck();

            stopWatch.Stop();
            Console.WriteLine("Time Elapsed (Initial Loading) {0}", initLoadingTime);
            Console.WriteLine("Time Elapsed (Calculations) {0}", stopWatch.ElapsedMilliseconds-initLoadingTime);
            Console.WriteLine("Time Elapsed (Total) {0}", stopWatch.ElapsedMilliseconds);
            Console.ReadKey();

        }

        private static void FinalCheck()
        {
            foreach (KeyValuePair<int,int> itemOriginal in Dictionary)
            {
                if(DictionaryCopy1[itemOriginal.Key] != (itemOriginal.Value + 5))
                {
                    Console.WriteLine("Check failed at: {0}", DictionaryCopy1[itemOriginal.Key]);
                    return;
                }
            }
            Console.WriteLine("DictionaryCopy1 - Values are as expected");
            Console.WriteLine("DictionaryCopy1 - Check passed count original: {0} count updated: {1}", Dictionary.Count, DictionaryCopy1.Count);

            foreach (KeyValuePair<int, int> itemOriginal in Dictionary)
            {
                if (DictionaryCopy2[itemOriginal.Key] != (itemOriginal.Value - 10))
                {
                    Console.WriteLine("Check failed at: {0}", DictionaryCopy2[itemOriginal.Key]);
                    return;
                }
            }
            Console.WriteLine("DictionaryCopy2 - Values are as expected");
            Console.WriteLine("DictionaryCopy2 - Check passed count original: {0} count updated: {1}", Dictionary.Count, DictionaryCopy2.Count);

        }

    }
}
