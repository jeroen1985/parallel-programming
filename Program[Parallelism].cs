using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parallelism
{
    class Program
    {
        delegate int del(int x);

        public static ConcurrentDictionary<int,int> DictConcurrent = new ConcurrentDictionary<int, int>();

        public static ConcurrentDictionary<int, int> DictConcurrentCopy1;

        public static ConcurrentDictionary<int, int> DictConcurrentCopy2;

        static void Main(string[] args)
        {
            var stopWatch = Stopwatch.StartNew();

            del randNumber = (x) => x * new Random().Next(1, 900000);

            //Data parallelism
            //Loading data in to the dictionary, we dont need a concurrent dict for this (but we will need this later in the app)
            Parallel.For(1, 1000, i =>
            {
                int randomNumber = randNumber(i);
                if (DictConcurrent.TryAdd(i, randomNumber))
                {
                    Console.WriteLine("Added <{0},{1}> on thread {2}", i, randomNumber, Thread.CurrentThread.ManagedThreadId);
                    for (int z = 0; z < 5000; z++)
                    {
                        Console.WriteLine(z);
                        //just keeping busy 
                    }
                }
                else
                {
                    Console.WriteLine("Failed to add <{0},{1}> on thread {2}", i, randomNumber, Thread.CurrentThread.ManagedThreadId);
                }
            });

            DictConcurrentCopy1 = new ConcurrentDictionary<int, int>(DictConcurrent);
            DictConcurrentCopy2 = new ConcurrentDictionary<int, int>(DictConcurrent);

            long initLoadingTime = stopWatch.ElapsedMilliseconds;
 
            // two parallel independent tasks
            Task task1 = new Task(() =>
            {
                for (int i = 1; i < 1000; i++)
                {
                    int orginalValue;
                    if (DictConcurrentCopy1.TryGetValue(i, out orginalValue))
                    {
                        DictConcurrentCopy1[i] = orginalValue + 5;
                    }
                    for (int z = 0; z < 5000; z++)
                    {
                        Console.WriteLine(z);
                        //just keeping busy 
                    }
                }
            });

            Task task2 = new Task(() =>
            {
                for (int i = 1; i < 1000; i++)
                {
                    int orginalValue;
                    if (DictConcurrentCopy2.TryGetValue(i, out orginalValue))
                    {
                        DictConcurrentCopy2[i] = orginalValue - 10;

                    }
                    for (int z = 0; z < 5000; z++)
                    {
                        Console.WriteLine(z);
                        //just keeping busy 
                    }
                }
            });

            task1.Start();
            task2.Start();

            try
            {
                Task.WaitAll(task1, task2);
            }
            catch (AggregateException ex)
            {
                ex.Handle((x) =>
                {
                    //enumerate the exceptions  
                    if (x is KeyNotFoundException)
                    {
                        Console.WriteLine("Exception type {0} from {1}", x.GetType(), x.Source);
                        return true;
                    }
                    return false;
                });
            }

            FinalCheck();

            stopWatch.Stop();
            Console.WriteLine("Time Elapsed (Initial Loading) {0}", initLoadingTime);
            Console.WriteLine("Time Elapsed (Calculations) {0}", stopWatch.ElapsedMilliseconds - initLoadingTime);
            Console.WriteLine("Time Elapsed (Total) {0}", stopWatch.ElapsedMilliseconds);
            Console.ReadKey();
        }

        private static void FinalCheck()
        {
            foreach (KeyValuePair<int, int> itemOriginal in DictConcurrent)
            {
                if (DictConcurrentCopy1[itemOriginal.Key] != (itemOriginal.Value + 5))
                {
                    Console.WriteLine("Check failed at: {0},{1}", itemOriginal.Key, DictConcurrentCopy1[itemOriginal.Key]);
                    return;
                }
            }
            Console.WriteLine("DictConcurrentCopy1 - Values are as expected");
            Console.WriteLine("DictConcurrentCopy1 - Check passed count original: {0} count updated: {1}", DictConcurrentCopy1.Count, DictConcurrent.Count);

            foreach (KeyValuePair<int, int> itemOriginal in DictConcurrent)
            {
                if (DictConcurrentCopy2[itemOriginal.Key] != (itemOriginal.Value - 10))
                {
                    Console.WriteLine("Check failed at: {0},{1}", itemOriginal.Key, DictConcurrentCopy2[itemOriginal.Key]);
                    return;
                }
            }
            Console.WriteLine("DictConcurrentCopy2 - Values are as expected");
            Console.WriteLine("DictConcurrentCopy2 - Check passed count original: {0} count updated: {1}", DictConcurrentCopy2.Count, DictConcurrent.Count);
        }
    }
}
