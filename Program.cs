using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;

using System.Linq;
using System.Threading;

namespace TextReview
{
    class Program
    {
        private static object ReadLocker = new object();
        /// <summary>
        /// Sorting params.
        /// </summary>
        public class TextParam
        {
            public string SourceText;
            public int SourceLength;
        }
        
        /// <summary>
        /// Spliting text by params.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IEnumerable<string> Triplets (string source, int length)
        {
            for (int i = length; i <= source.Length; i++)
                yield return source.Substring(i - length, length);
        }
        
        /// <summary>
        /// Sort spliting words by params.
        /// </summary>
        /// <param name="obj"></param>
        public static void SortedTriplets(object obj)
        {
            TextParam param = (TextParam) obj;
            
            var groups = Triplets(param.SourceText, param.SourceLength)
                .Where(str => str.All(ch => char.IsLetter(ch)))
                .GroupBy(str => str);

            Console.WriteLine(string.Join
            (
                ',',
                groups.OrderByDescending(gr => gr.Count()).Take(10)
                    .Select(gr => $"\"{gr.Key}\"({gr.Count()})")
            ));
            
        }
        
        static void Main(string[] args)
        {
            string text = null;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine("Введите путь к файлу");
            string textPath = Console.ReadLine();

            lock (ReadLocker)
            {
                try
                {
                 text = System.IO.File.ReadAllText(textPath);
                }
                catch
                {
                    throw new ArgumentNullException(text,
                    "Текст не считан. Проверьте местонахождение файла");
                }
            }

            TextParam textParam = new TextParam();
            textParam.SourceLength = 3;
            textParam.SourceText = text;
            
            Thread sortThread = new Thread(new ParameterizedThreadStart(SortedTriplets));
            sortThread.Start(textParam);
            sortThread.Join();
            
            stopwatch.Stop();
            Console.WriteLine($"Время выполнения программы - {stopwatch.ElapsedMilliseconds}");
            Console.ReadLine();
        }
    }
}