using System;
using System.Diagnostics;

namespace MultiThreading
{
    internal class Program
    {
        static object lockObject = new object();
        static int sum = 0;

        static void Main(string[] args)
        {
            int[] arraySizes = { 10000000, 100000, 1000000 };

            foreach (int size in arraySizes)
            {
                Console.WriteLine($"Array Size: {size}");

                int[] array = GenerateRandomArray(size);

                var sequentialResult = MeasureTime(() => SumSequential(array), "Sequential");

                var parallelResult = MeasureTime(() => SumParallel(array), "Parallel");

                var linqResult = MeasureTime(() => SumLinq(array), "LINQ");

                Console.WriteLine($"Sequential Sum: {sequentialResult}");
                Console.WriteLine($"Parallel Sum: {parallelResult}");
                Console.WriteLine($"LINQ Sum: {linqResult}");
                Console.WriteLine();
            }

            Console.ReadLine();
        }

        static int[] GenerateRandomArray(int size)
        {
            Random random = new Random();
            return Enumerable.Range(1, size).Select(_ => random.Next(1, 100)).ToArray();
        }

        static int SumSequential(int[] array)
        {
            return array.Sum();
        }

        static int SumLinq(int[] array)
        {
            return array.AsParallel().Sum();
        }

        static long SumParallel(int[] array)
        {
            sum = 0;
            int chunkSize = array.Length / Environment.ProcessorCount;

            Thread[] threads = new Thread[Environment.ProcessorCount];

            for (int i = 0; i < threads.Length; i++)
            {
                int start = i * chunkSize;
                int end = (i == threads.Length - 1) ? array.Length : (i + 1) * chunkSize;

                threads[i] = new Thread(() => PartialSum(array, start, end));
                threads[i].Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            return sum;
        }

        static void PartialSum(int[] array, int start, int end)
        {
            int partialSum = 0;

            for (int i = start; i < end; i++)
            {
                partialSum += array[i];
            }

            lock (lockObject)
            {
                sum += partialSum;
            }
        }

        static long MeasureTime(Func<long> func, string label)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            long sum = func.Invoke();
            stopwatch.Stop();
            Console.WriteLine($"{label} Time: {stopwatch.ElapsedMilliseconds} ms");
            return sum;
        }
    }
}